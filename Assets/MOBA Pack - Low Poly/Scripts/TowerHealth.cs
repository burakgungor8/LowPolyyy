using UnityEngine;

public class TowerHealth : MonoBehaviour
{
    public int maxHealth = 300;
    private int currentHealth;

    [Header("HP Bar")]
    public GameObject healthBarPrefab;
    public Transform healthBarPosition;
    private FloatingHealthBar healthBar;

    [Header("Explosion Effect")]
    public GameObject explosionEffect; // 💥 Patlama efekti prefab'ı

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBarPrefab != null && healthBarPosition != null)
        {
            GameObject hb = Instantiate(healthBarPrefab, healthBarPosition.position, Quaternion.identity, transform);
            healthBar = hb.GetComponent<FloatingHealthBar>();
            healthBar.followTarget = healthBarPosition;
            healthBar.SetMaxHealth(maxHealth);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBar != null)
        {
            healthBar.TakeDamage(amount);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // 💥 Efekti doğur
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position + Vector3.up * 1.5f, Quaternion.identity);
        }

        // 🧹 Kuleyi yok et
        Destroy(gameObject);
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }
}
