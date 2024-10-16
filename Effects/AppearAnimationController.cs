using UnityEngine;
using DG.Tweening;

public class AppearAnimationController : MonoBehaviour
{
    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
        Init();
    }
    
    void Init()
    {
        transform.localScale = Vector3.zero;
        
        var seq = DOTween.Sequence();
        seq.Append(transform.DOScale(initialScale * 1.2f, 1.0f).SetEase(Ease.OutBack))
            .Append(transform.DOLocalJump(transform.localPosition, 0.5f, 1, 0.5f));

        seq.Play();
    }
}