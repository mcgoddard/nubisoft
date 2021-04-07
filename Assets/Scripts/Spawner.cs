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

    void Start()
    {
        Random.InitState((int)System.DateTime.Now.Ticks);
        for (int i = 0; i < 100; i++) {
            SpawnAtRandomPosition(peonPrefab);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (timeSinceLastBunnySpawn >= BUNNY_SPAWN_TIMEOUT) {
            timeSinceLastBunnySpawn = 0f;
            SpawnAtRandomPosition(bunnyPrefab);
        } else {
            timeSinceLastBunnySpawn += Time.deltaTime;
        }
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
