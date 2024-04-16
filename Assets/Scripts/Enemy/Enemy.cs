using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable
public class Enemy : MonoBehaviour, IEffectTarget
{
    public VisionComponent Vision { get; private set; }

    protected List<Effect> effects = new();
    public List<Effect> Effects => effects;
    public Transform EffectTransform => transform;

    protected CharacterController controller;
    protected Animator animator;

    protected IGridPath? gridPath;
    protected int gridPathFollowIndex = 0;
    protected Vector3? lookAtLocation;
    public TileGrid? HostGrid { get
        {
            if (hostGrid != null) return hostGrid;
            hostGrid = TileGrid.GetClosestGrid(transform.position);
            return hostGrid;
        }
    }
    private TileGrid? hostGrid;

    [Header("Enemy Stats")]
    [SerializeField] protected float baseSpeed;
    [SerializeField] protected float pathLookRotationVelocity = 720f;
    [SerializeField] protected float pathRecalculateInterval = 0.25f;
    protected float pathRecalculateTime = 0.25f;
    private LineRenderer pathRenderer;

    protected float stopTime;
    private int id;
    public int Id => id;

    protected Vector3 stationPosition;
    protected Quaternion stationRotation;
    [SerializeField] protected float stationReachedRotationSpeed = 180;

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        Vision = GetComponent<VisionComponent>();
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        pathRenderer = GetComponent<LineRenderer>();
        stationPosition = transform.position;
        stationRotation = transform.rotation;
        id = HiveMind.Instance.RegisterEnemy(this);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (GameController.Instance.IsPaused) return;
        Vision.UpdateAwareness();
        AsEffectTarget().TickEffects(Time.deltaTime);
        if (Vision.Awareness > 0.99f && Vision.IsPlayerInLoS())
        {
            OnVisionPlayerDetected(Vision.Target!.transform.position);
        }

        // Path recalculation
        if (pathRecalculateTime > 0)
        {
            pathRecalculateTime -= Time.deltaTime;
        }
        else
        {
            pathRecalculateTime = pathRecalculateInterval;
            RecalculatePath();
        }

        if (IsFollowingPath())
        {
            RenderPath(gridPath!);
        }

        if (searchTimer > 0)
        {
            searchTimer -= Time.deltaTime;
        }

        float speed = AsEffectTarget().GetSpeed(baseSpeed);
        if (stopTime > 0)
        {
            stopTime -= Time.deltaTime;
        }
        else
        {
            Quaternion targetRotation = transform.rotation;
            if (gridPath != null && !gridPath.Follow(ref gridPathFollowIndex, transform.position, out Vector3 target))
            {
                // Has path to follow
                Vector3 moveDelta = target - transform.position;
                moveDelta.y = 0;
                controller.Move(moveDelta.normalized * (speed * Time.deltaTime));
                targetRotation = Quaternion.LookRotation(moveDelta);
            }

            // Look at where it's moving, or some preferred location
            float maxTurnAngle = pathLookRotationVelocity * Time.deltaTime;
            if (lookAtLocation.HasValue)
            {
                targetRotation = Quaternion.LookRotation(lookAtLocation.Value - transform.position);
            }
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxTurnAngle);
        }
    }

    public bool HasEffect(Effect.Tag tag)
    {
        return AsEffectTarget().IsAffectedBy(tag);
    }

    Vector2Int currentDestinationCell;
    public bool GoToCell(Vector2Int targetCell)
    {
        if (currentDestinationCell == targetCell) return true;

        TileGrid? grid = HostGrid;
        if (grid == null)
        {
            Debug.LogWarning($"{gameObject.name} has no grid!");
            return false;
        }
        Vector2Int currentCell = grid.GetCoordsFromPosition(transform.position);
        CellGridPath? rawPath = grid.AStar(currentCell, targetCell);
        if (rawPath == null)
        {
            Debug.LogWarning($"{gameObject.name} failed to path from CELL <{currentCell}> to CELL <{targetCell}>");
            return false;
        }
        //Debug.Log($"{gameObject.name} is moving from CELL<{currentCell}> to CELL<{targetCell}>");
        rawPath.Smoothen();
        gridPath = rawPath;
        gridPathFollowIndex = 1;
        currentDestinationCell = targetCell;
        return true;
    }
    public bool GoToPosition(Vector3 targetPosition)
    {
        //Debug.Log($"{gameObject.name} is moving to {targetPosition}");
        TileGrid? grid = HostGrid;
        if (grid == null)
        {
            Debug.LogWarning($"{gameObject.name} has no grid!");
            return false;
        }
        Vector2Int targetCell = grid.GetCoordsFromPosition(targetPosition);
        return GoToCell(targetCell);
    }
    public void ReturnToStation() {
        float sqrDistanceToStation = MathUtils.SqrDistance2D(transform.position, stationPosition);
        if (sqrDistanceToStation< 0.1f * 0.1f)
        {
            // Face the station direction
            transform.rotation = Quaternion.RotateTowards(transform.rotation, stationRotation, stationReachedRotationSpeed * Time.deltaTime);
            return;
        }

        GoToPosition(stationPosition);
    }
    public void StareAtPosition(Vector3 position)
    {
        position.y = transform.position.y;
        lookAtLocation = position;
    }
    public void ClearStareTarget()
    {
        lookAtLocation = null;
    }
    public void RecalculatePath()
    {
        if (gridPath == null) return;
        GoToPosition(gridPath.GetDestination());
    }

    public bool IsFollowingPath()
    {
        return gridPath != null && !gridPath.ReachedDestination(gridPathFollowIndex);
    }
    public bool HasDestination(out Vector3 destination)
    {
        destination = Vector3.zero;
        if (gridPath == null) return false;
        destination = gridPath.GetDestination();
        return true;
    }
    public void RenderPath(IGridPath path)
    {
        var points = path.GetDynamicSteps(transform.position, gridPathFollowIndex).ToArray();
        pathRenderer.positionCount = points.Length;
        pathRenderer.SetPositions(points);
    }

    // Whether they can send and receive messages to/from the hive mind
    public bool IsEnlightened()
    {
        return !HasEffect(Effect.Tag.Buzzed);
    }
    public bool HasVision()
    {
        return !HasEffect(Effect.Tag.Blinded);
    }

    public Transform GetEffectTransform()
    {
        return transform;
    }

    // AI and Communication
    protected float searchTimer = 0.0f;
    public bool Searching => searchTimer > 0.0f;
    public float SearchTimeLeft => searchTimer;
    public float SearchTimeMax => HiveMind.Instance.searchTime;
    protected Vector3? playerPosition = null;
    

    public void OnHiveMindPlayerDetected(Vector3 position)
    {
        if (!IsEnlightened()) return; // can't communicate with hive mind
        // When notified by the hive mind, immediately become aware of target
        Vision.MakeAware();
        PlayerDetectedBehaviour(position);
    }
    public void OnVisionPlayerDetected(Vector3 position)
    {
        if (IsEnlightened())
        {
            HiveMind.Instance.PlayerSpotted(this, position);
        }
        PlayerDetectedBehaviour(position);
    }
    public virtual void PlayerDetectedBehaviour(Vector3 position)
    {
        playerPosition = position;
        searchTimer = HiveMind.Instance.searchTime;
    }

    public IEffectTarget AsEffectTarget()
    {
        return this;
    }

    public void OnDrawGizmos()
    {
        if (gridPath != null)
        {
            List<Vector3> pathPoints = gridPath.GetSteps(transform.position.y);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLineStrip(pathPoints.ToArray(), false);
        }
    }
}
