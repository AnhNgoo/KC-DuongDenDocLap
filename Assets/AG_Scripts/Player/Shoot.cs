using UnityEngine;

public class Shoot : MonoBehaviour
{
    [SerializeField] Transform shootPosition; // Vị trí bắn
    [SerializeField] GameObject bulletPrefab; // Prefab của viên đạn

    void Update()
    {
        Shootting();
    }

    void Shootting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Tạo viên đạn tại vị trí bắn
            GameObject bullet = Instantiate(bulletPrefab, shootPosition.position, shootPosition.rotation);
            bullet.GetComponent<Bullet>().Init(200f);
            Destroy(bullet, 2f);
        }
    }
}
