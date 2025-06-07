
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private NavMeshAgent agent;

    public GameObject selectionCircle;

    public bool IsSelected { get; private set; }

    private void Start()
    {
        targetPosition = transform.position;
        SetSelected(false);

        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.angularSpeed = 360f; // Tăng tốc độ xoay để phản hồi nhanh hơn (nếu cần)
            agent.acceleration = 8f; // Đảm bảo tăng tốc hợp lý
            agent.stoppingDistance = 0.1f; // Khoảng cách dừng nhỏ để unit dừng chính xác
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }
        else
        {
            Debug.LogWarning("NavMeshAgent not found on " + gameObject.name + ". Using manual movement.");
        }
    }
    private void Update()
    {
        if (isMoving)
        {
            if (agent != null)
            {
                agent.SetDestination(targetPosition);

                // Chỉ xoay khi có hướng di chuyển rõ ràng và chưa gần đích
                if (Vector3.Distance(transform.position, targetPosition) > agent.stoppingDistance + 0.1f)
                {
                    Vector3 direction = agent.desiredVelocity;
                    if (direction.sqrMagnitude > 0.5f) // Ngưỡng cao để tránh nhiễu
                    {
                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                    }
                }
            }
            else
            {
                // Di chuyển thủ công nếu không có NavMeshAgent
                Vector3 direction = (targetPosition - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;

                // Chỉ xoay khi có hướng di chuyển rõ ràng và chưa gần đích
                if (Vector3.Distance(transform.position, targetPosition) > 0.1f && direction.sqrMagnitude > 0.5f)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
            }

            // Dừng di chuyển khi gần đích
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
                if (agent != null)
                {
                    agent.isStopped = true; // Dừng NavMeshAgent hoàn toàn
                    agent.velocity = Vector3.zero; // Xóa vận tốc để tránh nhiễu
                }
            }
        }
    }


    public void SetSelected(bool selected)
    {
        if (selectionCircle != null)
            selectionCircle.SetActive(selected);
    }

    public void MoveTo(Vector3 position)
    {
        targetPosition = position;
        isMoving = true;
    }

    public void CancelMove()
    {
        isMoving = false;
    }
}
