using UnityEngine;

public class EnemyController : MonoBehaviour, ITakeDamage
{
    [SerializeField] float health = 1000;
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
