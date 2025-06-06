using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionBox : MonoBehaviour
{
    public static SelectionBox Instance;

    public RectTransform boxTransform;
    public Canvas canvas;

    private Vector2 startPos;

    private void Awake()
    {
        Instance = this;
        boxTransform.gameObject.SetActive(false);
    }

    public void StartSelectionBox()
    {
        startPos = Input.mousePosition;
        boxTransform.gameObject.SetActive(true);
    }

    [System.Obsolete]
    public void EndSelectionBox()
    {
        boxTransform.gameObject.SetActive(false);
        Vector2 endPos = Input.mousePosition;

        Vector2 min = Vector2.Min(startPos, endPos);
        Vector2 max = Vector2.Max(startPos, endPos);

        Rect selectionRect = new Rect(min, max - min);

        SelectionManager.Instance.DeselectAll();
        Unit[] allUnits = FindObjectsOfType<Unit>();
        foreach (Unit unit in allUnits)
        {
            Vector2 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
            if (selectionRect.Contains(screenPos))
            {
                SelectionManager.Instance.SelectUnit(unit);
            }
        }
    }

    private void Update()
    {
        if (!boxTransform.gameObject.activeSelf) return;

        Vector2 currentMousePos = Input.mousePosition;

        Vector2 min = Vector2.Min(startPos, currentMousePos);
        Vector2 max = Vector2.Max(startPos, currentMousePos);
        Vector2 size = max - min;

        // Set anchored position và size
        boxTransform.position = min;
        boxTransform.sizeDelta = size;
    }
}
