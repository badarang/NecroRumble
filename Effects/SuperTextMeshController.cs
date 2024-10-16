using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SuperTextMeshController : MonoBehaviour
{
    private Vector3 initialScale;

    private void Start()
    {
        initialScale = Vector3.one * .4f;
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(initialScale, 0.5f).SetEase(Ease.OutCirc);
    }
}