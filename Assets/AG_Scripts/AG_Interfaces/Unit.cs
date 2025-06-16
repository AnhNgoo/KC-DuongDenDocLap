using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Unit : MonoBehaviour
{
    public enum UnitType { Soldier, Tank, Helicopter } // THAY ĐỔI MỚI: Thêm Helicopter

    [Header("Unit Configuration")]
    public UnitType unitType = UnitType.Soldier;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    [SerializeField] private float minDistance = 1f; // Khoảng cách tối thiểu giữa các đơn vị để dừng lại
    [SerializeField] private float rotationSpeed = 10f; // Tốc độ quay của thân/tháp pháo

    [Header("Combat Settings")]
    [SerializeField] private float shootLookResetTime = 2f; // Thời gian giữ hướng nhìn sau khi bắn
    [SerializeField] private Transform turretTransform; // Tháp pháo cho Tank
    [SerializeField] private Shoot unitShooter; // Tham chiếu đến script Shoot

    // --- BIẾN PRIVATE ---
    private Vector3 targetPosition;
    private Vector3? lookTarget = null;
    private bool isMoving = false;
    private bool isPaused = false;
    private bool isShooting = false;
    private NavMeshAgent agent; // Vẫn giữ NavMeshAgent cho Soldier/Tank
    private Coroutine shootLookResetCoroutine;

    private float currentManualShootCooldown = 0f;
    private bool canShootManual = true;

    private Dictionary<UnitType, GameObject> unitVisuals = new Dictionary<UnitType, GameObject>();

    private void Awake()
    {
        // NavMeshAgent chỉ cần thiết cho Soldier và Tank
        if (unitType == UnitType.Soldier || unitType == UnitType.Tank)
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                Debug.LogWarning($"Unit: {gameObject.name} ({unitType}) không có NavMeshAgent. Di chuyển có thể không hoạt động đúng.");
            }
        }
        else // Cho Helicopter, chúng ta sẽ không sử dụng NavMeshAgent
        {
            agent = null; // Đảm bảo agent là null cho Helicopter
        }

        FindAndInitializeVisuals();
        UpdateVisuals();
    }

    private void Start()
    {
        targetPosition = transform.position;
        if (agent != null) // Chỉ cấu hình agent nếu nó tồn tại (cho Soldier/Tank)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = 0.1f;
            agent.updateRotation = false;
            agent.angularSpeed = 360f;
            agent.acceleration = 8f;
            agent.updateUpAxis = false;
        }

        // Đảm bảo unitShooter được gán
        if (unitShooter == null)
        {
            unitShooter = GetComponentInChildren<Shoot>();
            if (unitShooter == null)
            {
                Debug.LogError($"Unit: {gameObject.name} không tìm thấy script Shoot trong các GameObject con. Đơn vị sẽ không thể bắn.");
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (unitShooter == null)
        {
            unitShooter = GetComponentInChildren<Shoot>();
        }
        // Để đảm bảo NavMeshAgent được khởi tạo đúng trong Editor
        if ((unitType == UnitType.Soldier || unitType == UnitType.Tank) && GetComponent<NavMeshAgent>() == null)
        {
            // Có thể cảnh báo người dùng cần thêm NavMeshAgent
            Debug.LogWarning($"Unit: {gameObject.name} (UnitType: {unitType}) cần một NavMeshAgent component.");
        }
        if (unitType == UnitType.Helicopter && GetComponent<NavMeshAgent>() != null)
        {
            // Có thể cảnh báo người dùng nên xóa NavMeshAgent
            Debug.LogWarning($"Unit: {gameObject.name} (UnitType: {unitType}) không nên có NavMeshAgent component. Hãy xem xét xóa nó.");
        }

        if (unitVisuals.Count == 0 || unitVisuals.Values.Any(v => v == null)) // Kiểm tra cả null entries
        {
            FindAndInitializeVisuals();
        }
        UpdateVisuals();
    }
#endif

    private void FindAndInitializeVisuals()
    {
        unitVisuals.Clear();
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Unit")) // Nên dùng một tag cụ thể hơn cho visual, ví dụ "UnitVisual"
            {
                // Sử dụng tên child để phân biệt loại UnitType
                if (System.Enum.TryParse(child.name, out UnitType type))
                {
                    unitVisuals[type] = child.gameObject;
                    child.gameObject.SetActive(false);
                }
            }
        }
    }

    private void UpdateVisuals()
    {
        if (unitVisuals.Count == 0) return;
        foreach (var visualEntry in unitVisuals)
        {
            // Kiểm tra visualEntry.Value != null trước khi truy cập
            if (visualEntry.Value != null)
            {
                bool shouldBeActive = visualEntry.Key == unitType;
                if (visualEntry.Value.activeSelf != shouldBeActive)
                {
                    visualEntry.Value.SetActive(shouldBeActive);
                }
            }
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();

        if (!canShootManual)
        {
            currentManualShootCooldown -= Time.deltaTime;
            if (currentManualShootCooldown <= 0)
            {
                canShootManual = true;
            }
        }
    }

    #region Updated Logic with 'Shoot' terminology

    private void HandleMovement()
    {
        if (!isMoving) return;

        // Xử lý di chuyển cho Soldier/Tank (dùng NavMeshAgent)
        if (unitType == UnitType.Soldier || unitType == UnitType.Tank)
        {
            if (agent == null) return;

            isPaused = ShouldPause(); // Kiểm tra xung đột với đơn vị khác
            agent.isStopped = isPaused;

            if (!isPaused)
            {
                agent.SetDestination(targetPosition);
            }
            else
            {
                agent.velocity = Vector3.zero; // Dừng hẳn khi bị tạm dừng
            }

            // Kiểm tra xem đã đến đích chưa
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                isMoving = false;
                isPaused = false;
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
        }
        // Xử lý di chuyển cho Helicopter (di chuyển trực tiếp)
        else if (unitType == UnitType.Helicopter)
        {
            // Helicopter không bị dừng bởi va chạm với đồng đội trong ShouldPause() ở đây
            // Nếu bạn muốn Helicopter tránh va chạm, cần implement logic riêng

            // Tính toán hướng di chuyển
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            // Di chuyển
            transform.position += directionToTarget * moveSpeed * Time.deltaTime;

            // Kiểm tra xem đã đến đích chưa (hoặc đủ gần)
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f) // Một ngưỡng nhỏ
            {
                isMoving = false;
                transform.position = targetPosition; // Đảm bảo đến đúng vị trí
            }
        }
    }

    private void HandleRotation()
    {
        Vector3 currentMoveDirection = Vector3.zero;

        if (unitType == UnitType.Soldier || unitType == UnitType.Tank)
        {
            if (agent != null && agent.desiredVelocity.sqrMagnitude > 0.01f)
            {
                currentMoveDirection = agent.desiredVelocity.normalized;
            }
        }
        else if (unitType == UnitType.Helicopter)
        {
            if (isMoving)
            {
                currentMoveDirection = (targetPosition - transform.position).normalized;
            }
        }

        // Xoay thân khi di chuyển hoặc không bắn
        if (currentMoveDirection != Vector3.zero && (unitType == UnitType.Tank || unitType == UnitType.Helicopter || !isShooting))
        {
            RotateBody(currentMoveDirection);
        }

        // Xoay hướng bắn khi đang bắn
        if (isShooting && lookTarget.HasValue)
        {
            Vector3 shootDirection = (lookTarget.Value - transform.position).normalized;
            if (unitType == UnitType.Soldier || unitType == UnitType.Helicopter)
            {
                RotateBody(shootDirection); // Soldier và Helicopter xoay toàn bộ thân để bắn
            }
            else if (unitType == UnitType.Tank)
            {
                RotateTurret(shootDirection); // Tank xoay tháp pháo
            }
        }
        // Nếu là Tank và không bắn, tháp pháo quay theo hướng di chuyển
        else if (unitType == UnitType.Tank && currentMoveDirection != Vector3.zero)
        {
            RotateTurret(currentMoveDirection);
        }
    }

    private void RotateBody(Vector3 direction)
    {
        if (direction == Vector3.zero) return;
        Quaternion lookRotation = Quaternion.LookRotation(Vector3.forward, direction) * Quaternion.Euler(0, 0, 180);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void RotateTurret(Vector3 direction)
    {
        if (turretTransform == null || direction == Vector3.zero) return;
        Quaternion lookRotation = Quaternion.LookRotation(Vector3.forward, direction) * Quaternion.Euler(0, 0, 180);
        turretTransform.rotation = Quaternion.Slerp(turretTransform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    // ShouldPause chỉ áp dụng cho đơn vị dùng NavMeshAgent
    private bool ShouldPause()
    {
        if (MovementManager.Instance == null) return false;
        if (unitType == UnitType.Helicopter) return false; // Helicopter không tạm dừng như Soldier/Tank

        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        foreach (Unit otherUnit in MovementManager.Instance.GetAllUnits())
        {
            if (otherUnit != this && (otherUnit.unitType == UnitType.Soldier || otherUnit.unitType == UnitType.Tank))
            {
                Vector3 toOther = otherUnit.transform.position - transform.position;
                if (toOther.magnitude < minDistance && Vector3.Angle(moveDirection, toOther) < 90f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void MoveTo(Vector3 position)
    {
        targetPosition = position;
        isMoving = true;
        isPaused = false;
        StopShooting();

        if (unitType == UnitType.Soldier || unitType == UnitType.Tank)
        {
            if (agent != null)
            {
                agent.isStopped = false;
            }
        }
        // Đối với Helicopter, không cần NavMeshAgent
    }

    /// <summary>
    /// Ra lệnh cho đơn vị bắn vào một vị trí cụ thể.
    /// </summary>
    public void ShootAt(Vector3 position)
    {
        lookTarget = position;

        if (canShootManual && unitShooter != null)
        {
            float fireRateFromShooter = unitShooter.FireBullet(lookTarget.Value);

            if (fireRateFromShooter > 0)
            {
                currentManualShootCooldown = 1f / fireRateFromShooter;
            }
            else
            {
                currentManualShootCooldown = 0f;
            }
            canShootManual = false;
        }

        if (shootLookResetCoroutine != null) { StopCoroutine(shootLookResetCoroutine); }
        shootLookResetCoroutine = StartCoroutine(ShootLookResetCoroutine());
        isShooting = true;
    }

    public void StopShooting()
    {
        isShooting = false;
        lookTarget = null;
        canShootManual = true;
        currentManualShootCooldown = 0f;
        if (shootLookResetCoroutine != null) { StopCoroutine(shootLookResetCoroutine); shootLookResetCoroutine = null; }
    }

    private IEnumerator ShootLookResetCoroutine()
    {
        yield return new WaitForSeconds(shootLookResetTime);
        isShooting = false;
        lookTarget = null;
        shootLookResetCoroutine = null;
    }
    #endregion
}