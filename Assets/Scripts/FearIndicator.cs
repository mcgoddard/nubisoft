using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FearIndicator : MonoBehaviour
{
    private static Gradient gradient;
    // Start is called before the first frame update

    static void InitGradient() {
        if (gradient == null) {
            gradient = new Gradient();

            var colorKeys = new GradientColorKey[]{
                new GradientColorKey(Color.green, 0.0f),
                new GradientColorKey(Color.yellow, 0.5f),
                new GradientColorKey(Color.red, 1.0f),
            };

            var alphaKeys = new GradientAlphaKey[]{
                new GradientAlphaKey(1.0f, 0.0f)
            };

            gradient.SetKeys(colorKeys, alphaKeys);
        }
    }

    void Start()
    {
        InitGradient();
    }

    void Update()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer != null) {
            renderer.material.color = gradient.Evaluate(this.GetComponentInParent<FearController>().GetFearLevel());
        }
    }
}
