using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileGrid))]
public class GridScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TileGrid gridScript = (TileGrid)target;
        if (GUILayout.Button("Rebuild Tile Grid"))
        {
            gridScript.GenerateTiles();
        }
        if (GUILayout.Button("Recollect Tile References"))
        {
            gridScript.RetrieveTiles();
        }
        if (GUILayout.Button("Build Tile Grid Boundary"))
        {
            gridScript.BuildOuterWall();
        }
        if (GUILayout.Button("Recalculate Walls"))
        {
            gridScript.RecalculateWalls();
        }
    }
}
