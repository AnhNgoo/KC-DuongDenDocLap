using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMove : MonoBehaviour
{
    private NavMeshAgent agent;
    private Camera mainCam;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        mainCam = Camera.main;

        // Chuyển NavMeshAgent sang chế độ dùng trong 2D
        agent.updateRotation = false;
        agent.updateUpAxis = false; // Quan trọng: tránh đổi trục Y
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            agent.SetDestination(mousePos);

        }
    }
}
