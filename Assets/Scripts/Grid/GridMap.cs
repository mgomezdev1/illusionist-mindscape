using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GridMap<T> : IEnumerable<T>
{
    public Vector2Int Size;
    public T[] Data;
    public int Length => Data.Length;

    public GridMap(Vector2Int size, T defaultVal = default) {
        Size = size;
        Data = new T[Size.x * Size.y];
        for (int i = 0; i < Data.Length; i++)
        {
            Data[i] = defaultVal;
        }
    }

    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return Data[index]; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set { Data[index] = value; }
    }
    public T this[int x, int y]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return Data[(y * Size.x) + x]; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set { Data[(y * Size.x) + x] = value; }
    }
    public T this[Vector2Int coords]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return this[coords.x, coords.y]; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set { this[coords.x, coords.y] = value; }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2Int IndexToCoords(int i)
    {
        return GridUtils.GetCoordsFromIndex(i, Size);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CoordsToIndex(int x, int y)
    {
        return GridUtils.GetIndexFromCoords(x, y, Size);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CheckCoords(int x, int y)
    {
        return GridUtils.ValidCoords(x, y, Size);
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (T t in Data)
        {
            yield return t;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
} 