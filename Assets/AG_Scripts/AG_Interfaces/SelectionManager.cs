using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance;

    public List<Unit> selectedUnits = new List<Unit>();

    public enum FormationType
    {
        Grid,
        Circle,
        Triangle,
        Random
    }

    public FormationType currentFormation = FormationType.Circle;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        HandleFormationSwitch();
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

        switch (currentFormation)
        {
            case FormationType.Grid:
                IssueGridFormation(target, unitCount);
                break;
            case FormationType.Circle:
                IssueCircleFormation(target, unitCount);
                break;
            case FormationType.Triangle:
                IssueTriangleFormation(target, unitCount);
                break;
            case FormationType.Random:
                IssueRandomFormation(target, unitCount);
                break;
        }
    }

    void IssueGridFormation(Vector3 target, int count)
    {
        float spacing = 1.0f;
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

            selectedUnits[i].MoveTo(target + offset);
        }
    }

    void IssueCircleFormation(Vector3 target, int count)
    {
        float radius = 1.5f;
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2 / count;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
            selectedUnits[i].MoveTo(target + offset);
        }
    }

    void IssueTriangleFormation(Vector3 target, int count)
    {
        float spacing = 1.0f;
        int index = 0;

        for (int row = 0; index < count; row++)
        {
            int unitsInRow = row + 1;
            for (int i = 0; i < unitsInRow && index < count; i++)
            {
                float x = (i - unitsInRow / 2f) * spacing;
                float y = -row * spacing;
                Vector3 offset = new Vector3(x, y, 0f);
                selectedUnits[index].MoveTo(target + offset);
                index++;
            }
        }
    }

    void IssueRandomFormation(Vector3 target, int count)
    {
        float radius = 2f;
        for (int i = 0; i < count; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * radius;
            Vector3 offset = new Vector3(randomOffset.x, randomOffset.y, 0f);
            selectedUnits[i].MoveTo(target + offset);
        }
    }

    private void HandleFormationSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            int val = (int)currentFormation;
            val = (val - 1 + System.Enum.GetValues(typeof(FormationType)).Length) % System.Enum.GetValues(typeof(FormationType)).Length;
            currentFormation = (FormationType)val;
            Debug.Log("Switched to formation: " + currentFormation);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            int val = (int)currentFormation;
            val = (val + 1) % System.Enum.GetValues(typeof(FormationType)).Length;
            currentFormation = (FormationType)val;
            Debug.Log("Switched to formation: " + currentFormation);
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
