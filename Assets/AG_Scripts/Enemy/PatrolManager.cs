using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class PatrolManager : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float patrolRadius = 10f; // Bán kính khu vực tuần tra
    public int maxPatrolPoints = 5; // Số điểm tuần tra tối đa
    public float minDwellTime = 0f; // Thời gian chờ tối thiểu
    public float maxDwellTime = 3f; // Thời gian chờ tối đa
    public float obstacleAvoidanceDistance = 1f; // Khoảng cách tối thiểu tới vật cản
    public LayerMask obstacleLayer; // Layer của vật cản

    private List<Vector3> patrolPoints = new List<Vector3>(); // Danh sách điểm tuần tra
    private int currentPatrolIndex = 0;
    private float dwellTimer = 0f;
    private bool isWaiting = false;

    void Start()
    {
        GeneratePatrolPoints();
        if (patrolPoints.Count == 0)
        {
            Debug.LogWarning("Không tạo được điểm tuần tra cho " + gameObject.name);
        }
    }

    public void GeneratePatrolPoints()
    {
        patrolPoints.Clear();
        Vector2 center = transform.position;

        for (int i = 0; i < maxPatrolPoints; i++)
        {
            bool validPoint = false;
            int attempts = 0;
            const int maxAttempts = 10;

            while (!validPoint && attempts < maxAttempts)
            {
                Vector2 randomPoint = center + Random.insideUnitCircle * patrolRadius;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, NavMesh.AllAreas))
                {
                    // Kiểm tra khoảng cách tới vật cản
                    Collider2D obstacle = Physics2D.OverlapCircle(hit.position, obstacleAvoidanceDistance, obstacleLayer);
                    if (obstacle == null)
                    {
                        patrolPoints.Add(hit.position);
                        validPoint = true;
                    }
                }
                attempts++;
            }

            if (!validPoint)
            {
                Debug.LogWarning("Không thể tạo điểm tuần tra thứ " + (i + 1) + " cho " + gameObject.name);
            }
        }
    }

    public bool GetNextPatrolPoint(out Vector3 point)
    {
        point = Vector3.zero;
        if (patrolPoints.Count == 0) return false;

        if (isWaiting)
        {
            dwellTimer -= Time.deltaTime;
            if (dwellTimer <= 0f)
            {
                isWaiting = false;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
            }
            return false;
        }

        point = patrolPoints[currentPatrolIndex];
        return true;
    }

    public void OnReachPatrolPoint()
    {
        isWaiting = true;
        dwellTimer = Random.Range(minDwellTime, maxDwellTime);
    }

    void OnDrawGizmos()
    {
        // Vẽ khu vực tuần tra
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        // Vẽ các điểm tuần tra
        Gizmos.color = Color.cyan;
        foreach (Vector3 point in patrolPoints)
        {
            Gizmos.DrawSphere(point, 0.3f);
        }
    }
}