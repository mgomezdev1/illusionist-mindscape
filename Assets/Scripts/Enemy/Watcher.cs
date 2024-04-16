using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Rendering;

#nullable enable
public class Watcher : Enemy
{
    protected OccupancyMap? omap;
    [SerializeField] protected float omapUpdateInterval = 0.1f;
    private float omapUpdateTime = 0.0f;
    [SerializeField] protected float repathInterval;
    [SerializeField] protected float stareHoldDistance = 5;
    private float repathTime = 0.0f;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
        if (GameController.Instance.IsPaused) return;

        if (searchTimer > 0)
        {
            Search();
        }
        else
        {
            Watch();
        }
    }

    protected void Watch()
    {
        ClearStareTarget();
        ReturnToStation();
    }
    protected void Search()
    {
        // While searching, ensure we update the occupancy map
        if (HostGrid == null) return;
        if (omap == null)
        {
            if (HostGrid != null)
            {
                omap = new OccupancyMap(HostGrid);
            }
            else { return; }
        }
        // omap is not null at this point

        omapUpdateTime -= Time.deltaTime;
        if (omapUpdateTime < 0.0f)
        {
            omapUpdateTime = omapUpdateInterval;
            omap.RunUpdate(omapUpdateInterval, Vision);
        }

        // Then go to the highest-probability cell
        // Or, failing that, return to station
        Vector3 suspectedPosition = HostGrid.GetPositionFromCoords(omap.BestCell);
        StareAtPosition(suspectedPosition);
        repathTime -= Time.deltaTime;
        if (repathTime < 0.0f)
        {
            repathTime = repathInterval;
            Vector2Int currentCell = HostGrid.GetCoordsFromPosition(transform.position);
            Vector2Int targetCell = omap.BestCell;
            Vector3 intendedDestination = suspectedPosition;
            if (omap.IsCellVisible(targetCell))
            {
                Vector3 here = transform.position;
                float distanceToDestination = Mathf.Sqrt(MathUtils.SqrDistance2D(here, intendedDestination)); ;
                float actualBestDistance = Mathf.Min(distanceToDestination, stareHoldDistance);
                Vector3 offsetVector = (here - intendedDestination).normalized * actualBestDistance;
                intendedDestination += offsetVector;
            }

            if (!GoToPosition(intendedDestination))
            {
                Debug.LogWarning($"{gameObject.name} was unable to reach occupancy map best cell <{omap.BestCell}>, with value {omap.BestCellValue:%}; Travelling to station instead");
                GoToPosition(stationPosition);
                ClearStareTarget();
            }
        }
    }

    public override void PlayerDetectedBehaviour(Vector3 position)
    {
        base.PlayerDetectedBehaviour(position);
        omap?.SetTargetPosition(position);
    }

    private void OnDrawGizmosSelected()
    {
        omap?.DrawOccupancyGizmos();
        omap?.DrawVisibilityGizmos();
    }
}