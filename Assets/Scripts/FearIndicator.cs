using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FearIndicator : MonoBehaviour
{
    private Peon peon;

    private Gradient gradient;
    // Start is called before the first frame update
    void Start()
    {
        this.peon = this.GetComponentInParent<Peon>();
        this.gradient = new Gradient();

        var colorKeys = new GradientColorKey[]{
            new GradientColorKey(Color.green, 0.0f),
            new GradientColorKey(Color.yellow, 0.5f),
            new GradientColorKey(Color.red, 1.0f),
        };

        var alphaKeys = new GradientAlphaKey[]{
            new GradientAlphaKey(1.0f, 0.0f)
        };

        this.gradient.SetKeys(colorKeys, alphaKeys);
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Renderer>().material.color = this.gradient.Evaluate(peon.GetFear());
    }
}
