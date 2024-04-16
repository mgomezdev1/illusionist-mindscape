using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    private static ResourceManager instance;
    public static ResourceManager Instance {
        get
        {
            if (instance == null) instance = FindObjectOfType<ResourceManager>(); 
            return instance;
        }
        private set { instance = value; }
    }

    [Header("Tile Resources")]
    public Material TileMaterial;
    public Material WallMaterial;
    public Material ObservedMaterial;
    public Material HighlightedMaterial;

    [ExecuteInEditMode]
    private void Awake()
    {
        Instance = this;
    }
}
