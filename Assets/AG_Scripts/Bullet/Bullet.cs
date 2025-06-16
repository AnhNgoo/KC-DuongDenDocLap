using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifetime = 2f;
    public LayerMask collisionLayers; // Layers the bullet can collide with (Enemy, Wall)

    private Vector3 moveDirection;
    private float moveSpeed;
    private float damage;

    private float currentLifetime;

    private void Awake()
    {
        currentLifetime = lifetime;
    }

    /// <summary>
    /// Initializes the bullet with direction, speed, and damage.
    /// </summary>
    /// <param name="direction">The normalized direction of the bullet's movement.</param>
    /// <param name="speed">The speed of the bullet.</param>
    /// <param name="bulletDamage">The damage the bullet inflicts.</param>
    public void Initialize(Vector3 direction, float speed, float bulletDamage)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
        damage = bulletDamage;

        if (direction != Vector3.zero)
        {
            transform.up = direction;
        }
    }

    private void Update()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        currentLifetime -= Time.deltaTime;
        if (currentLifetime <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) // Or OnTriggerEnter if you're using 3D Colliders
    {
        HandleCollision(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision) // Or OnCollisionEnter if you're using 3D Colliders (and Rigidbody is not Is Kinematic)
    {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject collidedObject)
    {
        // First, check if the collided object is on one of the designated collision layers.
        if (((1 << collidedObject.layer) & collisionLayers) != 0)
        {
            // Now, check if the collided object has the "Enemy" tag.
            if (collidedObject.CompareTag("Enemy"))
            {
                // Attempt to get the ITakeDamage interface from the collided object.
                ITakeDamage damageable = collidedObject.GetComponent<ITakeDamage>();
                if (damageable != null)
                {
                    // If it's an Enemy and has the ITakeDamage interface, apply damage.
                    damageable.TakeDamage(damage);
                    Debug.Log($"{collidedObject.name} received {damage} damage from the bullet.");
                }
                else
                {
                    Debug.LogWarning($"Bullet hit an object tagged 'Enemy' ({collidedObject.name}) but it does not have an ITakeDamage component!");
                }
            }
            // The bullet should always be destroyed upon hitting anything on a valid collision layer,
            // regardless of its tag, unless you specify other conditions (e.g., pass through walls).
            Destroy(gameObject);
        }
    }
}