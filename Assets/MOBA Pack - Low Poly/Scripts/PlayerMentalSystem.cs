using UnityEngine;

/// <summary>
/// Oyuncunun mental sağlığını takip eder.
/// Mental seviyesi %70'in üzerindeyse fazladan hasar verir.
/// </summary>
public class PlayerMentalSystem : MonoBehaviour
{
    [Header("Mental Bar Ayarları")]
    public int maxMental = 100;
    public int currentMental = 50;

    [Header("Hasar Çarpanı")]
    public float bonusDamageMultiplier = 1.0f;

    void Update()
    {
        // Mental seviyesi %70 üzerindeyse bonus hasar aktif
        bonusDamageMultiplier = (currentMental >= 70) ? 1.25f : 1.0f;
    }

    /// <summary>
    /// Mental seviyesini artırır.
    /// </summary>
    public void IncreaseMental(int amount)
    {
        currentMental = Mathf.Clamp(currentMental + amount, 0, maxMental);
        Debug.Log("Mental güncellendi: " + currentMental);
    }

    /// <summary>
    /// Güncel hasar çarpanını döner.
    /// </summary>
    public float GetDamageMultiplier()
    {
        return bonusDamageMultiplier;
    }
}
