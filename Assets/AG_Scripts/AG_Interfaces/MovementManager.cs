using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

public class MovementManager : MonoBehaviour
{
    public static MovementManager Instance { get; private set; }

    private List<Unit> allUnits = new List<Unit>();

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

        // THAY ĐỔI MỚI: Tự động gán priority duy nhất cho mỗi đơn vị
        ApplyUniqueUnitPriorities();
    }

    /// <summary>
    /// Áp dụng priority duy nhất cho mỗi đơn vị.
    /// Priority 0 là cao nhất, 99 là thấp nhất.
    /// </summary>
    private void ApplyUniqueUnitPriorities()
    {
        // Sắp xếp các đơn vị nếu muốn một thứ tự gán priority cụ thể (ví dụ: theo tên, theo khoảng cách ban đầu)
        // Hiện tại, chúng ta chỉ lấy theo thứ tự FindObjectsByType trả về.
        // allUnits = allUnits.OrderBy(unit => unit.name).ToList(); // Ví dụ sắp xếp theo tên

        for (int i = 0; i < allUnits.Count; i++)
        {
            // Gán priority từ 0 đến 99. Nếu có nhiều hơn 100 đơn vị, các đơn vị còn lại sẽ nhận priority 99.
            int priority = Mathf.Min(i, 99);
            allUnits[i].SetAgentPriority(priority);
            Debug.Log($"Đã đặt priority cho {allUnits[i].name} thành {priority}");
        }
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

        foreach (Unit unit in allUnits)
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

        foreach (Unit unit in allUnits)
        {
            unit.MoveTo(validTarget);
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

        foreach (Unit unit in allUnits)
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
}