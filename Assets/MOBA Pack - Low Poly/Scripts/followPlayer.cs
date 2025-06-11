using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 10f, -10f);
    private bool followMode = false;

    void Update()
    {
        // T tuşuna basıldığında takip modu aktif/pasif yapılır
        if (Input.GetKeyDown(KeyCode.T))
        {
            followMode = !followMode;
        }

        // Takip modu açıksa kamerayı hedefe sabitle
        if (followMode && target != null)
        {
            transform.position = target.position + offset;
        }
    }
}
