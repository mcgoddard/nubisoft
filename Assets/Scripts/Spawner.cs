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
    private const float BUNNY_SPAWN_TIMEOUT = 2f;
    private float timeSinceLastBunnySpawn = 0f;
    private GameObject[] peons;

    void Start()
    {
        Random.InitState((int)System.DateTime.Now.Ticks);
        peons = Enumerable
            .Range(0, peonCount)
            .Select(i => SpawnAtRandomPosition(peonPrefab))
            .ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        if (timeSinceLastBunnySpawn >= BUNNY_SPAWN_TIMEOUT) {
            timeSinceLastBunnySpawn = 0f;
            SpawnAtRandomPosition(bunnyPrefab);
            UiUpdate.bunnies += 1;
        } else {
            timeSinceLastBunnySpawn += Time.deltaTime;
        }

        UiUpdate.fear = peons.Average(peon => peon.GetComponent<Peon>().fear);
    }

    public void SpawnBunny(Vector3 position) {
        Instantiate(bunnyPrefab, position, Quaternion.identity, this.transform);
    }

    GameObject SpawnAtRandomPosition(GameObject prefab) {
        float randomX = (Random.value * MAP_SIZE) - (MAP_SIZE / 2.0f);
        float randomY = (Random.value * MAP_SIZE) - (MAP_SIZE / 2.0f);
        return Instantiate(prefab, new Vector3(randomX, randomY, 0), Quaternion.identity, this.transform);
    }
}
