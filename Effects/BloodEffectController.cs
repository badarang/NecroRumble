using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BloodEffectController : ParabolaEffectController
{
    public void Start()
    {
        SetRandomInit();
        FireProjectile(transform.position, GetRandomPos());
    }

    private void SetRandomInit()
    {
        transform.localScale = Random.Range(0.05f, .4f) * Vector3.one;
        speed = Random.Range(1.5f, 3f);
        projectileHeight = Random.Range(1f, 2f);
    }
    
    public void SetColorBasedOnFaction(Faction faction)
    {
        Color factionColor = Color.white;
        switch (faction)
        {
            case Faction.Undead:
                factionColor = GameManager.UndeadBloodColor;
                break;
            case Faction.Human:
                factionColor = GameManager.HumanBloodColor;
                break;
        }
        
        float randomOffset = Random.Range(-30f, 30f) / 255f;
        factionColor.r = Mathf.Clamp01(factionColor.r + randomOffset);
        factionColor.g = Mathf.Clamp01(factionColor.g + randomOffset);
        factionColor.b = Mathf.Clamp01(factionColor.b + randomOffset);

        GetComponent<SpriteRenderer>().color = factionColor;
    }
    
    protected override void Update()
    {
        base.Update();
        if (fireLerp >= 1 && !isGrounded)
        {
            isGrounded = true;
            HandleGroundCollision();
        }
    }

    private Vector3 GetRandomPos()
    {
        float xx = transform.position.x + Random.Range(-1f, 1f);
        float yy = transform.position.y + Random.Range(-.6f, .6f);
        return new Vector3(xx, yy, transform.position.z);
    }
    
    private void HandleGroundCollision()
    {
        Vector3 targetScale = new Vector3(transform.localScale.x * 1.5f, transform.localScale.y * 1.2f, transform.localScale.z);
        
        float duration = 1.5f;
        float startValue = 0.1f;
        //float endValue = 1f;
        Ease easeType = Ease.OutQuint;

        transform.localScale = Vector3.one * startValue;
        
        // todo: reload 시 null 체크 안되서 오류나는 버그 있음
        transform.DOScale(targetScale, duration)
            .SetEase(easeType)
            .OnComplete(() =>
            {
                if( transform.TryGetComponent( out SpriteRenderer renderer) )
                {
                    renderer.material.DOFade(0f, 1f)
                    .OnComplete(() =>
                    {
                        Destroy(gameObject);
                    });
                }
            });
    }

    private void OnDestroy() {
        transform.DOKill();
    }
}
