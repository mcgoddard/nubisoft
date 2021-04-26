using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public Bounds Bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(10, 10, 0));
    public float NodeSeparation = 0.5f;
    public bool RenderPath = false;
    private float nodeSeparation;
    private Bounds bounds;
    private int boundaryLayer;
    private List<Vector3> nodes = new List<Vector3>();
    private Dictionary<Vector3, List<Vector3>> neighbours;
    private bool renderingMarkers;

    private static Pathfinding _instance;
    public static Pathfinding Instance { get { return _instance; } }

    void Awake()
    {
        // Maintain if a new instance is created
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
    }

    void Start()
    {
        boundaryLayer = 1 << LayerMask.NameToLayer("Boundary");
        bounds = Bounds;
        nodeSeparation = NodeSeparation;
        GenerateGraph();
    }

    void GenerateGraph() {
        nodes = new List<Vector3>();
        
        var halfMaxSideLength = Mathf.Max(bounds.extents.x, bounds.extents.y);
        if (nodeSeparation / halfMaxSideLength <= 0.01f) {
            Debug.Log("Node Separation too low, ignoring");
            return;
        }

        for (float x = 0; x < bounds.max.x * 2; x += nodeSeparation)
        {
            for (float y = 0; y < bounds.max.y * 2; y += nodeSeparation)
            {
                var position = bounds.min + new Vector3(x, y, 0);
                if (Physics2D.OverlapCircle(position, 0.2f, boundaryLayer) == null)
                {
                    nodes.Add(position);
                }
            }
        }

        neighbours = nodes.ToDictionary(node => node, FindNeigbours);
    }

    void Update()
    {
        if (NodeSeparation != nodeSeparation || Bounds != bounds) {
            nodeSeparation = NodeSeparation;
            bounds = Bounds;
            GenerateGraph();
        }

        if (RenderPath)
        {
            foreach (var item in neighbours)
            {
                if (item.Value.Count > 8) {
                    Debug.LogWarning("There should be no more that 8 neigbours for a node in base graph");
                }
                item.Value.ForEach(neighbour => Debug.DrawLine(item.Key, neighbour, Color.grey));
            }
        }
    }

    // Searching neighbours that are < (nodeSeparation * 1.8) will definitely select diagonal neighours, but
    // not those that are next-but-one over from an actual neighbour
    List<Vector3> FindNeigbours(Vector3 point)
    {
        return nodes.FindAll(neighbour => Vector3.Distance(neighbour, point) <= nodeSeparation * 1.8  && neighbour != point);
    }

    public List<Vector3> FindPath(Vector3 from, Vector3 to, float perturbBy = 0.0f)
    {
        if (!Physics2D.Raycast(from, to - from, Vector3.Distance(from, to), boundaryLayer))
        {
            Debug.Log("Have direct line of sight to target, heading straight there!");
            return new[] { to }.ToList();
        }

        // Create a clone of the neighbours dictionary as we are going to modify it
        var neighbours = new Dictionary<Vector3, List<Vector3>>(this.neighbours);

        // Find the nearest pathfinding nodes to the start position and add them
        // neighbours dictionary for the start node
        neighbours.Add(from, FindNeigbours(from));

        // Find the nearest pathfinding nodes to the end position and it to their
        // list of neighbours
        FindNeigbours(to).ForEach(neighbour => {
            // Copy the list of neighbours as this is still a reference to the list in the base map
            var copy = new List<Vector3>(neighbours[neighbour]);
            copy.Add(to);
            neighbours[neighbour] = copy;
        });

        // Stores the best previous node to get to a node, we use this
        // to build the route back to the start.
        var cameFrom = new Dictionary<Vector3, Vector3>(nodes.Count);

        // Stores the cost of getting to to a node from the start position.
        // It's just a memoized value, really. It doesn't take into account
        // whether this actually gets us closer to the end.
        var costFromStartToNode = new Dictionary<Vector3, float>();
        costFromStartToNode.Add(from, 0);

        // Stores the cost getting to a node + the estimated cost of getting to
        // the finish from that node (i.e. this is what we want to minimise)
        var estimatedTotalCostForNode = new Dictionary<Vector3, float>();
        estimatedTotalCostForNode.Add(from, Cost(from, to));

        // These the candidates for the next node with the corresponding fScores
        // In the loop we start node that has the lowest/best fScore
        var candidateNodes = new List<(Vector3, float)>(nodes.Count);
        candidateNodes.Add((from, Mathf.Infinity));

        while (candidateNodes.Count != 0)
        {
            // candidate nodes are sorted by total estimated cost so we try the 
            // cheapest one first
            var (current, _) = candidateNodes.First();
            candidateNodes.RemoveAt(0);

            // We've reached our target so start going backwards looking up the best
            // previous node in the cameFrom store
            if (current == to)
            {
                var path = new List<Vector3>();
                path.Add(current);
                var previousAngle = Mathf.Infinity;
                while (cameFrom.TryGetValue(current, out current))
                {
                    var currentAngle = Vector3.Angle(Vector3.right, current - path[0]);
                    if (currentAngle != previousAngle) {
                        path.Insert(0, current);
                    }
                    previousAngle = currentAngle;
                }
                return Perturb(path, perturbBy);
            }

            foreach (var neighbour in neighbours[current])
            {
                // The the cost to get to this neighbour, via the current node, is the current nodes cost
                // plus the distance from the current node to the neighbour.
                var costViaCurrentNode = GetOrInfinity(costFromStartToNode, current) + Vector3.Distance(current, neighbour);

                // If this cost is cheaper than any existing cost, then we overwrite it.
                if (costViaCurrentNode < GetOrInfinity(costFromStartToNode, neighbour))
                {
                    // Update a current buest guesses
                    cameFrom[neighbour] = current;
                    costFromStartToNode[neighbour] = costViaCurrentNode;

                    // Estimated total cost is the cost to get to this node, plus an estimate
                    // for how much "effort" it takes to get to the finish from this node.
                    var estimatedTotalCost = costViaCurrentNode + Cost(neighbour, to);
                    estimatedTotalCostForNode[neighbour] = estimatedTotalCost;


                    // Add/update the candidates estimated score
                    var candidateIndex = candidateNodes.FindIndex((entry) => entry.Item1 == neighbour);
                    var newCandidate = (neighbour, estimatedTotalCost);

                    // If the new candidate is not in candidate list, add it
                    if (candidateIndex == -1)
                        candidateNodes.Add(newCandidate);
                    // Otherwise if it's score is lower, overwrite it.
                    else if (candidateNodes[candidateIndex].Item2 < estimatedTotalCost)
                        candidateNodes[candidateIndex] = newCandidate;
                }
            }

            // Resort the candidates based on their totalEstitamedScore so we pick the smallest one next time
            // TODO try min heap if this is too costly, I don't think is though, as we'd still be doing a scan to see if a neighour exists in anyway
            candidateNodes.Sort((first, second) => first.Item2.CompareTo(second.Item2));
        }

        return new List<Vector3>();
    }

    // Randomly shift the waypoint nodes around so that units don't just follow the same track all the time.
    public List<Vector3> Perturb(List<Vector3> path, float amount) {
        for (int i = 0; i < path.Count - 1; ++i) {
            var newPosition = (Vector2)path[i] + Random.insideUnitCircle * amount;
            var collider = Physics2D.OverlapCircle(newPosition, 0.2f, boundaryLayer);
            // If the new position (+ peon radius) does not overlap a boundary, we accept the new position
            if (collider == null) {
                path[i] = newPosition;
            }
        }
        return path;
    }

    public List<Vector3> RandomTarget(Vector3 from, float perturbBy = 0.0f) {
        var to = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), 0);
        while(Physics2D.OverlapCircle(to, 0.2f, boundaryLayer) != null) {
            Debug.Log("Targeted a boundary collider, trying gain");
            to = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), 0);
        }

        return FindPath(from, to, perturbBy);
    }


    private float Cost(Vector3 from, Vector3 to)
    {
        // TODO perhaps this could be weighted on if there is a collider in the way and how close we are to it
        return Vector3.Distance(from, to);
    }

    private static float GetOrInfinity(Dictionary<Vector3, float> dict, Vector3 key)
    {
        float value;
        if (dict.TryGetValue(key, out value))
        {
            return value;
        }

        return Mathf.Infinity;
    }
}
