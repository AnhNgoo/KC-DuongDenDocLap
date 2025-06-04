using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float bulletSpeed = 20f; // Tốc độ của viên đạn
    [SerializeField] float damage = 200f;

    Rigidbody2D rb;

    public void Init(float damage) // Khởi tạo các properties của viên đạn nếu cần thiết
    {
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
