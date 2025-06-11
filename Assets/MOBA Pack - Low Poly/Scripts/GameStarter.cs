using UnityEngine;

public class GameStarter : MonoBehaviour
{
    void Start()
    {
        if (CharacterDataHolder.Instance != null)
        {
            Instantiate(CharacterDataHolder.Instance.selectedChampionPrefab, Vector3.zero, Quaternion.identity);
        }
    }
}
