using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Cần thiết cho OrderBy
using UnityEngine.AI; // Cần thiết cho NavMesh

public class MovementManager : MonoBehaviour
{
    public static MovementManager Instance { get; private set; }

    [Header("Formation Settings")]
    [Tooltip("Khoảng cách giữa các đơn vị trong đội hình.")]
    public float unitSpacing = 3f;

    private List<Unit> allUnits = new List<Unit>(); // Giữ nguyên, quản lý TẤT CẢ đơn vị

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("MovementManager: Phát hiện nhiều hơn một Instance. Hủy bỏ bản sao này.", this);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Tìm tất cả các Unit trong Scene khi khởi động
        allUnits.AddRange(FindObjectsByType<Unit>(FindObjectsSortMode.None));
        Debug.Log($"MovementManager: Tìm thấy {allUnits.Count} đơn vị trong Scene.");
    }

    /// <summary>
    /// Ra lệnh di chuyển cho TẤT CẢ các đơn vị hiện có đến một vị trí mục tiêu.
    /// </summary>
    /// <param name="target">Vị trí mục tiêu trên thế giới.</param>
    public void IssueMoveCommand(Vector3 target)
    {
        int unitCount = allUnits.Count;
        if (unitCount == 0)
        {
            Debug.LogWarning("Không có đơn vị nào để ra lệnh di chuyển.");
            return;
        }

        // Dừng lệnh bắn của tất cả đơn vị trước khi di chuyển
        foreach (Unit unit in allUnits) // Vẫn lặp qua allUnits
        {
            unit.StopShooting();
        }

        Vector3 validTarget;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 10f, NavMesh.AllAreas))
        {
            validTarget = hit.position;
        }
        else
        {
            Debug.LogWarning($"MovementManager: Không tìm thấy điểm hợp lệ trên NavMesh gần {target}. Lệnh di chuyển bị hủy.");
            return;
        }

        // Sắp xếp các đơn vị theo khoảng cách đến mục tiêu để đơn vị gần nhất ở trung tâm đội hình
        List<Unit> sortedUnits = allUnits.OrderBy(unit => Vector3.Distance(unit.transform.position, validTarget)).ToList(); // Vẫn lặp qua allUnits

        IssueGridFormation(validTarget, sortedUnits, unitCount);
    }

    void IssueGridFormation(Vector3 centerTarget, List<Unit> units, int count)
    {
        if (count == 0) return;

        int unitsPerRow = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(count)));
        int numRows = Mathf.CeilToInt((float)count / unitsPerRow);

        float startX = -((unitsPerRow - 1) * unitSpacing) / 2f;
        float startY = -((numRows - 1) * unitSpacing) / 2f;

        for (int i = 0; i < count; i++)
        {
            int row = i / unitsPerRow;
            int col = i % unitsPerRow;

            float xOffset = startX + col * unitSpacing;
            float yOffset = startY + row * unitSpacing;
            Vector3 offset = new Vector3(xOffset, yOffset, 0f);

            Vector3 destination = centerTarget + offset;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(destination, out hit, unitSpacing * 2f, NavMesh.AllAreas))
            {
                units[i].MoveTo(hit.position);
            }
            else
            {
                Debug.LogWarning($"MovementManager: Không tìm thấy điểm hợp lệ trên NavMesh cho đơn vị {units[i].name} tại {destination}. Đơn vị sẽ không di chuyển.");
            }
        }
    }

    /// <summary>
    /// Ra lệnh cho TẤT CẢ các đơn vị hiện có để bắn vào một vị trí mục tiêu.
    /// </summary>
    /// <param name="lookTarget">Vị trí mục tiêu mà các đơn vị sẽ hướng tới để bắn.</param>
    public void IssueShootCommand(Vector3 lookTarget)
    {
        if (allUnits.Count == 0)
        {
            Debug.LogWarning("Không có đơn vị nào để ra lệnh bắn.");
            return;
        }

        foreach (Unit unit in allUnits) // Vẫn lặp qua allUnits
        {
            unit.ShootAt(lookTarget);
        }
    }

    /// <summary>
    /// Trả về một danh sách chỉ đọc của tất cả các đơn vị hiện đang được quản lý.
    /// </summary>
    /// <returns>IEnumerable<Unit> của tất cả các đơn vị.</returns>
    public IEnumerable<Unit> GetAllUnits()
    {
        return allUnits.AsReadOnly();
    }

    // LOẠI BỎ TOÀN BỘ CÁC PHƯƠNG THỨC SAU: SelectUnit, DeselectUnit, ClearSelection
    // Nếu bạn có các phương thức đó ở đây, hãy xóa chúng đi.
}