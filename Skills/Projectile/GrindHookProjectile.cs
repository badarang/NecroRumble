using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityTransform;
using DG.Tweening;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class GrindHookProjectile : Projectile
{
    private bool isCatched = false;
    private bool isBoom = false;
    private bool isDoTweenPlayed = false;
    Unit targetUnit = null;
    
    private Tween targetUnitShakeTween;
    private Tween selfShakeTween;
    
    private void OnEnable()
    {
        Init();
    }
    private void OnDisable()
    {
    }
    private void Init()
    {
        isBoom = false;
        isCatched = false;
        var scale = transform.localScale;
        transform.localScale = Vector3.zero;
        transform.DOScale(scale, 1f).SetEase(Ease.OutBack);
    }
    protected override void Update()
    {
        if (Vector3.Distance(_startPos, transform.position) > _destroyRange)
        {
            DestroyGrindHook();
            return;
        }

        if (isCatched && !isBoom)
        {
            if (targetUnit == null)
            {
                StopDOTweenAnimations();
                DestroyGrindHook();
                return;
            }
            if (targetUnit.IsDead)
            {
                StopDOTweenAnimations();
                DestroyGrindHook();
                return;
            }

            if (!isDoTweenPlayed)
            {
                if (targetUnit == null) return;
                if (gameObject == null) return;
                
                targetUnit.IsGrindHooked = true;
                isDoTweenPlayed = true;
                targetUnitShakeTween = targetUnit.transform.DOShakePosition(0.5f, strength: new Vector3(0.1f, 0.1f, 0f), vibrato: 20, randomness: 90, fadeOut: false);
				// ManagerRoot.Input.Vibration(0.3f, 0.5f, true, true);
				selfShakeTween = transform.DOShakePosition(0.5f, strength: new Vector3(0.1f, 0.1f, 0f), vibrato: 20, randomness: 90, fadeOut: false)
                    .OnComplete(() =>
                    {
                        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                        isBoom = true;
                        targetUnitShakeTween.Kill();
                        selfShakeTween.Kill();
                    });
            }
        }

        if (isBoom)
        {
            DestroyGrindHook();
            if (targetUnit == null)
            {
                Debug.LogError("!!!!!!!!!!!!!!!그라인드 프로젝타일 target Unit Null 임!!!!!!!!!!!!!!!!");
                return;
            }
            isBoom = false;
            
            // if (targetUnit.IsElite)
            // {
            //     var text = "이 유닛은 제거할 수 없습니다!";
            //     ManagerRoot.Sound.PlaySfx("Error Sound 4", 0.6f);
            //     GameUIManager.Instance.CreateNumberBalloon(text, targetUnit.transform.position, Color.red, .5f, 2.5f);
            //     targetUnit.IsGrindHooked = false;
            //     return;
            // }
            
            GameManager.Instance.GetPlayer().GetComponent<GrindUnit>().ExcuteGrind(targetUnit);
            //Explosion Particle
            GameObject explosionParticleYellowPrefab = Resources.Load<GameObject>("Prefabs/Effects/VFXElectricExplosionYellow");
            GameObject explosionParticleYellow = Instantiate(explosionParticleYellowPrefab, transform.position, Quaternion.identity);
            
            GameObject explosionParticlePurplePrefab = Resources.Load<GameObject>("Prefabs/Effects/VFXElectricExplosionPurple");
            GameObject explosionParticlePurple = Instantiate(explosionParticlePurplePrefab, transform.position, Quaternion.identity);
        }
    }

    
    
    void DestroyGrindHook(){
        GetComponent<TrailRenderer>().Clear();
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(transform.localScale.x);
        scale.y = Mathf.Abs(transform.localScale.y);
        transform.localScale = scale;

        Destroy(gameObject);
        if (targetUnit == null) return;
        
        //ManagerRoot.Resource.Release(gameObject);
    }

    protected override void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.layer == Layers.UndeadUnit && !isCatched)
        {
            if (other.TryGetComponent(out Unit _unit))
            {
                if (_unit.IsDead == false)
                {
                    //GameManager.Instance.GetPlayer().GetComponent<GrindUnit>().ExcuteGrind(other.GetComponent<Unit>());
                    isCatched = true;
                    targetUnit = other.GetComponent<Unit>();
                    //Vector3 pivotOffset = targetUnit.transform.position - targetUnit.transform.position;
                    // Set the position with the pivot offset
                    //targetUnit.transform.position = transform.position - pivotOffset;
                    if (targetUnit.TryGetComponent(out FeedbackController feedbackController))
                    {
                        feedbackController.ChangeMaterialBasedOnIsSelected(false);
                    }
                }
            }
        }
    }
    
    private void StopDOTweenAnimations()
    {
        targetUnitShakeTween?.Kill();
        selfShakeTween?.Kill();
    }

}
