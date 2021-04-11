using System.Linq;
using UnityEngine;

public class FearController : MonoBehaviour
{
    public LayerMask peonsLayer;
    public LayerMask bunniesLayer;
    private const float NEIGHBOUR_SEARCH_RADIUS = 1.0f;
    private const float BUNNY_SEARCH_RADIUS = 1.0f;
    private const float FEAR_MAX = 1.0f;
    private const float FEAR_MIN = 0.0f;
    private const float FEAR_DECAY_RATE = 0.2f;
    private const float FEAR_TRANSFER_RATE = 1f;
    private const float FEAR_SACRIFICE_MAX = 0.6f;
    private const float FEAR_SACRIFICE_MIN = 0.4f;
    private const float BUNNY_DROP_BUFFER = 0.2f;
    private float fear;
    private bool isCarryingBunny = false;


    void Start() {
        fear = Random.Range(0.0f, 0.1f);
    }

    void Update() {
        Collider2D[] neighbours = Physics2D.OverlapCircleAll(transform.position, NEIGHBOUR_SEARCH_RADIUS, peonsLayer);
        Collider2D[] bunnies = Physics2D.OverlapCircleAll(transform.position, BUNNY_SEARCH_RADIUS, bunniesLayer);

        // Calculate the number of bunnies surrounding the Peon. this is normalised by distance so that bunnies
        // further away have a smaller impact on the fear decay
        var surroundingBunnies = bunnies.Sum(bunny => {
            return 1f - (this.transform.position - bunny.transform.position).magnitude / BUNNY_SEARCH_RADIUS;
        });

        if(isCarryingBunny) {
            // If we are carrying a bunny, we are _obviously_ petting it so should our fear should decay a little faster?
            surroundingBunnies += 0.01f;
        }

        // Calculate the normalised sum of the fear levels of surrounding Peons. The fear from Peons that are further away have
        // a smaller impact on the fear transfer rate
        var surroundingFear = neighbours.Average(it => {
            var neighbour = it.GetComponent<Peon>();
            var normalisedDistance = (this.transform.position - neighbour.transform.position).magnitude / NEIGHBOUR_SEARCH_RADIUS;
            return 1f - (neighbour.GetComponent<FearController>().GetFearLevel() * normalisedDistance);
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

    public bool IsSufficientlyScaredToSacrifice() {
        return fear >= FEAR_SACRIFICE_MIN && fear <= FEAR_SACRIFICE_MAX;
    }

    public bool ShouldDropBunny() {
        // TODO: Add Min/Max limits so that that the drop range is inside fear range?
        return fear <= FEAR_SACRIFICE_MIN - BUNNY_DROP_BUFFER || fear >= FEAR_SACRIFICE_MAX + BUNNY_DROP_BUFFER;
    }

    public float GetFearLevel() {
        return fear;
    }
}
