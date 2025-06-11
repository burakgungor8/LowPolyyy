using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    public float moveSpeed = 20f;
    public float scrollSpeed = 1000f;
    public float minZoom = 10f;
    public float maxZoom = 80f;

    public Transform followTarget; // Hedef karakter
    public Vector3 followOffset = new Vector3(0, 20, -10);
    private bool isFollowing = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            isFollowing = !isFollowing; // Takip modunu aç/kapat
        }

        if (isFollowing && followTarget != null)
        {
            transform.position = followTarget.position + followOffset;
            return;
        }

        // Serbest kamera hareketi
        Vector3 pos = transform.position;

        if (Input.mousePosition.x >= Screen.width - 10)
            pos.x += moveSpeed * Time.deltaTime;
        if (Input.mousePosition.x <= 10)
            pos.x -= moveSpeed * Time.deltaTime;
        if (Input.mousePosition.y >= Screen.height - 10)
            pos.z += moveSpeed * Time.deltaTime;
        if (Input.mousePosition.y <= 10)
            pos.z -= moveSpeed * Time.deltaTime;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * scrollSpeed * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, minZoom, maxZoom);

        transform.position = pos;
    }
}
