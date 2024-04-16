using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;
using Utils;

#nullable enable
public class TileGrid : MonoBehaviour
{
    [SerializeField]
    private GameObject tilePrefab;
    [SerializeField]
    private float tileSize;
    public float TileSize => tileSize;
    [SerializeField]
    private Vector2Int gridSize;
    public Vector2Int GridSize => gridSize;

    public GameObject TileParentContainer;
    public GridMap<Tile> Tiles;

    public static readonly Vector2Int InvalidCell = new Vector2Int(-1, -1);
    public static List<TileGrid> worldGrids = new List<TileGrid>();

    void Awake()
    {
        worldGrids.Add(this);
        if (Tiles == null)
        {
            if (!RetrieveTiles())
            {
                GenerateTiles(gridSize);
            }
        }
    }

    private void OnDestroy()
    {
        worldGrids.Remove(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rect GetRect()
    {
        return new Rect(transform.position.x, transform.position.y, TileSize * GridSize.x, TileSize * GridSize.y);
    }

    public static TileGrid? GetClosestGrid(Vector3 position)
    {
        TileGrid? closest = null;
        float bestDistance = float.PositiveInfinity;
        foreach (TileGrid g in worldGrids)
        {
            Rect gRect = g.GetRect();
            float distance = MathUtils.MinRectDistance(gRect, position.x, position.z);
            if (distance < bestDistance)
            {
                closest = g;
                bestDistance = distance;
            }
        }
        return closest;
    }

    public bool RetrieveTiles()
    {
        if (TileParentContainer.transform.childCount != GridSize.x * GridSize.y) return false;
        Tiles = new GridMap<Tile>(GridSize);
        for (int i = 0; i < Tiles.Length; i++)
        {
            Transform tileTransform = TileParentContainer.transform.GetChild(i);
            Vector2Int coords = GetCoordsFromPosition(tileTransform.position);
            Tiles[coords] = tileTransform.GetComponent<Tile>();
        }
        return true;
    }

    public void GenerateTiles(Vector2Int? gridSize = null)
    {
        if (gridSize is not null)
            this.gridSize = gridSize.Value;

        if (Tiles is not null) {
            foreach (var t in Tiles.Data)
            {
                if (t != null)
                    DestroyImmediate(t.gameObject);
            }
        }
        Tiles = new GridMap<Tile>(this.gridSize);
        int idx = 0;
        for (int x = 0; x < this.gridSize.x; x++)
        {
            for (int y = 0; y < this.gridSize.y; y++)
            {
                var tileObj = Instantiate(tilePrefab, TileParentContainer.transform);
                tileObj.transform.position = new Vector3(x * tileSize, 0, y * tileSize);
                Tile t = tileObj.GetComponent<Tile>();
                t.id = idx;
                Tiles[idx++] = t;              
            }
        }
    }

    public void BuildOuterWall()
    {
        if (Tiles is null) GenerateTiles();
        Tile t;
        ResourceManager editModeResourceManager = ResourceManager.Instance != null ? ResourceManager.Instance : FindObjectOfType<ResourceManager>();
        Material wallMat = editModeResourceManager.WallMaterial;
        Material tileMat = editModeResourceManager.TileMaterial;
        for (int i = 0; i < gridSize.x; i++)
        {
            for(int j = 0; j < gridSize.y; j++)
            {
                bool isOuter = i == 0 || j == 0 || i == gridSize.x - 1 || j == gridSize.y - 1;
                t = Tiles![i, j];
                t.SetMaterial(isOuter ? wallMat : tileMat);
                if (isOuter) t.MakeWall(-1);
            }
        }
    }

    public void RecalculateWalls()
    {
        foreach (Tile t in Tiles)
        {
            t.RecalculateIsWall();
        }
    }

    public Tile? GetClosestTile(Vector3 position)
    {
        Vector2Int coords = GetCoordsFromPosition(position);
        int i = GridUtils.GetIndexFromCoords(coords, gridSize);
        if (i < 0) return null;
        return Tiles?[i];
    }

    static readonly Vector2Int[] NeighborDeltas = {
        new(0, 1),
        new(0, -1),
        new(1, 0),
        new(-1, 0)
    };
    public List<Vector2Int> GetNeighbors(Vector2Int coords, bool includeWalls = false)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        foreach (var delta in NeighborDeltas)
        {
            Vector2Int neighborCoords = coords + delta;
            if (!GridUtils.ValidCoords(neighborCoords, gridSize)) continue;
            if (includeWalls)
            {
                neighbors.Add(neighborCoords);
                continue;
            }
            Tile t = Tiles![neighborCoords.x, neighborCoords.y];
            // Debug.DrawLine(t.transform.position, t.transform.position + Vector3.up * 3, Color.red, 0.33f);
            if (!t.IsWall) neighbors.Add(neighborCoords);
        }
        return neighbors;
    }
    public List<Tile> GetNeighbors(Tile tile, bool includeWalls = false)
    {
        List<Tile> neighbors = new List<Tile>();
        Vector2Int tileCoords = GridUtils.GetCoordsFromIndex(tile.id, gridSize);
        foreach (var delta in NeighborDeltas)
        {
            Vector2Int neighborCoords = tileCoords + delta;
            if (!GridUtils.ValidCoords(neighborCoords, gridSize)) continue;
            Tile t = Tiles![neighborCoords.x, neighborCoords.y];
            if (includeWalls || !t.IsWall) neighbors.Add(t);
        }
        return neighbors;
    }

    public GridMap<CellVision> GetTraversabilityGridMap()
    {
        GridMap<CellVision> visionMap = new GridMap<CellVision>(gridSize);
        for (int i = 0; i < visionMap.Length; i++)
        {
            Tile t = Tiles[i];
            visionMap[i] = t.IsWall ? CellVision.Untraversable : CellVision.Hidden;
        }
        return visionMap;
    }

    public GridMap<CellVision> GetVisibilityGridMap(IEnumerable<Enemy> viewingEnemies)
    {
        var visionMap = GetTraversabilityGridMap();
        foreach (var enemy in viewingEnemies)
        {
            enemy.Vision.SetVisibleCells(this, visionMap, CellVision.Visible, t => t != CellVision.Hidden);
        }
        return visionMap;
    }

    public void UpdateTileVisibility(GridMap<CellVision> vision)
    {
        for (int i = 0; i < vision.Length; i++)
        {
            Tiles[i].SetVisibility(vision[i] == CellVision.Visible);
        }
    }

    public CellGridPath? AStar(Vector2Int start, Vector2Int goal)
    {
        PriorityQueue<Vector2Int, float> openQueue = new PriorityQueue<Vector2Int, float>();
        GridMap<bool> closedSet = new GridMap<bool>(gridSize, false);
        GridMap<float> bestPathDist = new GridMap<float>(gridSize, float.PositiveInfinity);
        GridMap<Vector2Int> predecessorMap = new GridMap<Vector2Int>(gridSize, InvalidCell);
        bestPathDist[start] = 0.0f;
        openQueue.Enqueue(start, (goal - start).magnitude);
        while (openQueue.Count > 0)
        {
            var current = openQueue.Dequeue();
            if (current == goal)
            {
                return CellGridPath.FromPredecessors(goal, predecessorMap, this);
            }
            if (closedSet[current]) continue;
            closedSet[current] = true;
            float currentCost = bestPathDist[current];
            foreach (var neighbor in GetNeighbors(current, false))
            {
                float heuristic = (neighbor - goal).magnitude;
                float totalCost = currentCost + (neighbor - current).magnitude;
                float bestFoundCost = bestPathDist[neighbor];
                if (bestFoundCost < totalCost) continue;
                bestPathDist[neighbor] = totalCost;
                predecessorMap[neighbor] = current;
                openQueue.Enqueue(neighbor, heuristic + totalCost);
            }
        }
        // No path exists
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 GetPositionFromCoords(Vector2Int coords)
    {
        return GridUtils.GetPositionFromCoords(coords, TileSize, transform.position);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2Int GetCoordsFromPosition(Vector3 position)
    {
        return GridUtils.GetCoordsFromPosition(position, TileSize, transform.position);
    }

    public bool DirectPathExists(Vector2Int cellFrom, Vector2Int cellTo, float epsilon = 0.2f)
    {
        return DirectPathExists(
            cellFrom, GetPositionFromCoords(cellFrom),
            cellTo,   GetPositionFromCoords(cellTo),
            epsilon
        );
    }
    public bool DirectPathExists(Vector3 from, Vector3 to, float epsilon = 0.2f)
    {
        return DirectPathExists(
            GetCoordsFromPosition(from), from,
            GetCoordsFromPosition(to),   to,
            epsilon
        );
    }
    public bool DirectPathExists(Vector2Int cellFrom, Vector3 from, Vector2Int cellTo, Vector3 to, float epsilon = 0.2f)
    {
        if (MathUtils.ManhattanDistance(cellFrom, cellTo) <= 1) return true;
        if ((to - from).magnitude < epsilon) return IsDiagonalClear(cellFrom, cellTo);
        Vector3 middle = (from + to) / 2;
        Vector2Int midCell = GetCoordsFromPosition(middle);
        if (Tiles[midCell].IsWall) return false;
        return DirectPathExists(cellFrom, from, midCell, middle) && DirectPathExists(midCell, middle, cellTo, to);
    }

    public bool IsDiagonalClear(Vector2Int cell1, Vector2Int cell2)
    {
        return !(Tiles[cell1.x, cell2.y].IsWall || Tiles[cell2.x, cell1.y].IsWall);
    }
    
    public IEnumerable<Vector2Int> GetCellsInLine(Vector2Int cellFrom, Vector2Int cellTo, float epsilon = 0.2f)
    {
        yield return cellFrom;
        foreach (var cell in GetCellsInLine(
            cellFrom, GetPositionFromCoords(cellFrom),
            cellTo, GetPositionFromCoords(cellTo),
            epsilon
        )) { yield return cell; }
    }
    private IEnumerable<Vector2Int> GetCellsInLine(Vector2Int cellFrom, Vector3 from, Vector2Int cellTo, Vector3 to, float epsilon = 0.2f)
    {
        if (cellFrom == cellTo) yield break;
        if (MathUtils.ManhattanDistance(cellFrom, cellTo) <= 1) yield return cellTo;
        if ((to - from).magnitude < epsilon) yield return cellTo;
        Vector3 middle = (from + to) / 2;
        Vector2Int midCell = GetCoordsFromPosition(middle);
        foreach (var cell in GetCellsInLine(cellFrom, from, midCell, middle, epsilon)) { yield return cell; }
        foreach (var cell in GetCellsInLine(midCell, middle, cellTo, to, epsilon)) { yield return cell; }
    }
}
