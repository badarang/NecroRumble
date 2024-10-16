using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class ExplosionController : MonoBehaviour
{
    public void Explode(float scale)
    {
        float _scale = scale;
        transform.localScale = new Vector3(0f, 0f, 1f);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(new Vector3(_scale, _scale, 1f), 1f).SetEase(Ease.OutElastic));
        sequence.Play();
    }
}