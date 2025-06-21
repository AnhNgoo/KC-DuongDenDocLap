using UnityEngine;

public class ClickToMove : MonoBehaviour
{
    public Transform target;
    public Camera cam;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;
            target.position = worldPos;
        }
    }
}
