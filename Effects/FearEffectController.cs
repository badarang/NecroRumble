using System;
using UnityEngine;
using DG.Tweening;

public class FearEffectController : MonoBehaviour
{
    private Vector3 initialScale = Vector3.one;
    private Quaternion initialRotation = Quaternion.identity;
    private Transform initialTransform = null;
    private float duration;
    public float Duration
    {
        get => duration;
        set => duration = value;
    }

    void Awake()
    {
        initialTransform = transform;
    }
    
    void OnEnable()
    {
        transform.localScale = Vector3.zero;
        transform.localRotation = initialRotation;
        transform.localPosition = initialTransform.localPosition;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(initialScale, duration)).SetEase(Ease.OutQuad)
                .Append(transform.DOScale(Vector3.zero, 1f).SetEase(Ease.OutQuad))
                .Insert(duration, transform.DOLocalMove(new Vector3(-0.2f, 1f, 0), 0.5f).SetEase(Ease.OutQuad))
                .Insert(duration, transform.DORotate(new Vector3(0, 0, 20f), 0.5f).SetEase(Ease.OutQuad))
                .Insert(duration + .5f, transform.DOLocalMove(new Vector3(0.2f, 1.5f, 0), 0.5f).SetEase(Ease.OutQuad))
                .Insert(duration + .5f, transform.DORotate(new Vector3(0, 0, -20f), 0.5f).SetEase(Ease.OutQuad));

        sequence.Play();

    }
}