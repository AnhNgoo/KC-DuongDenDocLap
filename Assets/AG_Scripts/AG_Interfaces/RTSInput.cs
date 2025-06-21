using UnityEngine;

public class RTSInput : MonoBehaviour
{
    // Biến để gán Camera từ Inspector. Nếu không gán, sẽ tự động tìm Camera.main.
    public Camera mainCamera; // Đổi tên từ 'cam' thành 'mainCamera' để rõ ràng hơn

    private void Awake()
    {
        // Nếu chưa được gán trong Inspector, cố gắng tìm Camera chính trong scene.
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Kiểm tra sau khi tìm kiếm để đảm bảo có camera.
        if (mainCamera == null)
        {
            Debug.LogError("RTSInput: Không tìm thấy Main Camera trong scene! Vui lòng đảm bảo có một camera được gắn thẻ 'MainCamera' hoặc gán thủ công vào Inspector.", this);
            // Vô hiệu hóa script nếu không có camera để tránh lỗi NullReferenceException
            enabled = false;
        }
    }

    private void Update()
    {
        // Đảm bảo có camera và MovementManager đã sẵn sàng trước khi xử lý input
        if (mainCamera == null || MovementManager.Instance == null)
        {
            return;
        }

        // --- Xử lý lệnh di chuyển bằng chuột phải ---
        if (Input.GetMouseButtonDown(1)) // 1 là nút chuột phải
        {
            // Lấy vị trí chuột trên màn hình
            Vector3 mouseScreenPos = Input.mousePosition;

            // Chuyển đổi vị trí chuột từ không gian màn hình sang không gian thế giới.
            // Giả định game bạn là 2D hoặc các đơn vị ở mặt phẳng Z = 0.
            Vector3 targetWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, mainCamera.nearClipPlane));
            targetWorldPos.z = 0f; // Đảm bảo vị trí Z là 0

            MovementManager.Instance.IssueMoveCommand(targetWorldPos);
        }

        // --- Xử lý lệnh bắn bằng chuột trái ---
        if (Input.GetMouseButtonDown(0)) // 0 là nút chuột trái
        {
            // Tương tự, lấy vị trí chuột và chuyển đổi sang không gian thế giới
            Vector3 mouseScreenPos = Input.mousePosition;
            Vector3 targetWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, mainCamera.nearClipPlane));
            targetWorldPos.z = 0f; // Đảm bảo vị trí Z là 0

            // Gọi IssueShootCommand thay vì IssueAttackCommand
            MovementManager.Instance.IssueShootCommand(targetWorldPos);
        }
    }
}