using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    public GameObject[] allChampionPrefabs;

    void Start()
    {
        string selectedName = PlayerPrefs.GetString("SelectedChampion");

        foreach (GameObject champ in allChampionPrefabs)
        {
            if (champ.name == selectedName)
            {
                Instantiate(champ, Vector3.zero, Quaternion.identity);
                break;
            }
        }
    }
}
