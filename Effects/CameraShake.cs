using UnityEngine;
using System.Collections;
using DG.Tweening;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    private CameraController _camera;

    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        _camera = GetComponent<CameraController>();
    }

    public void OnShake(float duration, float magnitude)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector3 shakeVector = new Vector3(Random.insideUnitSphere.x, Random.insideUnitSphere.y, 0f) * magnitude;
            _camera.ShakeOffset = shakeVector;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset the shake offset after the shake is complete
        _camera.ShakeOffset = Vector3.zero;
    }
    
    public static void Shake(float duration, float magnitude) => Instance.OnShake(duration, magnitude);
}