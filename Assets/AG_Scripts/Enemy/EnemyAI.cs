using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float minSpeed = 2.5f; // Tốc độ tối thiểu
    public float maxSpeed = 4.5f; // Tốc độ tối đa

    [Header("Detection Settings")]
    public float detectionRange = 5f; // Phạm vi phát hiện
    public float fovAngle = 90f; // Góc nhìn (FOV)
    public LayerMask playerLayer; // Layer của lính player
    public LayerMask obstacleLayer; // Layer của vật cản
    private Transform target; // Mục tiêu hiện tại

    [Header("Team AI Settings")]
    public float alertRange = 10f; // Phạm vi thông báo đồng đội
    public LayerMask enemyLayer; // Layer của enemy

    private NavMeshAgent agent;
    private PatrolManager patrolManager;
    private bool isChasingPlayer = false;
    private static List<EnemyAI> allEnemies = new List<EnemyAI>();

    void OnEnable()
    {
        allEnemies.Add(this);
    }

    void OnDisable()
    {
        allEnemies.Remove(this);
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent không được gắn trên GameObject này!");
            return;
        }
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = Random.Range(minSpeed, maxSpeed);

        patrolManager = GetComponent<PatrolManager>();
        if (patrolManager == null)
        {
            Debug.LogError("PatrolManager không được gắn trên GameObject này!");
            return;
        }
    }

    void Update()
    {
        if (agent == null || patrolManager == null) return;

        // Phát hiện lính player
        Transform newTarget = DetectPlayer();
        if (newTarget != null)
        {
            isChasingPlayer = true;
            target = newTarget;
            AlertNearbyEnemies(target);
            agent.SetDestination(target.position);
        }
        else if (isChasingPlayer && target != null)
        {
            if (!IsTargetValid())
            {
                isChasingPlayer = false;
                target = null;

                patrolManager.ForceRestart(); // ⬅️ GỌI Ở ĐÂY
                patrolManager.OnReachPatrolPoint();
                Vector3 nextPoint;
                if (patrolManager.GetNextPatrolPoint(out nextPoint))
                {
                    agent.SetDestination(nextPoint);
                }

                return; // tránh chạy tiếp phần tuần tra bên dưới
            }
            else
            {
                agent.SetDestination(target.position);
            }
        }

        else
        {
            // Tiếp tục tuần tra nếu không đuổi theo player
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                patrolManager.OnReachPatrolPoint(); // bắt đầu chờ
            }

            if (patrolManager.UpdatePatrolWaiting()) // kiểm tra đã chờ xong chưa
            {
                Vector3 nextPoint;
                if (patrolManager.GetNextPatrolPoint(out nextPoint))
                {
                    agent.SetDestination(nextPoint);
                }
            }
        }

        // Xoay sprite theo hướng di chuyển
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            transform.up = agent.velocity.normalized;
        }
    }


    Transform DetectPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange, playerLayer);
        Transform closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            Vector2 directionToTarget = (hit.transform.position - transform.position).normalized;
            float distanceToTarget = Vector2.Distance(transform.position, hit.transform.position);

            // Kiểm tra FOV
            float angleToTarget = Vector2.Angle(transform.up, directionToTarget);
            if (angleToTarget < fovAngle * 0.5f)
            {
                // Kiểm tra vật cản
                RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleLayer);
                if (raycastHit.collider == null)
                {
                    if (distanceToTarget < closestDistance)
                    {
                        closestDistance = distanceToTarget;
                        closestTarget = hit.transform;
                    }
                }
            }
        }

        return closestTarget;
    }

    bool IsTargetValid()
    {
        if (target == null) return false;

        float distance = Vector2.Distance(transform.position, target.position);
        if (distance > detectionRange) return false;

        // Kiểm tra vật cản chắn giữa enemy và player
        Vector2 directionToTarget = (target.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, distance, obstacleLayer);

        if (hit.collider != null)
        {
            // Có vật cản chắn giữa
            return false;
        }

        // Kiểm tra xem player còn trong góc nhìn
        float angleToTarget = Vector2.Angle(transform.up, directionToTarget);
        if (angleToTarget > fovAngle * 0.5f)
        {
            return false;
        }

        return true;
    }


    void AlertNearbyEnemies(Transform target)
    {
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, alertRange, enemyLayer);
        foreach (Collider2D enemy in nearbyEnemies)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null && enemyAI != this)
            {
                enemyAI.ReceiveAlert(target);
            }
        }
    }

    public void ReceiveAlert(Transform newTarget)
    {
        if (!isChasingPlayer && newTarget != null)
        {
            isChasingPlayer = true;
            target = newTarget;
            agent.SetDestination(target.position);
        }
    }

    void OnDrawGizmos()
    {
        // Vẽ phạm vi phát hiện
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Vẽ FOV
        Gizmos.color = Color.yellow;
        Vector3 leftFov = Quaternion.Euler(0, 0, fovAngle * 0.5f) * transform.up * detectionRange;
        Vector3 rightFov = Quaternion.Euler(0, 0, -fovAngle * 0.5f) * transform.up * detectionRange;
        Gizmos.DrawLine(transform.position, transform.position + leftFov);
        Gizmos.DrawLine(transform.position, transform.position + rightFov);

        // Vẽ phạm vi thông báo đồng đội
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, alertRange);
    }
}