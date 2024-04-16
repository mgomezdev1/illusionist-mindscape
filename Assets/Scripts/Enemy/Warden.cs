using UnityEngine;
using UnityEngine.Assertions;

public class Warden : Enemy
{
    [SerializeField] private Transform[] patrolPoints;
    protected Vector3? lastPlayerSeenLocation;
    protected int currentPatrolIndex = 0;

    [SerializeField] protected float patrolReachTime;
    [SerializeField] protected float rotationTime;
    [SerializeField] protected float patrolDepartTime;
    [SerializeField] protected float stopDistance = 0.1f;
    [SerializeField] protected float patrolSpeed = 2.0f;
    private float regularSpeed = 0.0f;
    protected float waitTime;

    [Header("Attack Behaviour")]
    [SerializeField] protected float attackDistance = 0.75f;

    public enum WardenState
    {
        Moving,
        Reaching,
        Turning,
        Departing
    }
    [SerializeField] protected WardenState state = WardenState.Moving;

    protected override void Awake()
    {
        base.Awake();
        regularSpeed = baseSpeed;
    }

    protected override void Update()
    {
        base.Update();
        if (GameController.Instance.IsPaused) return;

        if (searchTimer > 0)
        {
            baseSpeed = regularSpeed;
            Search();
        }
        else
        {
            baseSpeed = patrolSpeed;
            Patrol();
        }
    }

    protected void Search()
    {
        //playerPosition is the target to reach
        if (!playerPosition.HasValue) return;
        Vector3 target = playerPosition.Value;
        if ((transform.position - target).sqrMagnitude < attackDistance * attackDistance)
        {
            if (!Vision.IsPlayerInLoS())
            {
                LookAround();
            }
            else
            {
                AttackBehaviour(target);
            }
        }
        else
        {
            GoToPosition(target);
        }
    }
    protected void Patrol()
    {
        if (patrolPoints.Length == 0)
        {
            ReturnToStation();
            return;
        }

        Vector3 patrolTarget = patrolPoints[currentPatrolIndex].position;
        if (state == WardenState.Moving)
        {
            if (!IsFollowingPath())
            {
                GoToPosition(patrolTarget);
            }
            float sqrDistanceToTarget = MathUtils.SqrDistance2D(transform.position, patrolTarget);
            if (sqrDistanceToTarget < (stopDistance + 0.5f) * (stopDistance + 0.5f))
            {
                state = WardenState.Reaching;
                waitTime = patrolReachTime;
            }
            return;
        }
        if (waitTime > 0)
        {
            waitTime -= Time.deltaTime;
            if (state == WardenState.Turning)
            {
                Vector3 relativePos = patrolPoints[currentPatrolIndex].position - transform.position;
                relativePos.y = 0;
                Quaternion targetRotation = Quaternion.LookRotation(relativePos, Vector3.up);
                float rotationLeft = Quaternion.Angle(transform.rotation, targetRotation);
                float angularVelocity = rotationLeft / waitTime;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, angularVelocity * Time.deltaTime);
            }
            if (waitTime < 0)
            {
                switch (state)
                {
                    case WardenState.Moving:
                        // should never reach this
                        Assert.IsTrue(false);
                        break;
                    case WardenState.Reaching:
                        // Start preparing to go to the next patrol point
                        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                        state = WardenState.Turning;
                        waitTime = rotationTime;
                        break;
                    case WardenState.Turning:
                        state = WardenState.Departing;
                        waitTime = patrolDepartTime;
                        break;
                    case WardenState.Departing: 
                        state = WardenState.Moving;
                        GoToPosition(patrolPoints[currentPatrolIndex].position);
                        break;
                }
            }
        }
        
    }

    protected void LookAround()
    {
        animator.SetTrigger("LookAround");
    }
    [SerializeField] protected float attackCooldown = 3.0f;
    [SerializeField] protected float attackDuration = 1.75f;
    protected float attackTime = 0.0f;
    protected void AttackBehaviour(Vector3 target)
    {
        if (attackTime > 0)
        {
            // cooldown
            attackTime -= Time.deltaTime;
            return;
        }
        attackTime = attackCooldown;
        //stopTime = attackDuration;
        transform.LookAt(target);
        animator.SetTrigger("Attack");
    }
}