using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

# nullable enable
public class OccupancyMap : GridMap<float>
{
    TileGrid hostGrid;
    GridMap<CellVision> visionMap;
    float diffusionRate;

    private Vector2Int bestCell = new(-1, -1);
    private float bestCellValue = 0.0f;
    public Vector2Int BestCell => MaxCell(out float _);
    public float BestCellValue { get { MaxCell(out float r); return r; } }
    
    private bool occupancyMapDirty = false;

    public OccupancyMap(TileGrid hostGrid, float diffusionRate = 1.0f) : base(hostGrid.GridSize, 0)
    {
        this.hostGrid = hostGrid;
        visionMap = new GridMap<CellVision>(Size, CellVision.Hidden);
        this.diffusionRate = diffusionRate;
    }

    public void UpdateVisionMap(GridMap<CellVision> visionMap)
    {
        this.visionMap = visionMap;
    }

    public void RunUpdate(float updateInterval, VisionComponent observer)
    {
        if (observer.IsAwareOfTarget(out Player? p))
        {
            SetTargetPosition(p.transform.position);
        }

        // We need to know of a target for this to make any sense
        if (observer.Target == null) return;
        Vector2Int targetCell = hostGrid.GetCoordsFromPosition(observer.Target.transform.position);

        // We have to do this even if the player is visible, otherwise we'll zero out the player position in the next step
        Diffuse(updateInterval);
        if (!observer.Host.IsEnlightened())
        {
            // If we can't talk to the hive mind, only clear our own probability.
            UpdateVisionMap(hostGrid.GetVisibilityGridMap(new Enemy[] { observer.Host }));
        }
        else
        {
            // We'll clear the probability from the whole hive mind otherwise
            UpdateVisionMap(HiveMind.Instance.GetHiveVision(hostGrid));
        }

        ClearVisible(targetCell);

        occupancyMapDirty = true;
    }

    public void SetTargetPosition(Vector3 position)
    {
        SetTargetCell(hostGrid.GetCoordsFromPosition(position));
    }
    public void SetTargetCell(Vector2Int cell)
    {
        for (int i = 0; i < Length; i++)
        {
            Data[i] = 0.0f;
        }
        this[cell] = 1.0f;
    }

    public bool IsCellVisible(Vector2Int cell)
    {
        return visionMap[cell] == CellVision.Visible;
    }

    private static readonly List<Tuple<Vector2Int, float>> diffuseDeltas = new() { 
        new(new(1, 0), 0.25f),
        new(new(0, 1), 0.25f),
        new(new(-1, 0), 0.25f),
        new(new(0, -1), 0.25f)
    };
    public void Diffuse(float deltaTime)
    {
        GridMap<float> temp = new GridMap<float>(Size);
        float totalDiffused = 0.0f;
        // Copy all values to temp
        for (int i = 0; i < Length; ++i)
        {
            temp[i] = Data[i];
        }

        Debug.Log("Running omap diffuse");
        float alphaFactor = Mathf.Min(1, diffusionRate * deltaTime);

        for (int x = 0; x < Size.x; x++)
        {
            for (int y = 0; y < Size.y; y++)
            {
                // We'll only diffuse the density of visible cells
                // if (visionMap[x, y] != CellVision.Hidden) continue;
                float toDiffuse = this[x, y];
                if (toDiffuse == 0.0f) continue;

                foreach (var delta in diffuseDeltas)
                {
                    Vector2Int diffuseTarget = new(x + delta.Item1.x, y + delta.Item1.y);
                    if (!GridUtils.ValidCoords(diffuseTarget, Size)) continue;
                    if (temp[diffuseTarget] > temp[x, y]) continue;
                    if (visionMap[diffuseTarget] != CellVision.Hidden) continue;
                    float alpha = alphaFactor * delta.Item2;
                    float diffused = toDiffuse * alpha;
                    temp[diffuseTarget] += diffused;
                    temp[x, y] -= diffused;
                    totalDiffused += diffused;
                }
            }
        }

        for (int i = 0; i < Length; ++i)
        {
            Data[i] = temp[i];
        }
        Debug.Log($"Diffused {totalDiffused}");
    }

    public void ClearVisible(Vector2Int targetCell)
    {
        for (int i = 0; i < Length; i++)
        {
            if (visionMap[i] != CellVision.Hidden) {
                Vector2Int cell = GridUtils.GetCoordsFromIndex(i, hostGrid.GridSize);
                if (MathUtils.ManhattanDistance(cell, targetCell) <= 1) continue;
                Data[i] = 0.0f; 
            }
        }
        Renormalize();
    }

    public float Sum()
    {
        float s = 0.0f;
        foreach (float v in Data) s += v;
        return s;
    }
    public void Renormalize(float epsilon = 1e-7f)
    {
        float s = Sum();
        if (s < epsilon) { return; }
        for (int i = 0; i < Length; ++i)
        {
            Data[i] /= s;
        }
    }

    public Vector2Int MaxCell(out float occupancyValue)
    {
        // We'll not recalculate unless we've run an update since we last calculated this
        // Yay, caching! Some optimization
        if (!occupancyMapDirty)
        {
            occupancyValue = this.bestCellValue;
            return bestCell;
        }

        occupancyMapDirty = false;
        int bestIndex = 0;
        occupancyValue = 0.0f;
        for (int i = 0; i < Length; ++i)
        {
            float v = Data[i];
            if (v > occupancyValue)
            {
                occupancyValue = v;
                bestIndex = i;
            }
        }
        this.bestCellValue = occupancyValue;
        bestCell = GridUtils.GetCoordsFromIndex(bestIndex, Size);
        return bestCell;
    }

    public static Color OccupancyGizmoColor(float h)
    {
        return new Color(0, 0, h);
    }

    public void DrawOccupancyGizmos()
    {
        Vector2Int maxCell = MaxCell(out float maxValue);
        int bestIndex = GridUtils.GetIndexFromCoords(maxCell, Size);
        for (int i = 0; i < Length; ++i)
        {
            float h = Data[i] / maxValue;
            Vector3 pos = hostGrid.Tiles[i].transform.position + Vector3.up;
            Gizmos.color = i == bestIndex ? Color.green : OccupancyGizmoColor(h);
            Gizmos.DrawWireCube(new(pos.x, pos.y + h, pos.z), new(1, h * 2, 1));
        }
    }

    public void DrawVisibilityGizmos()
    {
        for (int i = 0; i < Length; ++i)
        {
            CellVision v = visionMap[i];
            Vector3 pos = hostGrid.Tiles[i].transform.position + Vector3.up;
            Gizmos.color = v switch
            {
                CellVision.Visible => Color.green,
                CellVision.Hidden => Color.white,
                CellVision.Untraversable => Color.red,
                _ => Color.black
            };
            Gizmos.DrawWireSphere(new(pos.x, pos.y + 1.5f, pos.z), 0.25f);
        }
    }
}