using System.Linq;
using UnityEngine;

public class Peon : MonoBehaviour
{
    private enum State
    {
        Grouping,
        Wandering,
    }
    public LayerMask peonsLayer;
    private Vector2 target;
    private State state = State.Grouping;
    private float stateChangeTimeout = STATE_CHANGE_TIMEOUT;
    private float randomDirectionTimeout = -1.0f;
    private const float SPEED = 0.25f;
    private const float STATE_CHANGE_TIMEOUT = 15.0f;
    private const float MAP_SIZE = 20.0f;
    private const float RANDOM_DIRECTION_TIMEOUT = 15.0f;
    private const float GROUP_STANDOFF_DISTANCE = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        if (Random.value < 0.5) {
            state = State.Grouping;
        } else {
            state = State.Wandering;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D[] neighbours = GetNeighbours();
        switch (state) {
            case State.Grouping:
                if (stateChangeTimeout < 0.0f &&
                    (neighbours.Length < 4 || neighbours.Length > 10 || Random.value < (0.05 * Time.deltaTime))) {
                    SetState(State.Wandering);
                } else {
                    SetGroupTarget(neighbours);
                    stateChangeTimeout -= Time.deltaTime;
                }
                break;
            case State.Wandering:
            default:
                if (stateChangeTimeout < 0 && neighbours.Length > 4) {
                    SetState(State.Grouping);
                } else {
                    if (randomDirectionTimeout < 0.0f) {
                        if (neighbours.Length == 1) {
                            float randomX = (Random.value * MAP_SIZE) - (MAP_SIZE / 2.0f);
                            float randomY = (Random.value * MAP_SIZE) - (MAP_SIZE / 2.0f);
                            target = new Vector2(randomX, randomY);
                        } else {
                            SetAntiGroupTarget(neighbours);
                        }
                        randomDirectionTimeout = plusMinusPercent(RANDOM_DIRECTION_TIMEOUT, 20);
                    } else {
                        randomDirectionTimeout -= Time.deltaTime;
                    }
                    stateChangeTimeout -= Time.deltaTime;
                }
                break;
        }
        float step = SPEED * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, target, step);
    }

    void SetGroupTarget(Collider2D[] neighbours) {
        Vector2 total = neighbours.Aggregate(new Vector3(), (a, n) => n.gameObject.transform.position + a);
        Vector2 centrePoint = total/neighbours.Length;
        if ((centrePoint - (Vector2)transform.position).magnitude < GROUP_STANDOFF_DISTANCE) {
            target = transform.position;
        } else {
            target = centrePoint;
        }
    }

    void SetAntiGroupTarget(Collider2D[] neighbours) {
        Vector2 total = neighbours.Aggregate(new Vector3(), (a, n) => n.gameObject.transform.position + a);
        Vector2 centrePoint = total/neighbours.Length;
        var direction = (centrePoint - (Vector2)transform.position) * -10.0f;
        target = direction;
    }

    Collider2D[] GetNeighbours() {
        return Physics2D.OverlapCircleAll(transform.position, 2, peonsLayer);
    }

    void SetState(State newState) {
        state = newState;
        stateChangeTimeout = plusMinusPercent(STATE_CHANGE_TIMEOUT, 20);
    }

    float plusMinusPercent(float original, float percent) {
        float twentyPercent = original * ((percent * 2) / 100);
        return original + (Random.value * twentyPercent) - (twentyPercent / 2.0f);
    }
}
