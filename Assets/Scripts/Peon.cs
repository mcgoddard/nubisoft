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
    public Vector2 target;
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
    private const float ROTATION_SPEED = 1f;
    private const float STATE_CHANGE_TIMEOUT = 15.0f;
    private const float MAP_SIZE = 20.0f;
    private const float RANDOM_DIRECTION_TIMEOUT = 10.0f;
    private const float GROUP_STANDOFF_DISTANCE = 0.5f;
    private const float CATCH_DISTANCE = 0.3f;
    private const float SACRIFICE_DISTANCE = 0.5f;
    private const float NEIGHBOUR_SEARCH_RADIUS = 2f;
    private const float BUNNY_SEARCH_RADIUS = 1f;
    private AudioSource audioSource;
    public AudioClip[] chantSamples;

    // Start is called before the first frame update
    void Awake()
    {
        uiUpdate = GameObject.Find("UI")?.GetComponent<UiUpdate>();
        fearController = GetComponent<FearController>();
        rigidbody = GetComponent<Rigidbody2D>();
        altar = GameObject.Find("Altar");
        animator = this.GetComponent<Animator>();
        audioSource = this.GetComponent<AudioSource>();

        if (Random.value < 0.5) {
            state = State.Grouping;
        } else {
            // If we're wandering pick a random direction
            state = State.Wandering;
            target = (Vector2)transform.position + new Vector2((Random.value * 10f) - 5f, (Random.value * 10f) - 5f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D[] neighbours = GetNeighbours();
        Collider2D[] bunnies = GetBunnies();

        switch (state) {
            case State.Grouping:
                // If the group is inadequate (too small or large), the Peon has been there for some time and a random change is hit
                // or all neighbours have already left which to randomly wandering
                if (((stateChangeTimeout < 0.0f || neighbours.Length < 3 || neighbours.Length > 4) && 
                    Random.value < (0.1 * Time.deltaTime)) || neighbours.Length == 1) {
                    SetRandomTarget();
                    SetState(State.Wandering);
                } else {
                    // Otherwise stay centred on the group
                    SetGroupTarget(neighbours);
                }
                break;
            case State.Chasing:
                // Check our target hasn't been picked up by another peon
                if (bunnyTarget == null) {
                    SetRandomTarget();
                    SetState(State.Wandering);
                } else if ((bunnyTarget.transform.position - transform.position).magnitude < CATCH_DISTANCE) {
                    // If we've caught it update our state and destroy the bunny object
                    GameObject.Destroy(bunnyTarget);
                    if (fearController.IsTerrified()) {
                        // Just kill it and go back to wandering
                        uiUpdate.kills += 1;
                        SetState(State.Wandering);
                    } else {
                        this.animator.SetTrigger("CarryingBunny");
                        SetState(State.CarryingBunny);
                    }
                } else {
                    // Otherwise keep up the chase
                    target = bunnyTarget.transform.position;
                }
                break;
            case State.CarryingBunny:
                // If we've reached the alter start sacrificing
                
                if (altar.GetComponent<Renderer>().bounds.Contains(transform.position)) {
                    SetState(State.Sacrifice);
                    this.animator.SetTrigger("DroppedBunny");
                    this.animator.SetBool("Moving", false);
                    this.stateChangeTimeout = 30f; // 30 seconds to perform sacrifice
               } else if(fearController.ShouldDropBunny()) {
                   // We either calmed down too much or walk past a bunch of terrified people to the point that we forget what we were doing
                   this.animator.SetTrigger("DroppedBunny");
                   this.SetState(State.Wandering);
                   this.transform.parent.GetComponent<Spawner>()?.SpawnBunny(this.transform.position);
                } else {
                    // Otherwise keep aiming for the altar
                    target = altar.transform.position;
                }
                break;
            case State.Sacrifice:
                // If we're done sacrificing go back to wandering
                if (stateChangeTimeout < 0) {
                    uiUpdate.sacrifices += 1;
                    uiUpdate.bunnies -= 1;
                    SetRandomTarget();
                    SetState(State.Wandering);
                    this.animator.SetTrigger("DroppedBunny");
                }
                break;
            case State.Wandering:
            default:
                // TODO should we actually chase a rabbit?
                if ((fearController.IsSufficientlyScaredToSacrifice() || fearController.IsTerrified()) && bunnies.Length > 0) {
                    bunnyTarget = bunnies[0]?.gameObject;
                    SetState(State.Chasing);
                }
                // We should stay away from the alter where people are sacrificing  bunnies. Not a pleasant place to hang out.
                else if (neighbours.Any(neighbour => neighbour.GetComponent<Peon>()?.state == State.Sacrifice)) {
                    SetAntiGroupTarget(neighbours);
                    SetState(State.Wandering);
                }
                // If we've got some neighbours and are ready to finish wandering form a group with those neighbours
                // This will result in following that neighbour if they aren't ready to group yet
                else if (stateChangeTimeout < 0 && neighbours.Length > 3) {
                    SetState(State.Grouping);
                } else {
                    // Otherwise should we change direction?
                    if (randomDirectionTimeout < 0.0f) {
                        // Perhaps turn slightly if we're walking alone
                        if (neighbours.Length == 1) {
                            target = target + new Vector2(Random.value * 2, Random.value * 2);
                            SetMoving(true);
                        } else {
                            // Or if there are people nearby walk away from their centrepoint
                            SetAntiGroupTarget(neighbours);
                        }
                        // Reset the direction change timeout (this prevents us constantly changing direction)
                        randomDirectionTimeout = plusMinusPercent(RANDOM_DIRECTION_TIMEOUT, 20);
                    } else {
                        // We're not going to change direction, so just ensure we haven't reached our target
                        randomDirectionTimeout -= Time.deltaTime;
                        target = (Vector2)transform.position + ((target - (Vector2)transform.position).normalized * 10f);
                        SetMoving(true);
                    }
                }
                break;
        }
        // Apply rotation and velocity to the Peon based on their current state
        Vector3 vectorToTarget = target - (Vector2)transform.position;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * ROTATION_SPEED);
        if (move) {
            rigidbody.velocity = vectorToTarget.normalized * SPEED;
        } else {
            rigidbody.velocity = new Vector2(0, 0);
        }
        stateChangeTimeout -= Time.deltaTime;

        if (state == State.Sacrifice && !audioSource.isPlaying && chantSamples.Length != 0) {
            var sampleIndex = Random.Range(0, chantSamples.Length);
            this.audioSource.PlayOneShot(chantSamples[sampleIndex]);
        }

        UpdateAudioSourceVolume();
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

        Debug.LogFormat("viewportCenter: {3}, viewPortCoords: {0}, distanceFromCenter: {1}, volumeFromScreenSpace: {2}",
            viewportCoords,
            distanceFromCenter,
            volumeFromScreenSpace,
            viewportCenter
        );


        var volumeFromZoom = Mathf.Max(0.1f, 1.0f - Camera.main.GetComponent<CameraControls>().GetZoomLevel());

        Debug.LogFormat("volumeFromZoom: {0}, volumeFromScreenSpace: {1}", volumeFromZoom, volumeFromScreenSpace);
        // We need to clamp, because part of the sprite be visble on screen but the centre position is offscreen
        // We also make the at least remotely audible if they are on screen
        this.audioSource.volume = Mathf.Clamp(volumeFromZoom * volumeFromScreenSpace, 0.05f, 1.0f);
    }

    // Move towards the centrepoint of your neighbours, but don't get too close to the centre
    void SetGroupTarget(Collider2D[] neighbours) {
        Vector2 total = neighbours.Aggregate(new Vector3(), (a, n) => n.gameObject.transform.position + a);
        Vector2 centrePoint = total/neighbours.Length;
        SetMoving(!((centrePoint - (Vector2)transform.position).magnitude < GROUP_STANDOFF_DISTANCE));
        target = centrePoint;
    }

    // Move away from the centrepoint of your neighbours
    void SetAntiGroupTarget(Collider2D[] neighbours) {
        Vector2 total = neighbours.Aggregate(new Vector3(), (a, n) => n.gameObject.transform.position + a);
        Vector2 centrePoint = total/neighbours.Length;
        var direction = (centrePoint - (Vector2)transform.position) * -10.0f;
        SetMoving(true);
        target = direction;
    }

    // Set a random target location
    void SetRandomTarget() {
        target = (Vector2)transform.position + new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
        SetMoving(true);
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
}
