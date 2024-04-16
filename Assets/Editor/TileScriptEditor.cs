using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(Tile))]
public class TileScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Toggle Wall"))
        {
            foreach (var t in targets)
            {
                if (t is Tile tileScript)
                {
                    ResourceManager resourceManager = ResourceManager.Instance != null ? ResourceManager.Instance : FindObjectOfType<ResourceManager>();
                    bool isWall = tileScript.IsWall;
                    if (isWall)
                    {
                        tileScript.MakeWalkable();
                        tileScript.SetMaterial(resourceManager.TileMaterial);
                    }
                    else
                    {
                        tileScript.MakeWall(-1);
                        tileScript.SetMaterial(resourceManager.WallMaterial);
                    }
                }
            }            
        }

    }
}
