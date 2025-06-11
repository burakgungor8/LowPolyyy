using UnityEngine;

/// <summary>
/// Oyuncunun saldırılarını ve bonus hasar çarpanını kontrol eder.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    public int baseDamage = 10;
    private PlayerMentalSystem mentalSystem;

    void Start()
    {
        // Oyuncunun mental sistemini al
        mentalSystem = GetComponent<PlayerMentalSystem>();
        if (mentalSystem == null)
        {
            Debug.LogError("Mental sistemi bulunamadı! Lütfen PlayerMentalSystem scriptini objeye ekle.");
        }
    }

    public void Attack(GameObject target)
    {
        float multiplier = (mentalSystem != null) ? mentalSystem.GetDamageMultiplier() : 1.0f;
        int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);

        // Hedefe hasar gönder
        Debug.Log("Saldırı yapıldı: " + finalDamage + " hasar verildi!");

        // Eğer hedefin health scripti varsa, hasar uygula
        var health = target.GetComponent<MinionHealth>();
        if (health != null)
        {
            health.TakeDamage(finalDamage);
        }
    }
}
