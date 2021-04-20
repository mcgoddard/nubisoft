using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodSplatDecal : MonoBehaviour
{
    public Sprite[] decals;
    private float aliveFor;
    private float remainingTime;
    private new SpriteRenderer renderer;

    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = decals[Random.Range(0, decals.Length)];
        aliveFor = remainingTime = Random.Range(5f, 20f);
    }

    void Update()
    {
        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0) {
            Destroy(gameObject);
        }

        // Set the alpha channel to fade out over time.
        var color = renderer.color;
        color.a = aliveFor / remainingTime;
        renderer.color = color;
    }
}
