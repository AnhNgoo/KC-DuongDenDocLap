using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Unit : MonoBehaviour
{
    public enum UnitType { Soldier, Tank }

    [Header("Unit Configuration")]
    public UnitType unitType = UnitType.Soldier;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f; // Tốc độ quay của thân/tháp pháo

    [Header("Combat Settings")]
    [SerializeField] private float shootLookResetTime = 2f; // Thời gian giữ hướng nhìn sau khi bắn
    [SerializeField] private Transform turretTransform; // Tháp pháo cho Tank
    [SerializeField] private Shoot unitShooter; // Tham chiếu đến script Shoot

    // --- BIẾN PRIVATE ---
    private Vector3 targetPosition;
    private Vector3? lookTarget = null;
    private bool isMoving = false;
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

        FindAndInitializeVisuals();
        UpdateVisuals();
    }

    private void Start()
    {
        targetPosition = transform.position;
        if (agent != null) // Chỉ cấu hình agent nếu nó tồn tại (cho Soldier/Tank)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = 5f; // Giữ lại stoppingDistance 5f như code bạn cung cấp
            agent.updateRotation = false;
            agent.angularSpeed = 360f;
            agent.acceleration = 8f;
            agent.updateUpAxis = false;
            // agent.priority được gán từ MovementManager.cs
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
            Debug.LogWarning($"Unit: {gameObject.name} (UnitType: {unitType}) cần một NavMeshAgent component.");
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
            if (child.CompareTag("Unit"))
            {
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

            agent.SetDestination(targetPosition);

            // Kiểm tra xem đã đến đích chưa
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                isMoving = false;
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
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

        // Xoay thân khi di chuyển hoặc không bắn
        if (currentMoveDirection != Vector3.zero && (unitType == UnitType.Tank || !isShooting))
        {
            RotateBody(currentMoveDirection);
        }

        // Xoay hướng bắn khi đang bắn
        if (isShooting && lookTarget.HasValue)
        {
            Vector3 shootDirection = (lookTarget.Value - transform.position).normalized;
            if (unitType == UnitType.Soldier)
            {
                RotateBody(shootDirection); // Soldier xoay toàn bộ thân để bắn
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

    public void MoveTo(Vector3 position)
    {
        targetPosition = position;
        isMoving = true;
        StopShooting();

        if (unitType == UnitType.Soldier || unitType == UnitType.Tank)
        {
            if (agent != null)
            {
                agent.isStopped = false;
            }
        }
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

    /// <summary>
    /// Đặt priority cho NavMeshAgent của đơn vị này. Priority thấp hơn = ưu tiên cao hơn.
    /// </summary>
    /// <param name="priority">Giá trị priority từ 0-99.</param>
    public void SetAgentPriority(int priority)
    {
        if (agent != null)
        {
            agent.avoidancePriority = Mathf.Clamp(priority, 0, 99); // Đảm bảo priority nằm trong khoảng hợp lệ
        }
    }
    #endregion
}