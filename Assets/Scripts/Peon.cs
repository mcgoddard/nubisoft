using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Peon : MonoBehaviour
{
    public enum State
    {
        Grouping,
        Wandering,
        Chasing,
        CarryingBunny,
        Sacrifice,
    }
    public LayerMask peonsLayer;
    public LayerMask bunniesLayer;
    public State state = State.Grouping;
    public List<Vector3> path;
    public bool move = false;
    private GameObject bunnyTarget;
    private GameObject altar;
    private FearController fearController;
    private UiUpdate uiUpdate;
    private Animator animator;
    private float stateChangeTimeout = STATE_CHANGE_TIMEOUT;
    private float randomDirectionTimeout = RANDOM_DIRECTION_TIMEOUT;
    private new Rigidbody2D rigidbody;
    private const float SPEED = 1f;
    private const float ROTATION_SPEED = 10f;
    private const float STATE_CHANGE_TIMEOUT = 15.0f;
    private const float MAP_SIZE = 18.0f;
    private const float RANDOM_DIRECTION_TIMEOUT = 10.0f;
    private const float GROUP_STANDOFF_DISTANCE = 0.5f;
    private const float CATCH_DISTANCE = 0.3f;
    private const float SACRIFICE_DISTANCE = 0.5f;
    private const float NEIGHBOUR_SEARCH_RADIUS = 2f;
    private const float BUNNY_SEARCH_RADIUS = 1f;
    private const float VOICE_LINE_TIMEOUT = 30f;
    private float timeToNextVoiceLine;
    private AudioSource audioSource;
    public AudioClip[] chantSamples;
    public AudioClip[] sacrificeSamples;
    public AudioClip[] threatSamples;
    public AudioClip[] confusedSamples;
    public AudioClip[] tetchySamples;
    public AudioClip[] hmmSamples;
    public AudioClip[] whistlingSamples;
    public AudioClip[] screamSamples;
    public GameObject bloodSplatDecal;


    // Start is called before the first frame update
    void Start()
    {
        uiUpdate = GameObject.Find("UI")?.GetComponent<UiUpdate>();
        fearController = GetComponent<FearController>();
        rigidbody = GetComponent<Rigidbody2D>();
        altar = GameObject.Find("Altar");
        animator = this.GetComponent<Animator>();
        audioSource = this.GetComponent<AudioSource>();
        timeToNextVoiceLine = plusMinusPercent(VOICE_LINE_TIMEOUT, 100);

        if (Random.value < 0.5) {
            state = State.Grouping;
        } else {
            // If we're wandering pick a random direction
            WanderAimlessly();
        }
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D[] neighbours = GetNeighbours();
        Collider2D[] bunnies = GetBunnies();

        if (
            (fearController.IsSufficientlyScaredToSacrifice() || fearController.IsTerrified()) &&
            state != State.Sacrifice &&
            state != State.CarryingBunny &&
            bunnies.Length > 0
        ) {
            if (bunnyTarget == null)
                bunnyTarget = bunnies[0]?.gameObject;
            SetState(State.Chasing);
            SetMoving(true);
        }

        switch (state) {
            case State.Grouping:
                // If the group is inadequate (too small or large), the Peon has been there for some time and a random change is hit
                // or all neighbours have already left which to randomly wandering
                if (((stateChangeTimeout < 0.0f || neighbours.Length < 3 || neighbours.Length > 4) && 
                    Random.value < (0.1 * Time.deltaTime)) || neighbours.Length == 1) {
                    WanderAimlessly();
                } else {
                    // We should stay away from the alter where people are sacrificing  bunnies. Not a pleasant place to hang out.
                    if (neighbours.Any(neighbour => neighbour.GetComponent<Peon>()?.state == State.Sacrifice)) {
                        WanderAimlessly();
                    } else {
                        // Otherwise stay centred on the group
                        SetGroupTarget(neighbours);
                    }
                }
                break;
            case State.Chasing:
                // Check our target hasn't been picked up by another peon
                if (bunnyTarget == null) {
                    WanderAimlessly();
                } else if ((bunnyTarget.transform.position - transform.position).magnitude < CATCH_DISTANCE) {
                    // If we've caught it update our state and destroy the bunny object
                    GameObject.Destroy(bunnyTarget);
                    if (fearController.IsTerrified()) {
                        // Just kill it and go back to wandering
                        uiUpdate.kills += 1;
                        KillBunny();
                        SetState(State.Wandering);
                    } else {
                        this.animator.SetTrigger("CarryingBunny");
                        SetState(State.CarryingBunny);
                    }
                } else {
                    // Otherwise keep up the chase
                    path = new[] { bunnyTarget.transform.position }.ToList();
                }
                break;
            case State.CarryingBunny:
                // If we've reached the alter start sacrificing
                if (altar.GetComponent<Renderer>().bounds.Contains(transform.position)) {
                    SetState(State.Sacrifice);
                    SetMoving(false);
                    this.animator.SetTrigger("DroppedBunny");
                    this.animator.SetBool("Moving", false);
                    this.animator.SetBool("Sacrifice", true);
                    this.stateChangeTimeout = 10f; // 10 seconds to perform sacrifice
               } else if(fearController.ShouldDropBunny()) {
                   // We either calmed down too much or walk past a bunch of terrified people to the point that we forget what we were doing
                   this.animator.SetTrigger("DroppedBunny");
                   this.transform.parent.GetComponent<Spawner>()?.SpawnBunny(this.transform.position);
                   WanderAimlessly();
                } else {
                    // Otherwise keep aiming for the altar
                    GoToAltar();
                }
                break;
            case State.Sacrifice:
                // If we're done sacrificing go back to wandering
                if (stateChangeTimeout < 0) {
                    uiUpdate.sacrifices += 1;
                    uiUpdate.bunnies -= 1;
                    WanderAimlessly();
                    this.animator.SetBool("Sacrifice", false);
                }
                break;
            case State.Wandering:
            default:
                // We should stay away from the alter where people are sacrificing  bunnies. Not a pleasant place to hang out.
                if (
                    neighbours.Any(neighbour => neighbour.GetComponent<Peon>()?.state == State.Sacrifice) &&
                    CurrentTarget() == transform.position
                ) {
                    WanderAimlessly();
                }
                else if (stateChangeTimeout < 0 || CurrentDestination() == transform.position) {
                    if (neighbours.Length > 3)
                        SetState(State.Grouping);
                    else
                        WanderAimlessly();
                }
                break;
        }
        stateChangeTimeout -= Time.deltaTime;

        ShowPath(Color.green);
        UpdateAudioState();

        if (uiUpdate.fadingOut)
        {
            audioSource.mute = true;
        }
    }

    void ShowPath(Color color, float duration = 0f) {
        var current = transform.position;
        for (int i = 0; i < path.Count; ++i) {
            var next = path[i];
            Debug.DrawLine(current, next, color, duration, false);
            current = next;
        }
    }

    void OnMouseDown() {
        CursorManager.Instance.SetCursor(CursorManager.CursorType.Click, force: true);
    }

    void OnMouseEnter() {
        CursorManager.Instance.SetCursor(CursorManager.CursorType.Hover, force: true, looping: true);
    }

    void OnMouseExit() {
        CursorManager.Instance.ResetCursor();
    }

    // Apply rotation and velocity to the Peon based on their current state
    // This is done in Fixed Update so that we don't accidentally over-shoot our
    // waypoint if the framerate drops
    void FixedUpdate() {
        if (move) {
            Vector3 currentTarget = CurrentTarget();
            Vector3 vectorToTarget = currentTarget - transform.position;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * ROTATION_SPEED);
            rigidbody.velocity = transform.right * SPEED;
        } else {
            rigidbody.velocity = Vector3.zero;
        }
    }

    IEnumerator TemporarilyDisableCollision(Collider2D collider1, Collider2D collider2) {
        Physics2D.IgnoreCollision(collider1, collider2);
        yield return new WaitForSeconds(1f);
        Physics2D.IgnoreCollision(collider1, collider2, false);

    }
    void OnCollisionEnter2D(Collision2D collision) {
        if (state == State.Wandering)
        {
            var isPeon = collision.collider.GetComponent<Peon>() != null;
            if (isPeon) {
                StartCoroutine(TemporarilyDisableCollision(collision.otherCollider, collision.collider));
            }
        }
    }

    void UpdateAudioState() {
        switch (state)
        {
            case State.Sacrifice:
                TriggerSacrificeSamples();
                break;
            default:
                TriggerCasualAudio();
                break;
        }

        UpdateAudioSourceVolume();
    }

    void TriggerCasualAudio() {
        timeToNextVoiceLine -= Time.deltaTime;

        if (timeToNextVoiceLine >= 0 || Random.value < 0.90) return;

        timeToNextVoiceLine = plusMinusPercent(VOICE_LINE_TIMEOUT, 90);

        var neighbours = GetNeighbours();
        var bunnies = GetBunnies();

        if (fearController.IsTerrified() || state == State.Chasing && neighbours.Length > 0 && bunnies.Length > 0)  {
            audioSource.PlayOneShot(PickOne(threatSamples));
            return;
        }

        if (state == State.Wandering) {
            if (Random.value > 0.9) {
                audioSource.PlayOneShot(PickOne(whistlingSamples));
            }
            return;
        }

        if (state == State.Grouping) {
            var hasTerrifiedNeigbour = neighbours.Any(neighbour => (bool)(neighbour.GetComponent<FearController>()?.IsTerrified()));
            var hasNeighbourChasing = neighbours.Any(neighbour => (bool)(neighbour.GetComponent<Peon>().state == State.Chasing));

            if (hasTerrifiedNeigbour)
            {
                audioSource.PlayOneShot(PickOne(tetchySamples));
            }
            else if (hasNeighbourChasing)
            {
                audioSource.PlayOneShot(PickOne(confusedSamples));
            }
            else
            {
                audioSource.PlayOneShot(PickOne(hmmSamples));
            }
        }

    }
     
    void TriggerSacrificeSamples() {
        if (!audioSource.isPlaying) {
            var chantSample = PickOne(chantSamples);

            this.audioSource.PlayOneShot(chantSample);
            StartCoroutine(PlaySacrificeSounds());
        }
    }

    void KillBunny() {
        var sacrificeSample = PickOne(sacrificeSamples);
        this.audioSource.PlayOneShot(sacrificeSample);
        var position = transform.position + (transform.right / 3) + Random.insideUnitSphere / 3;
        position.z = transform.position.z;
        var rotation = Random.value;
        Instantiate(bloodSplatDecal, position, Quaternion.AngleAxis(rotation, Vector3.forward));
    }

    IEnumerator PlaySacrificeSounds() {
        // We _may_ have stopped sacrifing since we triggered this co-routine
        while (state == State.Sacrifice) {
            yield return new WaitForSeconds(Random.Range(1f, 5.0f));
            KillBunny();
        }
    }

    void UpdateAudioSourceVolume() {
        if (!audioSource.isPlaying) return;

        if (!GetComponent<Renderer>().isVisible) {
            this.audioSource.volume = 0;
            return;
        }

        var viewportCenter = new Vector3(0.5f, 0.5f, -Camera.main.transform.position.z);
        var viewportCoords = Camera.main.WorldToViewportPoint(transform.position);
        var distanceFromCenter = (viewportCoords - viewportCenter).magnitude;
        var volumeFromScreenSpace = 1.0f - (distanceFromCenter * 2);

        var volumeFromZoom = Mathf.Max(0.1f, 1.0f - Camera.main.GetComponent<CameraControls>().GetZoomLevel());

        // We need to clamp, because part of the sprite be visble on screen but the centre position is offscreen
        this.audioSource.volume = Mathf.Clamp(volumeFromZoom * volumeFromScreenSpace, 0.0f, 1.0f);
    }

    // Move towards the centrepoint of your neighbours, but don't get too close to the centre
    void SetGroupTarget(Collider2D[] neighbours) {
        var groupingNeighbours = neighbours.Where(collider => collider.GetComponent<Peon>()?.state == State.Grouping);
        var total = groupingNeighbours.Aggregate(new Vector3(), (a, n) => n.gameObject.transform.position + a);

        Vector3 centrePoint = total/groupingNeighbours.Count();
        var shouldMove = !((centrePoint - transform.position).magnitude < GROUP_STANDOFF_DISTANCE);

        SetMoving(shouldMove);

        if (shouldMove) {
            // If we're not already headed towards the centre, path-find to it
            if (Vector3.Distance(centrePoint, CurrentDestination()) >= GROUP_STANDOFF_DISTANCE) {
                path = Pathfinding.Instance.FindPath(transform.position, centrePoint);
            }
        } else {
            // If we're in the group just look towards the centre
            this.transform.right = (centrePoint - this.transform.position).normalized;
        }
    }

    void WanderAimlessly() {
        path = Pathfinding.Instance.RandomTarget(this.transform.position, 0.2f);
        SetMoving(true);
        SetState(State.Wandering);
    }

    // Get the colliders for nearby Peons (your neighbours)
    Collider2D[] GetNeighbours() {
        return Physics2D.OverlapCircleAll(transform.position, NEIGHBOUR_SEARCH_RADIUS, peonsLayer);
    }

    // Get the colliders for nearby Bunnies
    Collider2D[] GetBunnies() {
        return Physics2D.OverlapCircleAll(transform.position, BUNNY_SEARCH_RADIUS, bunniesLayer);
    }

    // Change the Peons state and reset the timer before it will consider another state change
    void SetState(State newState) {
        state = newState;
        stateChangeTimeout = plusMinusPercent(STATE_CHANGE_TIMEOUT, 20);
    }

    // Get a random value that is + or - a percentage of the original value
    float plusMinusPercent(float original, float percent) {
        float twentyPercent = original * ((percent * 2) / 100);
        return original + (Random.value * twentyPercent) - (twentyPercent / 2.0f);
    }

    void SetMoving(bool move) {
        this.move = move;
        this.animator.SetBool("Moving", move);
    }

    T PickOne<T>(T[] array) {
        return array[Random.Range(0, array.Length)];
    }

    private Vector3 CurrentTarget() {
        // No where to go, stand still.
        if (path.Count == 0) {
            return transform.position;
        }

        var current = path.First();

        // If we're close enough to the next point in our path, just start
        // heading to the next point. If it's our last waypoint we return
        // our current position so that our caller can perfrom strict equality
        // to know we've arrived.
        if (Vector3.Distance(current, transform.position) < 0.1f) {
            path.RemoveAt(0);
            return path.Count > 0 ? path.First() : transform.position;
        }

        // Otherwise, carry on!
        return current;
    }

    private Vector3 CurrentDestination() {
        // No where to go, stand still.
        if (path.Count == 0) {
            return transform.position;
        }

        return path.Last();
    }

    private void GoToAltar() {
        if (path.Count == 0 || path.Last() != altar.transform.position) {
            path = Pathfinding.Instance.FindPath(transform.position, altar.transform.position, 0.2f);
        }
    }
}
