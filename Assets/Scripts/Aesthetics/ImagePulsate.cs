using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImagePulsate : MonoBehaviour
{
    public float Period = 1.0f;
    public AnimationCurve ScaleCurve;
    public Gradient ColorCurve;
    public Image Target;

    // Update is called once per frame
    void Update()
    {
        float t = (Time.time % Period) / Period;
        float s = ScaleCurve.Evaluate(t);
        Target.rectTransform.localScale = new Vector3(s, s, s);
        Target.color = ColorCurve.Evaluate(t);
    }
}
