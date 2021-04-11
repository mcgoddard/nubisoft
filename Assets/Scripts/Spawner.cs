using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject peonPrefab;
    public GameObject bunnyPrefab;
    public int peonCount = 50;
    private const float MAP_SIZE = 20.0f;
    private Bounds mapBounds;
    private const float BUNNY_SPAWN_RATE = 0.25f;
    private float timeSinceLastBunnySpawn = 0f;
    private GameObject[] peons;

    void Start()
    {
        mapBounds = new Bounds(Vector3.zero, new Vector3(MAP_SIZE, MAP_SIZE, 0));
        Random.InitState((int)System.DateTime.Now.Ticks);
        peons = Enumerable
            .Range(0, peonCount)
            .Select(i => SpawnAtRandomPosition(peonPrefab))
            .ToArray();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timeSinceLastBunnySpawn -= Time.deltaTime;

        if (timeSinceLastBunnySpawn <0 && Input.GetMouseButton(1))
        {
            var mousePosition = UnitSelector.GetMouseWorldPosition();

            if (mapBounds.Contains(mousePosition))
            {
                var offset = Random.insideUnitCircle / 10;
                var spawnLocation = mousePosition + new Vector3(offset.x, offset.y, 0);
                SpawnBunny(spawnLocation);
                UiUpdate.bunnies += 1;
            }

            timeSinceLastBunnySpawn = BUNNY_SPAWN_RATE;
        }

        UiUpdate.fear = peons.Average(peon => peon.GetComponent<FearController>().GetFearLevel());
    }

    public void SpawnBunny(Vector3 position)
    {
        Instantiate(bunnyPrefab, position, Quaternion.identity, this.transform);
    }

    GameObject SpawnAtRandomPosition(GameObject prefab)
    {
        float randomX = (Random.value * MAP_SIZE) - (MAP_SIZE / 2.0f);
        float randomY = (Random.value * MAP_SIZE) - (MAP_SIZE / 2.0f);
        return Instantiate(prefab, new Vector3(randomX, randomY, 0), Quaternion.identity, this.transform);
    }
}
