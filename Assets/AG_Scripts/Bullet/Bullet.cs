using UnityEngine;

public class Bullet : MonoBehaviour
{
    float bulletSpeed;// Tốc độ của viên đạn
    float damage;

    Rigidbody2D rb;

    public void Init(float bulletSpeed, float damage) // Khởi tạo các properties của viên đạn nếu cần thiết
    {
        this.bulletSpeed = bulletSpeed;
        this.damage = damage;
        // Có thể thêm các khởi tạo khác nếu cần
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        BulletForce();
    }

    void BulletForce()
    {
        rb.linearVelocity = transform.up * bulletSpeed;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.GetComponent<ITakeDamage>() != null)
        {
            col.GetComponent<ITakeDamage>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
