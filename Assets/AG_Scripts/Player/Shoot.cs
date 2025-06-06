using UnityEngine;

public class Shoot : MonoBehaviour
{
    [SerializeField] Transform shootPosition; // Vị trí bắn
    [SerializeField] GameObject bulletPrefab; // Prefab của viên đạn
    [SerializeField] float bulletSpeed = 200f; // Tốc độ của viên đạn
    [SerializeField] float damage = 10f; // Sát thương của viên đạn


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
            bullet.GetComponent<Bullet>().Init(bulletSpeed, damage);
            Destroy(bullet, 2f);
        }
    }
}
