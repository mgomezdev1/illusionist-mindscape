using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialTilingOffset : MonoBehaviour
{
    public float xOffset, yOffset;
    public Color color = Color.white;

    public Renderer r;
    
    private void Awake()
    {
        if (r == null) r = GetComponent<Renderer>();
        if(r != null) InitMaterial();
    }

    private void InitMaterial()
    {
        var material = r.material;
        material.mainTextureOffset = new Vector2(xOffset, yOffset);
        material.color = color;
    }
}
