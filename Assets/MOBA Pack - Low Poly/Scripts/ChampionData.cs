using UnityEngine;

[System.Serializable]
public class ChampionData
{
    public string name;          // Karakter ismi
    public Sprite icon;          // UI'da gösterilecek resim
    public GameObject prefab;    // Seçilen karakterin prefabı (sahneye instantiate edeceğiz)
}
