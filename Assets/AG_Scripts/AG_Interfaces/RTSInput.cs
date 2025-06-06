using UnityEngine;

public class RTSInput : MonoBehaviour
{
    public LayerMask unitMask;
    public Camera cam;

    [System.Obsolete]
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100f, unitMask);
            if (hit.collider != null)
            {
                // Select one unit
                SelectionManager.Instance.DeselectAll();
                Unit unit = hit.collider.GetComponent<Unit>();
                if (unit != null)
                {
                    SelectionManager.Instance.SelectUnit(unit);
                }
            }
            else
            {
                // Start drag-select
                SelectionBox.Instance.StartSelectionBox();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            SelectionBox.Instance.EndSelectionBox();
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0f;
            SelectionManager.Instance.IssueMoveCommand(pos);
        }

        if (Input.GetMouseButtonDown(2))
        {
            SelectionManager.Instance.CancelMoveCommand();
        }
    }
}
