using UnityEngine;

public class MinionHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    [Header("HP Bar")]
    public GameObject healthBarPrefab;
    public Transform healthBarPosition;
    private FloatingHealthBar healthBar;

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
            Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }

    // 🔥 İŞTE BURASI: Kule sistemi için gerekli
    public bool IsDead()
    {
        return currentHealth <= 0;
    }
}
