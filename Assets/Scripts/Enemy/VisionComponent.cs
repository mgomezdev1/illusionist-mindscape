using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

#nullable enable
public class VisionComponent : MonoBehaviour
{
    [SerializeField] private float visionAngleDegrees = 90f;
    [SerializeField] private float visionDistanceMeters = 5f;
    [SerializeField] private Transform eye;
    [SerializeField] private float surfaceY = 1.0f; 
    [SerializeField] private LayerMask visionBlockingLayers;

    private Enemy host;
    public Enemy Host => host;

    [SerializeField] private float awarenessRate = 0.65f;
    [SerializeField] private float obliviousnessRate = 0.05f;
    [SerializeField] private float awareness;
    public float Awareness { 
        get => awareness; 
        private set { awareness = value; OnAwarenessUpdated.Invoke(value); } 
    }

    private Player? target = null;
    public Player? Target => target;

    [Header("Vision Rendering")]
    [SerializeField] protected int visionRenderRayCount = 16;
    [SerializeField] protected MeshFilter visionMeshFilter;
    [SerializeField] protected MeshRenderer visionRenderer;
    private Mesh visionMesh;
    private Color currentVisionColor = Color.clear;

    [Header("Synchronization")]
    public UnityEvent<float> OnAwarenessUpdated = new();

    private void Awake()
    {
        host = GetComponent<Enemy>();
        visionMesh = new Mesh();
        visionMeshFilter.mesh = visionMesh;
    }

    private void Update()
    {
        RedrawVisionCone();
        Color myHivemindVisionColor = HiveMind.Instance.GetVisionRenderColor(Host);
        if (myHivemindVisionColor != currentVisionColor)
        {
            currentVisionColor = myHivemindVisionColor;
            Color withProperAlpha = myHivemindVisionColor.WithAlpha(0.5f);
            visionRenderer.material.color = withProperAlpha;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInVisionCone(Vector3 point)
    {
        return IsInVisionCone(eye, point, GetEffectiveVisionRange(), visionAngleDegrees);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInVisionCone(Transform viewer, Vector3 point, float distance, float visionAngleDegrees)
    {
        return IsInVisionCone(point - viewer.position, viewer.forward, distance * distance, Mathf.Cos(Mathf.Deg2Rad * visionAngleDegrees / 2));
    }
    // This is the most efficient version of this function, use when optimizing
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInVisionCone(Vector3 delta, Vector3 viewerForward, float sqrDistance, float halfVisionAngleCosine)
    {
        if (delta.sqrMagnitude > sqrDistance) return false;
        float angleCos = Vector3.Dot(viewerForward, delta.normalized);
        return angleCos >= halfVisionAngleCosine;
    }

    public float GetEffectiveVisionRange()
    {
        if (host != null && host.HasEffect(Effect.Tag.Blinded)) return Effect.BlindEffectVisionRange;
        return visionDistanceMeters;
    }

    public bool IsPlayerInLoS(int index = 0)
    {
        var player = GameController.Instance.GetPlayerByIndex(index);
        if (player == null) return false;
        target = player;
        return IsObjectInLoS(player.gameObject);
    }
    public bool IsObjectInLoS(GameObject obj)
    {
        if (!IsInVisionCone(obj.transform.position)) {
            return false;
        }
        Vector3 delta = obj.transform.position - transform.position;
        if (Physics.Raycast(transform.position, delta.normalized, delta.magnitude, visionBlockingLayers))
        {
            return false;
        }
        return true;
    }

    public void UpdateAwareness()
    {
        if (IsPlayerInLoS())
        {
            awareness += awarenessRate * Time.deltaTime;
        } 
        else
        {
            awareness -= obliviousnessRate * Time.deltaTime;
        }
        // This calls the updated event

        Awareness = Mathf.Clamp01(awareness);
    }

    public bool IsAwareOfTarget([NotNullWhen(true)] out Player? currentTarget)
    {
        if (awareness > 0.99f && Target != null)
        {
            currentTarget = Target;
            return true;
        }
        currentTarget = null;
        return false;
    }
    public void MakeAware()
    {
        Awareness = 1.0f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetVisibleCells<T>(TileGrid grid, GridMap<T> map, T visibleValue)
    {
        SetVisibleCells(grid, map, visibleValue, _ => false);
    }
    public void SetVisibleCells<T>(TileGrid grid, GridMap<T> map, T visibleValue, Predicate<T> shouldSkip)
    {
        float visDist = GetEffectiveVisionRange();
        float distSqrd = visDist * visDist;
        float cosAngle = Mathf.Cos(Mathf.Deg2Rad * visionAngleDegrees / 2);
        Vector3 forward = eye.forward;
        Vector3 here = eye.position;
        for (int i = 0; i < map.Data.Length; i++)
        {
            T val = map.Data[i];
            if (shouldSkip(val)) continue;
            Vector2Int coords = map.IndexToCoords(i);
            Tile t = grid.Tiles[coords.x, coords.y];
            if (t == null) continue;
            Vector3 target = t.transform.position;
            target.y = here.y; // Keep vision moving forward, not down.
            Vector3 delta = target - here;
            if (!IsInVisionCone(delta, forward, distSqrd, cosAngle)) continue;

            if (Physics.Raycast(here, delta.normalized, delta.magnitude, visionBlockingLayers))
            {
                continue; // hit a wall
            }
            map.Data[i] = visibleValue;
        }
    }

    public void RedrawVisionCone()
    {
        Vector3[] points = new Vector3[visionRenderRayCount + 1];
        Vector3 here = eye.transform.position;
        Vector3 forward = eye.transform.forward;
        float forwardAngleRads = Mathf.Atan2(forward.z, forward.x);
        float visRange = GetEffectiveVisionRange();
        float deg90 = Mathf.Deg2Rad * 90;
        float degsPerRay = visionAngleDegrees / (visionRenderRayCount - 1);
        float halfVisAngle = visionAngleDegrees / 2;
        points[0] = Vector3.zero;
        for (int i = 0; i < visionRenderRayCount; i++)
        {
            float angleDelta = Mathf.Deg2Rad * (i * degsPerRay - halfVisAngle);
            float theta = forwardAngleRads + angleDelta;
            // Should always be normal!!!
            Vector3 worldDirection = new(Mathf.Cos(theta), 0, Mathf.Sin(theta));
            Vector3 localDirection = new(Mathf.Cos(angleDelta + deg90), 0, Mathf.Sin(angleDelta + deg90));
            Ray ray = new(here, worldDirection);
            float distanceReached = visRange;
            if (Physics.Raycast(ray, out RaycastHit hit, visRange, visionBlockingLayers))
            {
                distanceReached = hit.distance;
            }
            Debug.DrawLine(here, here + worldDirection * distanceReached, Color.red);
            points[i + 1] = localDirection * distanceReached;
        }
        MeshUtils.DrawPolygon(visionMesh, points);

        // Reposition vision cone to be on the ground
        Vector3 visionRendererPos = visionRenderer.transform.position;
        visionRendererPos.y = surfaceY + 0.05f;
        visionRenderer.transform.position = visionRendererPos;
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawFrustum(Vector3.zero, visionAngleDegrees, GetEffectiveVisionRange(), 0, 1);

        /*
        Vector3 here = eye.transform.position;
        foreach (Vector3 p in visionMeshFilter.mesh != null ? visionMeshFilter.mesh.vertices : new Vector3[0])
        {
            Gizmos.DrawLine(here, here + p);
        }
        */
    }
}