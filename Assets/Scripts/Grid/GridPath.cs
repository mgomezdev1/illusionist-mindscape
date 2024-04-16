using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Assertions;

public interface IGridPath
{
    /// <summary>
    /// Updates a path follow index and returns true if the path following is completed; target is the next point to go towards in the path.
    /// </summary>
    /// <param name="index">A reference to the index of the current step being followed in the path</param>
    /// <param name="position">The current position of the follower</param>
    /// <param name="target">The resulting destination that the follower must go towards in a straight line</param>
    /// <param name="stopDistance">The distance at which a path point is considered "reached"</param>
    /// <returns></returns>
    bool Follow(ref int index, Vector3 position, out Vector3 target, float stopDistance = 0.1f);
    Vector3 GetDestination();
    List<Vector3> GetSteps(float y);
    bool ReachedDestination(int gridPathFollowIndex);

    IEnumerable<Vector3> GetDynamicSteps(Vector3 currentPosition, int gridPathFollowIndex)
    {
        List<Vector3> allSteps = GetSteps(currentPosition.y);
        yield return currentPosition;
        for (int i = gridPathFollowIndex; i < allSteps.Count; i++)
        {
            yield return allSteps[i];
        }
    }
}
public class GridPath : IGridPath
{
    public List<Vector3> Steps { get; private set; } = new List<Vector3>();

    public GridPath(IEnumerable<Vector3> steps)
    {
        foreach (Vector3 v in steps)
        {
            this.Steps.Add(v);
        }
    }

    // Takes a reference to the current path index and returns the next target point in Target
    // Returns true if the following is complete
    public bool Follow(ref int index, Vector3 position, out Vector3 target, float stopDistance)
    {
        target = Vector3.zero;
        if (index >= Steps.Count) return true;
        target = Steps[index];
        Vector3 delta = target - position;
        while (delta.sqrMagnitude < stopDistance * stopDistance)
        {
            index++;
            if (index >= Steps.Count) return true;
            target = Steps[index];
            delta = target - position;
        }
        return false;
    }

    public bool ReachedDestination(int gridPathFollowIndex)
    {
        return gridPathFollowIndex >= Steps.Count; 
    }

    public Vector3 GetDestination()
    {
        Debug.Log($"Getting destination from steps {Steps.ToArray()}");
        return Steps[^0];
    }

    public List<Vector3> GetSteps(float y)
    {
        return Steps.Select(s => new Vector3(s.x, y, s.z)).ToList();
    }
}

public class CellGridPath : IGridPath
{
    private readonly TileGrid hostGrid;
    public List<Vector2Int> Steps { get; private set; } = new List<Vector2Int>();

    public CellGridPath(List<Vector2Int> cells, TileGrid hostGrid, bool useListReference = false)
    {
        this.hostGrid = hostGrid;
        if (useListReference) Steps = cells;
        else
        {
            foreach (var c in cells)
            {
                Steps.Add(c);
            }
        }
    }

    public static CellGridPath FromPredecessors(Vector2Int end, GridMap<Vector2Int> predecessors, TileGrid hostGrid)
    {
        List<Vector2Int> cells = new();
        while (end != TileGrid.InvalidCell)
        {
            cells.Add(end);
            end = predecessors[end];
        }
        cells.Reverse();
        return new CellGridPath(cells, hostGrid, true);
    }

    public void Smoothen()
    {
        if (Steps.Count <= 2) return;
        Vector2Int lastReachableCell = Steps[0];
        List<Vector2Int> newSteps = new()
        {
            lastReachableCell
        };
        for (int i = 2; i < Steps.Count; i++) { 
            Vector2Int cell = Steps[i];
            if (!hostGrid.DirectPathExists(lastReachableCell, cell))
            {
                lastReachableCell = Steps[i - 1];
                newSteps.Add(lastReachableCell);
            }
        }
        newSteps.Add(Steps[^1]);
        Steps.Clear();
        Steps.AddRange(newSteps);
    }

    public bool Follow(ref int index, Vector3 position, out Vector3 target, float stopDistance)
    {
        target = Vector3.zero;
        if (index >= Steps.Count)
        {
            Vector3 lastPos = hostGrid.GetPositionFromCoords(Steps.Last());
            if (MathUtils.SqrDistance2D(position, lastPos) > stopDistance * stopDistance)
            {
                target = lastPos;
                index--;
                return false;
            }
            return true;
        }
        Vector2Int targetCell = Steps[index];
        target = hostGrid.GetPositionFromCoords(targetCell);
        while (MathUtils.SqrDistance2D(target, position) < stopDistance * stopDistance)
        {
            index++;
            if (index >= Steps.Count) return true;
            targetCell = Steps[index]; 
            target = hostGrid.GetPositionFromCoords(targetCell);
        }
        return false;
    }

    public bool ReachedDestination(int gridPathFollowIndex)
    {
        return gridPathFollowIndex >= Steps.Count;
    }

    public Vector3 GetDestination()
    {
        return hostGrid.GetPositionFromCoords(Steps.Last());
    }

    public List<Vector3> GetSteps(float y)
    {
        return Steps.Select(s => hostGrid.GetPositionFromCoords(s))
            .Select(s => new Vector3(s.x, y, s.z)).ToList();
    }
}