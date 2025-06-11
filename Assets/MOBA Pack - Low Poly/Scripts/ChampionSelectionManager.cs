using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ChampionSelectionManager : MonoBehaviour
{
    [Header("Şampiyon Listesi ve UI")]
    public List<ChampionData> champions;              // Inspector'dan tanımla
    public Transform championGrid;                    // UI'daki butonların olacağı alan
    public GameObject championButtonPrefab;           // Her karakteri temsil eden UI prefab

    private ChampionData selectedChampion;

    void Start()
    {
        if (champions == null || champions.Count == 0)
        {
            Debug.LogError("Şampiyon listesi boş! Lütfen Inspector'dan doldur.");
            return;
        }

        if (championGrid == null || championButtonPrefab == null)
        {
            Debug.LogError("UI elemanları eksik! ChampionGrid veya ButtonPrefab atanmadı.");
            return;
        }

        PopulateChampionList();
    }

    void PopulateChampionList()
    {
        foreach (ChampionData champ in champions)
        {
            GameObject buttonObj = Instantiate(championButtonPrefab, championGrid);

            // UI içeriği güncelle
            Text textComponent = buttonObj.GetComponentInChildren<Text>();
            Image imageComponent = buttonObj.GetComponentInChildren<Image>();

            if (textComponent != null)
                textComponent.text = champ.name;

            if (imageComponent != null && champ.icon != null)
                imageComponent.sprite = champ.icon;

            // Buton davranışı
            Button btn = buttonObj.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => OnChampionSelected(champ));
        }
    }

    public void OnChampionSelected(ChampionData champ)
    {
        selectedChampion = champ;
        Debug.Log("Seçilen Şampiyon: " + champ.name);
    }

    public void ConfirmSelection()
    {
        if (selectedChampion != null && selectedChampion.prefab != null)
        {
            PlayerPrefs.SetString("SelectedChampionPrefab", selectedChampion.prefab.name);
            Debug.Log("Prefab kayıt edildi: " + selectedChampion.prefab.name);
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.LogWarning("Şampiyon veya prefab seçilmedi! Lütfen bir seçim yap.");
        }
    }
}
