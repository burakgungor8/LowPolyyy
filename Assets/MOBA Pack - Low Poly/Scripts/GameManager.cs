using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Victory/Defeat Ayarları")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public AudioSource musicSource;
    public AudioClip victoryClip;
    public AudioClip defeatClip;

    [Header("Şampiyon Prefabları")]
    public GameObject[] availablePrefabs;
    public Transform spawnPoint; // Hierarchy'de boş obje oluştur → buraya referans ver

    private bool gameEnded = false;

    void Start()
    {
        // Victory/Defeat panellerini kapat
        if (victoryPanel) victoryPanel.SetActive(false);
        if (defeatPanel) defeatPanel.SetActive(false);

        // Seçilen prefab'ı al ve instantiate et
        string selectedPrefabName = PlayerPrefs.GetString("SelectedChampionPrefab", "");

        if (string.IsNullOrEmpty(selectedPrefabName))
        {
            Debug.LogWarning("Seçilen şampiyon prefab adı bulunamadı.");
            return;
        }

        foreach (GameObject prefab in availablePrefabs)
        {
            if (prefab.name == selectedPrefabName)
            {
                Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
                Debug.Log("Karakter sahneye yerleştirildi: " + prefab.name);
                break;
            }
        }
    }

    public void GameOver(bool isVictory)
    {
        if (gameEnded) return;
        gameEnded = true;

        if (musicSource) musicSource.Stop();

        CameraEffects cameraEffects = Camera.main.GetComponent<CameraEffects>();
        if (cameraEffects != null)
        {
            cameraEffects.ShakeCamera(1f, 0.5f);
            cameraEffects.Zoom(30f, 1f);
        }

        if (isVictory)
        {
            if (victoryClip)
                AudioSource.PlayClipAtPoint(victoryClip, Camera.main.transform.position);
            if (victoryPanel)
                victoryPanel.SetActive(true);
        }
        else
        {
            if (defeatClip)
                AudioSource.PlayClipAtPoint(defeatClip, Camera.main.transform.position);
            if (defeatPanel)
                defeatPanel.SetActive(true);
        }
    }
}
