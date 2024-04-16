using System;
using System.Runtime.CompilerServices;
using UnityEngine;

static class GridUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int GetCoordsFromPosition(Vector3 position, float tileSize, Vector3 gridRoot)
    {
        return new Vector2Int(
            (int)((position.x - gridRoot.x + tileSize / 2) / tileSize),
            (int)((position.z - gridRoot.z + tileSize / 2) / tileSize)
        );
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 GetPositionFromCoords(Vector2Int coords, float tileSize, Vector3 gridRoot)
    {
        return new Vector3(
            gridRoot.x + (coords.x * tileSize),
            gridRoot.y,
            gridRoot.z + (coords.y * tileSize)
        );
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetIndexFromCoords(int x, int y, Vector2Int gridSize)
    {
        if (!ValidCoords(x,y, gridSize))
        {
            return -1;
        }
        return x + y * gridSize.x;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetIndexFromCoords(Vector2Int coords, Vector2Int gridSize)
    {
        return GetIndexFromCoords(coords.x, coords.y, gridSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector2Int GetCoordsFromIndex(int id, Vector2Int gridSize)
    {
        return new Vector2Int(
            id % gridSize.x,
            id / gridSize.x
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool ValidCoords(Vector2Int coords, Vector2Int gridSize)
    {
        return ValidCoords(coords.x, coords.y, gridSize);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool ValidCoords(int x, int y, Vector2Int gridSize)
    {
        return x >= 0 && y >= 0 && x < gridSize.x && y < gridSize.y;
    }
}