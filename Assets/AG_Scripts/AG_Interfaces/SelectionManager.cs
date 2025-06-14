using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance;

    public List<Unit> selectedUnits = new List<Unit>();

    private void Awake()
    {
        Instance = this;
    }

    public void SelectUnit(Unit unit)
    {
        if (!selectedUnits.Contains(unit))
        {
            selectedUnits.Add(unit);
            unit.SetSelected(true);
        }
    }

    public void DeselectAll()
    {
        foreach (var unit in selectedUnits)
        {
            unit.SetSelected(false);
        }
        selectedUnits.Clear();
    }

    public void IssueMoveCommand(Vector3 target)
    {
        int unitCount = selectedUnits.Count;
        if (unitCount == 0) return;

        // Kiểm tra và tìm điểm hợp lệ trên NavMesh
        Vector3 validTarget = target;
        NavMeshHit hit;
        float maxDistance = 10f; // Bán kính tìm kiếm điểm hợp lệ trên NavMesh
        if (!NavMesh.SamplePosition(target, out hit, maxDistance, NavMesh.AllAreas))
        {
            Debug.LogWarning($"Target {target} is outside NavMesh. Using nearest valid position: {hit.position}");
            validTarget = hit.position;
        }
        else
        {
            validTarget = hit.position;
        }

        // Sắp xếp unit theo khoảng cách đến validTarget (gần nhất đến xa nhất)
        List<Unit> sortedUnits = selectedUnits.OrderBy(unit => Vector3.Distance(unit.transform.position, validTarget)).ToList();

        IssueGridFormation(validTarget, sortedUnits, unitCount);
    }

    void IssueGridFormation(Vector3 target, List<Unit> sortedUnits, int count)
    {
        float spacing = 3f; // Khoảng cách giữa các unit
        int perRow = Mathf.CeilToInt(Mathf.Sqrt(count));

        for (int i = 0; i < count; i++)
        {
            int row = i / perRow;
            int col = i % perRow;

            Vector3 offset = new Vector3(
                (col - perRow / 2f) * spacing,
                (row - perRow / 2f) * spacing,
                0f
            );

            sortedUnits[i].MoveTo(target + offset);
        }
    }

    public void CancelMoveCommand()
    {
        foreach (var unit in selectedUnits)
        {
            unit.CancelMove();
        }
    }
}