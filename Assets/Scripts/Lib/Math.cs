using System;
using System.Runtime.CompilerServices;
using UnityEngine;

static class MathUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ManhattanDistance(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z + b.z);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ChebyshevDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ChebyshevDistance(Vector3Int a, Vector3Int b)
    {
        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y), Mathf.Abs(a.z + b.z));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ManhattanDistance(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ManhattanDistance(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z + b.z);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ChebyshevDistance(Vector2 a, Vector2 b)
    {
        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ChebyshevDistance(Vector3 a, Vector3 b)
    {
        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y), Mathf.Abs(a.z + b.z));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float MinRectDistance(Rect rect, Vector2 point)
    {
        return MinRectDistance(rect, point.x, point.y);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float MinRectDistance(Rect rect, float x, float y)
    {
        Vector2 closestPoint = ClosestRectPoint(rect, x, y);
        float dx = closestPoint.x - x;
        float dy = closestPoint.y - y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
    public static Vector2 ClosestRectPoint(Rect rect, float x, float y)
    {
        float deltaX = 0.0f;
        float deltaY = 0.0f;
        if (x < rect.xMin) { deltaX = x - rect.xMin; }
        else if (x > rect.xMax) { deltaX = rect.xMax - x; }
        if (y < rect.yMin) { deltaY = y - rect.yMin; }
        else if (y > rect.yMax) { deltaY = rect.yMax - y; }
        return new Vector2(x - deltaX, y - deltaY);
    }

    internal static float SqrDistance2D(Vector3 a, Vector3 b)
    {
        float deltaX = a.x - b.x;
        float deltaZ = a.z - b.z;
        return (deltaX * deltaX) + (deltaZ * deltaZ);
    }
}