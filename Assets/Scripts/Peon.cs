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
    public bool move = true;
    public float fear;
    private GameObject bunnyTarget;
    private GameObject altar;
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
    private const float FEAR_MAX = 1.0f;
    private const float FEAR_MIN = 0.0f;
    private const float FEAR_DECAY_RATE = 0.2f;
    private const float FEAR_TRANSFER_RATE = 1f;
    private const float FEAR_SACRIFICE_MAX = 0.6f;
    private const float FEAR_SACRIFICE_MIN = 0.4f;
    private const float BUNNY_DROP_BUFFER = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        fear = Random.Range(0.1f, 0.7f);
        rigidbody = GetComponent<Rigidbody2D>();
        altar = GameObject.Find("Altar");
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
                    SetState(State.CarryingBunny);
                } else {
                    // Otherwise keep up the chase
                    target = bunnyTarget.transform.position;
                }
                break;
            case State.CarryingBunny:
                // If we've reached the alter start sacrificing
                if ((transform.position - altar.transform.position).magnitude < SACRIFICE_DISTANCE) {
                    SetState(State.Sacrifice);
                } else if(shouldDropBunny()) {
                    // We either calmed down too much or walk past a bunch of terrified people to the point that we forget what we were doing
                    this.SetState(State.Wandering);
                    this.transform.parent.GetComponent<Spawner>().SpawnBunny(this.transform.position);
                } else {
                    // Otherwise keep aiming for the altar
                    target = altar.transform.position;
                }
                break;
            case State.Sacrifice:
                // If we're done sacrificing go back to wandering
                if (stateChangeTimeout < 0) {
                    UiUpdate.sacrifices += 1;
                    UiUpdate.bunnies -= 1;
                    SetRandomTarget();
                    SetState(State.Wandering);
                }
                break;
            case State.Wandering:
            default:
                // TODO should we actually chase a rabbit?
                if (isSufficientlyScaredToSacrifice() && bunnies.Length > 0) {
                    bunnyTarget = bunnies[0].gameObject;
                    SetState(State.Chasing);
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
                            move = true;
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
                        move = true;
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

        UpdateFear(neighbours, bunnies);
    }

    // Move towards the centrepoint of your neighbours, but don't get too close to the centre
    void SetGroupTarget(Collider2D[] neighbours) {
        Vector2 total = neighbours.Aggregate(new Vector3(), (a, n) => n.gameObject.transform.position + a);
        Vector2 centrePoint = total/neighbours.Length;
        move = !((centrePoint - (Vector2)transform.position).magnitude < GROUP_STANDOFF_DISTANCE);
        target = centrePoint;
    }

    // Move away from the centrepoint of your neighbours
    void SetAntiGroupTarget(Collider2D[] neighbours) {
        Vector2 total = neighbours.Aggregate(new Vector3(), (a, n) => n.gameObject.transform.position + a);
        Vector2 centrePoint = total/neighbours.Length;
        var direction = (centrePoint - (Vector2)transform.position) * -10.0f;
        move = true;
        target = direction;
    }

    // Set a random target location
    void SetRandomTarget() {
        target = (Vector2)transform.position + new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
        move = true;
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

    void UpdateFear(Collider2D[] neighbours, Collider2D[] bunnies) {
        // Calculate the number of bunnies surrounding the Peon. this is normalised by distance so that bunnies
        // further away have a smaller impact on the fear decay
        var surroundingBunnies = bunnies.Sum(bunny => {
            return 1f - (this.transform.position - bunny.transform.position).magnitude / BUNNY_SEARCH_RADIUS;
        });

        if(state == State.CarryingBunny) {
            // If we are carrying a bunny, we are _obviously_ petting it so should our fear should decay a little faster?
            surroundingBunnies += 0.01f;
        }

        // Calculate the normalised sum of the fear levels of surrounding Peons. The fear from Peons that are further away have
        // a smaller impact on the fear transfer rate
        var surroundingFear = neighbours.Average(it => {
            var neighbour = it.GetComponent<Peon>();
            var normalisedDistance = (this.transform.position - neighbour.transform.position).magnitude / NEIGHBOUR_SEARCH_RADIUS;
            return 1f - (neighbour.fear * normalisedDistance);
        });

        // If everyone around us is chill, we shouldn't try to match there fear, Only bunnies and time can reduce our fear-level;
        if (surroundingFear > fear) {
            fear = Mathf.Lerp(fear, surroundingFear, FEAR_TRANSFER_RATE * Time.deltaTime);
            fear = Mathf.Clamp(fear, FEAR_MIN, FEAR_MAX);
        } else {
            // Decay fear based on how many bunnies are surrounding us
            fear = Mathf.Lerp(fear, FEAR_MIN, (surroundingBunnies + FEAR_DECAY_RATE) * Time.deltaTime);
        }
    }

    private bool isSufficientlyScaredToSacrifice() {
        return fear >= FEAR_SACRIFICE_MIN && fear <= FEAR_SACRIFICE_MAX;
    }

    private bool shouldDropBunny() {
        // TODO: Add Min/Max limits so that that the drop range is inside fear range?
        return fear <= FEAR_SACRIFICE_MIN - BUNNY_DROP_BUFFER || fear >= FEAR_SACRIFICE_MAX + BUNNY_DROP_BUFFER;
    }

    void OnMouseDown() {
        this.fear = FEAR_MAX;
    }

    public float GetFear() {
        return fear;
    }
}
