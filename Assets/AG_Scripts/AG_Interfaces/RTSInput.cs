using UnityEngine;

public class RTSInput : MonoBehaviour
{
    public LayerMask unitMask;
    public Camera cam;

    private Vector3 mouseDownPos;
    private bool isDragging = false;

    [System.Obsolete]
    private void Update()
    {
        // Bắt đầu nhấn chuột trái
        if (Input.GetMouseButtonDown(0))
        {
            mouseDownPos = Input.mousePosition;
            isDragging = false;
        }

        // Kiểm tra kéo chuột
        if (Input.GetMouseButton(0))
        {

            if (!isDragging && Vector3.Distance(mouseDownPos, Input.mousePosition) > 10f)
            {
                isDragging = true;
                SelectionBox.Instance.StartSelectionBox();
            }
        }

        // Nhả chuột trái
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                // Kết thúc kéo chọn
                SelectionBox.Instance.EndSelectionBox();
            }
            else
            {
                // Raycast kiểm tra lính
                RaycastHit2D hit = Physics2D.Raycast(cam.ScreenToWorldPoint(mouseDownPos), Vector2.zero, 100f, unitMask);
                if (hit.collider != null)
                {
                    Unit unit = hit.collider.GetComponent<Unit>();
                    if (unit != null)
                    {
                        // Không bỏ chọn cũ, chỉ thêm lính mới nếu chưa có
                        SelectionManager.Instance.SelectUnit(unit);
                    }
                }
                else
                {
                    // Click vào vùng trống => bỏ chọn tất cả
                    SelectionManager.Instance.DeselectAll();
                }
            }
        }

        // Lệnh di chuyển bằng chuột phải
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0f;
            SelectionManager.Instance.IssueMoveCommand(pos);
        }

        // Hủy lệnh di chuyển bằng chuột giữa
        if (Input.GetMouseButtonDown(2))
        {
            SelectionManager.Instance.CancelMoveCommand();
        }
    }
}
