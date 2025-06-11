using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    public Image fillImage;
    public Transform followTarget;
    public Vector3 offset = new Vector3(0, 2f, 0);

    private int maxHealth = 100;
    private int currentHealth;

    // 👇 Yeni animasyon değişkenleri
    private float currentFillAmount = 1f;
    public float smoothSpeed = 5f;

    void Start()
    {
        currentHealth = maxHealth;
        currentFillAmount = 1f;
        UpdateBar();
    }

    void Update()
    {
        if (Camera.main != null)
            transform.LookAt(transform.position + Camera.main.transform.forward);

        if (followTarget != null)
            transform.position = followTarget.position + offset;
    }

    public void SetMaxHealth(int value)
    {
        maxHealth = value;
        currentHealth = value;
        currentFillAmount = 1f;
        UpdateBar();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateBar();
    }

    void UpdateBar()
    {
        float targetRatio = (float)currentHealth / maxHealth;

        // 🔄 Smooth geçişli doluluk
        currentFillAmount = Mathf.Lerp(currentFillAmount, targetRatio, Time.deltaTime * smoothSpeed);
        fillImage.fillAmount = currentFillAmount;

        // 🎨 Smooth renk geçişi: yeşil → kırmızı
        fillImage.color = Color.Lerp(Color.red, Color.green, currentFillAmount);
    }
}
