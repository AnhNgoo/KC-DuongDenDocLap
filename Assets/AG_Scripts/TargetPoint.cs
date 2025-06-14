using System.Collections;
using UnityEngine;

public class TargetPoint : MonoBehaviour
{
    [SerializeField] GameObject targetPoint; // Đối tượng sẽ hiển thị vị trí chuột
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0; // Đặt z về 0 để phù hợp với không gian 2D
            targetPoint.transform.position = mousePos; // Di chuyển đối tượng đến vị trí chuột
            StartCoroutine(TargetPointCoroutine()); // Bắt đầu coroutine để ẩn đối tượng sau một thời gian
        }


    }

    IEnumerator TargetPointCoroutine()
    {
        targetPoint.SetActive(true); // Hiển thị đối tượng nếu nó đang ẩn
        yield return new WaitForSeconds(0.3f); // Đợi một chút trước khi ẩn đối tượng
        targetPoint.SetActive(false); // Ẩn đối tượng sau khi đã hiển thị
    }
}
