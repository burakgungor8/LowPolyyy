using UnityEngine;
using System.Collections;

/// <summary>
/// Kamera shake ve zoom gibi efektleri kontrol eder.
/// </summary>
public class CameraEffects : MonoBehaviour
{
    private Vector3 originalPosition;
    private float originalFOV;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        originalPosition = cam.transform.position;
        originalFOV = cam.fieldOfView;
    }

    public void ShakeCamera(float duration, float magnitude)
    {
        StartCoroutine(DoShake(duration, magnitude));
    }

    IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            cam.transform.position += new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;

            cam.transform.position = originalPosition;
        }
    }

    public void Zoom(float targetFOV, float duration)
    {
        StartCoroutine(DoZoom(targetFOV, duration));
    }

    IEnumerator DoZoom(float targetFOV, float duration)
    {
        float startFOV = cam.fieldOfView;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            cam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.fieldOfView = targetFOV;
    }

    public void ResetCamera()
    {
        cam.fieldOfView = originalFOV;
        cam.transform.position = originalPosition;
    }
}
