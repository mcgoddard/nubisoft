using System.Linq;
using UnityEngine;

public class FearController : MonoBehaviour
{
    public LayerMask peonsLayer;
    public LayerMask bunniesLayer;
    private const float NEIGHBOUR_SEARCH_RADIUS = 1.0f;
    private const float BUNNY_SEARCH_RADIUS = 0.5f;
    private const float FEAR_MAX = 1.0f;
    private const float FEAR_MIN = 0.0f;
    private const float BASE_FEAR_DECAY_RATE = 0.2f;
    private const float BASE_FEAR_TRANSFER_RATE = 0.5f;
    private const float FEAR_SACRIFICE_MAX = 0.6f;
    private const float FEAR_SACRIFICE_MIN = 0.4f;
    private const float BUNNY_DROP_BUFFER = 0.2f;
    private const float TERRIFIED_LEVEL = 0.9f;
    private Peon peon;
    public float fear;
    private float fearTransferRate;
    private UiUpdate uiUpdate;

    void Start() {
        uiUpdate = GameObject.Find("UI").GetComponent<UiUpdate>();
        fear = Random.Range(0.0f, 0.1f);
        fearTransferRate = (1f - Random.Range(0.0f, 0.33f)) * BASE_FEAR_TRANSFER_RATE;
        peon = this.GetComponent<Peon>();
    }

    void Update() {
        var me = this.GetComponent<Collider2D>();
        Collider2D[] neighbours = Physics2D
            .OverlapCircleAll(transform.position, NEIGHBOUR_SEARCH_RADIUS, peonsLayer)
            .Where(neighbour => neighbour != me)
            .ToArray();

        Collider2D[] bunnies = Physics2D.OverlapCircleAll(transform.position, BUNNY_SEARCH_RADIUS, bunniesLayer);

        var surroundingBunnies = bunnies.Count();

        // if(isCarryingBunny) {
        //     // If we are carrying a bunny, we are _obviously_ petting it so should our fear should decay a little faster?
        //     surroundingBunnies += 0.01f;
        // }

        // Once we are terrified only bunnies can calm us down
        if (fear > 0.9 && surroundingBunnies == 0) {
            return; 
        }

        var maxFear = neighbours
            .Select(it => {
                var neighbour = it.GetComponent<Peon>();
                return neighbour.GetComponent<FearController>().GetFearLevel();
            })
            .DefaultIfEmpty(0f)
            .Max();

        if (maxFear > fear) {
            fear = Mathf.Lerp(fear, maxFear, fearTransferRate * Time.deltaTime);
            fear = Mathf.Clamp(fear, FEAR_MIN, FEAR_MAX);
        } else {
            fear = Mathf.Lerp(fear, FEAR_MIN, (surroundingBunnies / 10) + BASE_FEAR_DECAY_RATE * Time.deltaTime);
        }
    }

    public bool IsSufficientlyScaredToSacrifice() {
        return fear >= FEAR_SACRIFICE_MIN && fear <= FEAR_SACRIFICE_MAX;
    }

    public bool IsTerrified() {
        return fear >= 0.9f;
    }

    public bool ShouldDropBunny() {
        // TODO: Add Min/Max limits so that that the drop range is inside fear range?
        return fear <= FEAR_SACRIFICE_MIN - BUNNY_DROP_BUFFER || fear >= FEAR_SACRIFICE_MAX + BUNNY_DROP_BUFFER;
    }

    public float GetFearLevel() {
        return fear;
    }

    public void OnMouseDown()
    {
        var newFear = Mathf.Clamp(fear + uiUpdate.fearModifier, FEAR_MIN, FEAR_MAX);

        if (fear < TERRIFIED_LEVEL && newFear > TERRIFIED_LEVEL) {
            this.GetComponent<AudioSource>().PlayOneShot(peon.screamSamples[2]);
        } else if (fear < FEAR_SACRIFICE_MAX && newFear > FEAR_SACRIFICE_MAX) {
            this.GetComponent<AudioSource>().PlayOneShot(peon.screamSamples[1]);
        } else if (fear < FEAR_SACRIFICE_MIN && newFear > FEAR_SACRIFICE_MIN) {
            this.GetComponent<AudioSource>().PlayOneShot(peon.screamSamples[0]);
        }
        fear = newFear;
    }
}
