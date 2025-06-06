using UnityEngine;

public class Unit : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector3 targetPosition;
    private bool isMoving = false;

    public GameObject selectionCircle;

    public bool IsSelected { get; private set; }

    private void Start()
    {
        targetPosition = transform.position;
        SetSelected(false);
    }

    private void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
            }
        }
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        if (selectionCircle != null)
            selectionCircle.SetActive(selected);
    }

    public void MoveTo(Vector3 position)
    {
        targetPosition = position;
        isMoving = true;
    }

    public void CancelMove()
    {
        isMoving = false;
    }
}
