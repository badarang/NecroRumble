using UnityEngine;
using DG.Tweening;

public class FlagController : MonoBehaviour
{
    private float raiseHeight = 1f;
    private float raiseDuration = 0.2f;
    private float fallDuration = 0.05f;
    //private bool isRaised = false; // just to prevent warning log by cyrano

    void Start()
    {
        Init();
    }
    
    void Init()
    {
        transform.localScale = Vector3.zero;
        RaiseFlag();
    }

    void RaiseFlag()
    {
        //isRaised = true;

        float currentY = transform.position.y;

        transform.DOScale(new Vector3(1.5f, 1f, 1f), raiseDuration);
        transform.DOMoveY(currentY + raiseHeight, raiseDuration).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            DOVirtual.DelayedCall(0.1f, () =>
            {
                transform.DOMoveY(currentY, fallDuration).SetEase(Ease.InQuad).OnComplete(() =>
                {
                    //isRaised = false;
                });
            });
        });
    }
}