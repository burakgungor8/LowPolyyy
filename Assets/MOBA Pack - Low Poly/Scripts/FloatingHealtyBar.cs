using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [Header("UI Referansları")]
    public Image fillImage;
    public Image backgroundImage; // Opsiyonel
    
    [Header("Takip Ayarları")]
    public Transform followTarget;
    public Vector3 offset = new Vector3(0, 2f, 0);
    
    [Header("Can Ayarları")]
    private int maxHealth = 100;
    private int currentHealth;
    private float currentFillAmount = 1f;
    public float smoothSpeed = 5f;
    
    [Header("Görünüm")]
    public bool hideWhenFull = true;
    public bool alwaysFaceCamera = true;
    
    void Start()
    {
        currentHealth = maxHealth;
        currentFillAmount = 1f;
        UpdateBar();
        
        // Başlangıçta can barını gizle (tam canlıysa)
        if (hideWhenFull && currentHealth >= maxHealth)
        {
            gameObject.SetActive(false);
        }
    }
    
    void Update()
    {
        // Kameraya bak
        if (alwaysFaceCamera && Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }
        
        // Hedefi takip et
        if (followTarget != null)
        {
            transform.position = followTarget.position + offset;
        }
        
        // Smooth bar güncellemesi
        UpdateBarSmooth();
    }
    
    void UpdateBarSmooth()
    {
        float targetRatio = (float)currentHealth / maxHealth;
        
        // Smooth geçiş
        currentFillAmount = Mathf.Lerp(currentFillAmount, targetRatio, Time.deltaTime * smoothSpeed);
        
        if (fillImage != null)
        {
            fillImage.fillAmount = currentFillAmount;
            
            // Renk geçişi: Yeşil → Sarı → Kırmızı
            Color healthColor = Color.Lerp(Color.red, Color.green, currentFillAmount);
            fillImage.color = healthColor;
        }
    }
    
    public void SetMaxHealth(int value)
    {
        maxHealth = value;
        currentHealth = value;
        currentFillAmount = 1f;
        UpdateBar();
        
        if (hideWhenFull)
            gameObject.SetActive(false);
    }
    
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        // Can barını göster
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);
        
        UpdateBar();
        
        // Eğer can doluysa tekrar gizle
        if (hideWhenFull && currentHealth >= maxHealth)
        {
            Invoke("HideBar", 2f); // 2 saniye sonra gizle
        }
    }
    
    void UpdateBar()
    {
        float targetRatio = (float)currentHealth / maxHealth;
        currentFillAmount = targetRatio;
        
        if (fillImage != null)
        {
            fillImage.fillAmount = currentFillAmount;
            fillImage.color = Color.Lerp(Color.red, Color.green, currentFillAmount);
        }
    }
    
    void HideBar()
    {
        if (currentHealth >= maxHealth && hideWhenFull)
            gameObject.SetActive(false);
    }
}