using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitEnemyAK : MonoBehaviour, ITakeDamage
{
    [SerializeField] private double health;
    [SerializeField] private double maxHealth = 100;

    public double Health
    {
        get => health;
        set
        {
            health = Mathf.Clamp((float)value, 0, (float)maxHealth);
        }
    }

    [SerializeField] private float speed = 5;
    [SerializeField] private float attackDistance = 5f;
    [SerializeField] private float chaseDistance = 10f;
    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private LayerMask enemyLayerMask; // Layer của đồng minh
    [SerializeField] private List<Transform> patrolPoints; // Danh sách các điểm tuần tra
    [SerializeField] private float lookAroundAngle = 45f; // Góc xoay khi quan sát
    [SerializeField] private float lookAroundDuration = 2f; // Thời gian quan sát
    private float detectDistance;
    private int nextIndex = 0; // Chỉ số điểm tuần tra tiếp theo
    private bool isFirstPatrol = true; // Kiểm tra lần tuần tra đầu tiên
    private bool isLookingAround = false; // Trạng thái đang quan sát
    private bool isRespondingToCall = false; // Trạng thái phản hồi lời gọi đồng minh
    private Collider2D targetPlayer; // Player đang đuổi theo
    private NavMeshAgent agent;

    void OnEnable()
    {
        Debug.Log("UnitEnemyAK enabled: " + gameObject.name);
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = speed;
        detectDistance = chaseDistance;
    }

    void Update()
    {
        DetectPlayer();
    }

    void DetectPlayer()
    {
        if (targetPlayer != null)
        {
            float distance = Vector2.Distance(transform.position, targetPlayer.transform.position);
            if (distance <= chaseDistance)
            {
                Vector2 direction = (targetPlayer.transform.position - transform.position).normalized;
                if (distance <= attackDistance)
                {
                    AttackPlayer(direction, distance, targetPlayer);
                    isFirstPatrol = true;
                }
                else
                {
                    ChasePlayer(targetPlayer, isRespondingToCall);
                    isFirstPatrol = true;
                    if (!isRespondingToCall)
                    {
                        CallForHelp(targetPlayer);
                    }
                }
                return;
            }
            else
            {
                targetPlayer = null;
                isRespondingToCall = false;
            }
        }

        Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, detectDistance, playerLayerMask);
        if (players.Length > 0)
        {
            float minDistance = float.MaxValue;
            Collider2D closestPlayer = null;
            foreach (Collider2D player in players)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPlayer = player;
                }
            }

            targetPlayer = closestPlayer;
            Vector2 direction = (targetPlayer.transform.position - transform.position).normalized;

            if (minDistance <= attackDistance)
            {
                AttackPlayer(direction, minDistance, targetPlayer);
                isFirstPatrol = true;
            }
            else if (minDistance <= chaseDistance)
            {
                ChasePlayer(targetPlayer, false);
                isFirstPatrol = true;
                if (!isRespondingToCall)
                {
                    CallForHelp(targetPlayer);
                }
            }
            else
            {
                isRespondingToCall = false;
                Patrol();
            }
        }
        else
        {
            targetPlayer = null;
            isRespondingToCall = false;
            Patrol();
        }
    }

    private void CallForHelp(Collider2D player)
    {
        Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, 20, enemyLayerMask);
        foreach (Collider2D ally in allies)
        {
            if (ally.gameObject != gameObject)
            {
                UnitEnemyAK allyScript = ally.GetComponent<UnitEnemyAK>();
                if (allyScript != null && allyScript.enabled)
                {
                    float distanceToPlayer = Vector2.Distance(ally.transform.position, player.transform.position);
                    if (distanceToPlayer > allyScript.attackDistance)
                    {
                        allyScript.ChasePlayer(player, true);
                    }
                }
            }
        }
    }

    private void AttackPlayer(Vector3 direction, float distanceToPlayer, Collider2D player)
    {
        RaycastHit2D obstacle = Physics2D.Raycast(transform.position, direction, distanceToPlayer, obstacleLayerMask);

        if (obstacle.collider != null)
        {
            ChasePlayer(player, isRespondingToCall);
            return;
        }

        agent.ResetPath();

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(Vector3.forward, direction) * Quaternion.Euler(0, 0, 180); // Xoay 180 độ để hướng xuống dưới
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    private void ChasePlayer(Collider2D player, bool isCalled = false)
    {
        isRespondingToCall = isCalled;
        agent.SetDestination(player.transform.position);
        Vector3 moveDirection = agent.velocity.normalized;
        if (moveDirection != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(Vector3.forward, moveDirection) * Quaternion.Euler(0, 0, 180); // Xoay 180 độ để hướng xuống dưới
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Count == 0) return;

        if (isLookingAround) return;

        if (!agent.hasPath || Vector2.Distance(transform.position, patrolPoints[nextIndex].position) < 0.5f)
        {
            isFirstPatrol = false;
            agent.ResetPath();
            StartCoroutine(LookAround());
            return;
        }

        if (isFirstPatrol)
        {
            agent.SetDestination(patrolPoints[nextIndex].position);
            isFirstPatrol = false;
        }

        Vector3 moveDirection = agent.velocity.normalized;
        if (moveDirection != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(Vector3.forward, moveDirection) * Quaternion.Euler(0, 0, 180); // Xoay 180 độ để hướng xuống dưới
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    private IEnumerator LookAround()
    {
        isLookingAround = true;

        Quaternion originalRotation = transform.rotation;
        Quaternion leftRotation = originalRotation * Quaternion.Euler(0, 0, lookAroundAngle);
        Quaternion rightRotation = originalRotation * Quaternion.Euler(0, 0, -lookAroundAngle);
        float elapsedTime = 0f;
        float halfDuration = lookAroundDuration / 4f;

        while (elapsedTime < halfDuration)
        {
            transform.rotation = Quaternion.Slerp(originalRotation, leftRotation, elapsedTime / halfDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            transform.rotation = Quaternion.Slerp(leftRotation, rightRotation, elapsedTime / halfDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            transform.rotation = Quaternion.Slerp(rightRotation, originalRotation, elapsedTime / halfDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (nextIndex == patrolPoints.Count - 1)
        {
            nextIndex = 0;
        }
        else
        {
            nextIndex++;
        }

        agent.SetDestination(patrolPoints[nextIndex].position);
        isLookingAround = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;
        if (Health == 0) Destroy(gameObject);
    }
}