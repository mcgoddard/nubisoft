using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject peonPrefab;
    public int peonCount = 50;
    private const float MAP_SIZE = 20.0f;

    void Start()
    {
        Random.InitState((int)System.DateTime.Now.Ticks);
        for (int i = 0; i < 100; i++) {
            float randomX = (Random.value * MAP_SIZE) - (MAP_SIZE / 2.0f);
            float randomY = (Random.value * MAP_SIZE) - (MAP_SIZE / 2.0f);
            Instantiate(peonPrefab, new Vector3(randomX, randomY, 0), Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
