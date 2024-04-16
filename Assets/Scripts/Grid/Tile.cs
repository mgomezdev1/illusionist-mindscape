using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int id;
    [SerializeField]
    private MeshRenderer highlightRender;
    private MeshRenderer render;
    private Vector3 startPosition;
    private Vector3 startScale;
    [SerializeField] private float wallTime;
    private float initialWallTime = 0.0f;
    [SerializeField]
    private List<IObstacle> obstacles = new List<IObstacle>();
    public List<IObstacle> Obstacles { get => obstacles; private set => obstacles = value; }

    private bool visible = false;
    private bool selected = false;

    public static float WallHeight = 3f;
    public bool IsWall { get => wallTime < -0.5f || wallTime > 0.05f; }

    public enum HighlightState
    {
        Off,
        Observed,
        Selected
    }

    bool started = false;
    // Start is called before the first frame update
    void Start()
    {
        if (started) return;
        started = true;
        RecalculateIsWall();
        startPosition = transform.position;
        startScale = transform.localScale;
        render = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (wallTime > 0.0f)
        {
            wallTime -= Time.deltaTime;
            if (wallTime < 0)
            {
                SetHeight(0);
                wallTime = -0.1f;
            }
        }
    }

    public void SetMaterial(Material mat)
    {
        if (!started) Start();
        render.material = mat;
    }

    public void SetHighlightState(HighlightState state)
    {
        if (state == HighlightState.Off)
        {
            highlightRender.gameObject.SetActive(false);
            return;
        }
        highlightRender.gameObject.SetActive(true);
        Material mat = (state) switch
        {
            HighlightState.Observed => ResourceManager.Instance.ObservedMaterial,
            HighlightState.Selected => ResourceManager.Instance.HighlightedMaterial,
            _ => throw new System.Exception($"Invalid state passed to SetHighlightState: {Enum.GetName(typeof(HighlightState), state)}")
        };
        highlightRender.material = mat;
    }
    public void SetVisibility(bool v)
    {
        if (v == visible) return;
        visible = v;
        if (selected) return; // Selection has priority over visibility
        SetHighlightState(v ? HighlightState.Observed : HighlightState.Off);
    }
    public void SetSelected(bool s)
    {
        if (s == selected) return;
        selected = s;
        if (s) SetHighlightState(HighlightState.Selected);
        else SetHighlightState(visible ? HighlightState.Observed : HighlightState.Off);
    }

    public void SetHeight(float h)
    {
        if (!started) Start();
        transform.position = new Vector3(startPosition.x, startPosition.y + h * startScale.y / 2, startPosition.z);
        transform.localScale = new Vector3(startScale.x, startScale.y * (1 + h), startScale.z);
    }

    public void MakeWall(float duration = -1)
    {
        if (initialWallTime < 0.5f) duration = -1; // infinite wall
        SetHeight(WallHeight);
        initialWallTime = duration;
        wallTime = duration;
    }
    public void MakeWalkable()
    {
        if (!IsWall) return;
        SetHeight(0);
        initialWallTime = 0.0f;
        wallTime = 0.0f;
    }

    internal void RecalculateIsWall()
    {
        MeshRenderer render = GetComponent<MeshRenderer>();
        bool shouldBeWall = render.sharedMaterial == ResourceManager.Instance.WallMaterial;
        if (shouldBeWall && !IsWall)
        {
            initialWallTime = -1;
            wallTime = -1;
        }
        else if (!shouldBeWall && IsWall)
        {
            MakeWalkable();
        }
    }
}
