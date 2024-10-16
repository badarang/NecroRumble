using UnityEngine;
using DG.Tweening;

public class MagicCircleController : MonoBehaviour
{
    private float animationDuration = 2.0f;
    private float destroyDelay = 1.0f;
    private float startTime;
    //private bool destroyScheduled = false; // just to prevent warning log by cyrano

    private Vector3 initialScale = Vector3.zero;
    private Vector3 targetScale = Vector3.one * 2.0f;
    private float initialRotation = 0f;
    private float rotationSpeed = 90f;
    private float initialAlpha = 0f;
    private float targetAlpha = 1f;

    private void Start()
    {
        startTime = Time.time;
        transform.localScale = initialScale;
        SetAlpha(initialAlpha);

        float scaleIncreaseDuration = 0.5f;

        transform.DOScale(targetScale * 1.2f, scaleIncreaseDuration)
            .OnComplete(() => transform.DOScale(targetScale, animationDuration - scaleIncreaseDuration));

        Invoke("StartDestroyAndCreateUnit", animationDuration + destroyDelay);
    }

    private void Update()
    {
        float elapsedTime = Time.time - startTime;
        float t = Mathf.Clamp01(elapsedTime / animationDuration);
        transform.rotation = Quaternion.Euler(0f, 0f, initialRotation + elapsedTime * rotationSpeed);
        SetAlpha(Mathf.Lerp(initialAlpha, targetAlpha, t));

        if (GameManager.Instance.State != GameManager.GameState.Round)
        {
            Destroy(gameObject);
        }
    }

    private void SetAlpha(float alpha)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }

    private void ScheduleDestroy()
    {
        Destroy(gameObject);
    }

    private void StartDestroyAndCreateUnit()
    {
        transform.DOScale(targetScale * 1.2f, 0.1f)
            .OnComplete(() =>
            {
                transform.DOScale(Vector3.zero, 0.3f)
                    .OnComplete(() => Invoke("ScheduleDestroy", destroyDelay - 0.5f));
            });
    }
}