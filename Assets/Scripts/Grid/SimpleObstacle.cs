using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

#nullable enable
public class SimpleObstacle : MonoBehaviour, IObstacle
{
    public bool Blocking
    {
        get => blocking; set => blocking = value;
    }
    [SerializeField] protected bool blocking = true;
    [SerializeField] protected List<Tile> tiles = new List<Tile>();
    [SerializeField] private bool autoCalculateAffectedTilesOnAwake = true;

    [SerializeField] float radius;
    [SerializeField] bool isRadiusSquare;

    protected virtual void Awake()
    {
        if (autoCalculateAffectedTilesOnAwake)
        {
            SetAffectedTilesAutomatically();
        }
        else
        {
            foreach (Tile tile in tiles)
            {
                tile.Obstacles.Add(this);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetAffectedTilesAutomatically(TileGrid? grid = null)
    {
        if (grid == null) {
            grid = TileGrid.GetClosestGrid(transform.position);
            if (grid == null)
            {
                Debug.LogWarning("Failed to find a grid while automatically setting obstacle affected tiles");
                return;
            }
        }
        SetAffectedTilesAutomatically(grid, radius, isRadiusSquare);
    }
    public void SetAffectedTilesAutomatically(TileGrid grid, float radius, bool squareRadius = false)
    {
        foreach (Tile tile in tiles)
        {
            tile.Obstacles.Remove(this);
        }
        tiles.Clear();
        int maxTileRadius = (int)(radius / grid.TileSize + 1);
        var cellPos = grid.GetCoordsFromPosition(transform.position);
        float r2 = radius * radius;
        for (int dx = -maxTileRadius; dx <= maxTileRadius; dx++)
        {
            for (int dy = -maxTileRadius; dy <= maxTileRadius; dy++)
            {
                int x = cellPos.x + dx;
                int y = cellPos.y + dy;
                if (!GridUtils.ValidCoords(x, y, grid.GridSize)) continue;
                Tile t = grid.Tiles[x, y];
                if (squareRadius)
                {
                    if (MathUtils.ChebyshevDistance(t.transform.position, transform.position) < radius) continue;
                }
                else
                {
                    if (!squareRadius && (t.transform.position - transform.position).sqrMagnitude > r2) continue;
                }
                t.Obstacles.Add(this);
                tiles.Add(t);
            }
        }
    }

    public void SetAffectedTiles(List<Tile> newTiles)
    {
        foreach (Tile tile in tiles) { 
            tile.Obstacles.Remove(this);
        }
        tiles.Clear();
        foreach (Tile tile in newTiles) {
            tile.Obstacles.Add(this);
            tiles.Add(tile);
        }
    }

    public bool IsBlocking()
    {
        return blocking;
    }
}
