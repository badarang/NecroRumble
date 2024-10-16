using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class TwoSwordAssassinUnit : Unit
{
    public float SpecialAttackCoolTime = 5f; // 회전 회오리 쿨타임
    public float SpecialAttackRadius = 3f; // 회전 회오리 공격 범위
    public float SpecialAttackDamage = 1f; // 회전 회오리 공격 데미지
    public float SpecialAttackDurationSec = 0.2f; // 회전 회오리 공격 Duration
    public float TelepoteDurationSec = 0.2f; // 순간이동 하는데 걸리는 시간
    
    public float SpecialMoveCoolTime = 1f; // 순간이동 쿨타임
    public float SpecialMoveRange = 1f; // 순간이동 거리
    bool _isSpecialMove = false;
    //bool _isSpecialMoving = false;
    
    private float _attackSoundDelayTimer = 0f;

    float _timer = 0f;
    
    private GameObject afterImageEffectPrefabUndead;
    private GameObject afterImageEffectPrefabHuman;

    protected override void Init()
    {
        base.Init();
        reviveAudio = "Dragon Roar 07 Larger";
        if(CurrentFaction == Faction.Undead){
            StartCoroutine(SpecialAtkCooltimeRoutine());
            UIWinPopup.IsFamilyImage[(int)UIWinPopup.Images.TwoSwordImage] = true;

			SteamUserData.IsTwoSwordRevived = true;
		}
        _isSpecialMove = true;
        IsSpecialAttacking = false;
        
        afterImageEffectPrefabUndead = ManagerRoot.Resource.Load<GameObject>("Prefabs/Effects/TwoSwordAssassinAfterImageEffect");
        afterImageEffectPrefabHuman = ManagerRoot.Resource.Load<GameObject>("Prefabs/Effects/TwoSwordAssassinAfterImageEffectHuman");
    }
    public override void Update()
    {
        base.Update();
        if (_attackSoundDelayTimer > 0f)
        {
            _attackSoundDelayTimer -= Time.deltaTime;
        }
        if(!IsDead && IsSpecialAttacking){
            // 공격 중
            _timer += Time.deltaTime;
            if(_timer >= SpecialAttackDurationSec){
                // 공격
                _timer = 0f;
                //Collider2D[] detectEnemies_ = new Collider2D[100];
                //Physics2D.OverlapCircleNonAlloc(transform.position, _detectRadius, detectEnemies_, targetLayerMask);
                Collider2D[] detectEnemies_ = GetHumanListEcllipse(transform.position, SpecialAttackRadius, SpecialAttackRadius * .5f);

                foreach (var thing in detectEnemies_)
                {
                    if(thing == null) break;
                    if(thing == _collider) continue;

                    if(_collider.Distance(thing).distance <= SpecialAttackRadius){
                        if(thing.gameObject.TryGetComponent(out Unit unit) && unit != null)
                        {
                            AttackInfo atkInfo = new AttackInfo(this, SpecialAttackDamage, attackingMedium: GetAttackTransformByUnitType());
                            
                            if (_attackSoundDelayTimer <= 0f)
                            {
                                _attackSoundDelayTimer = 0.1f;
                                var punchSounds = new string[]{"Dagger woosh 4", "Dagger woosh 6"};
                                ManagerRoot.Sound.PlaySfx(punchSounds[Random.Range(0, punchSounds.Length)], 1f);
                            }
                            
                            unit.TakeDamage(atkInfo);
                            unit.TakeKnockBack(transform, 1f);
                            
                            GameObject hitSwordPrefab = Resources.Load<GameObject>("Prefabs/Effects/TwoSwordAssassinHitSwordEffect");
                            GameObject hitSwordEffect = Instantiate(hitSwordPrefab, transform.position, Quaternion.identity);
                            hitSwordEffect.transform.position = transform.position;
                            
                            Vector3 newThingPos = new Vector3(0f, .5f, 0f) + thing.transform.position;
                            hitSwordEffect.transform.DOMove(newThingPos, 0.15f);
                            hitSwordEffect.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(newThingPos.y - transform.position.y, newThingPos.x - transform.position.x) * Mathf.Rad2Deg);
                            Destroy(hitSwordEffect, .2f);
                        }
                        if (thing.gameObject.TryGetComponent(out Player player) && player != null)
                        {
                            player.TakeKnockBack(transform, 1f);
                        }
                    }
                }
        }
        }

    }
    
    Collider2D[] GetHumanListEcllipse(Vector3 centerPoint, float _width, float _height)
    {
        var humanUnits = Physics2D.OverlapAreaAll(
            new Vector2(centerPoint.x - _width, centerPoint.y - _height),
            new Vector2(centerPoint.x + _width, centerPoint.y + _height),
            targetLayerMask
        );
        return humanUnits;
    }
    
    public override void TurnUndeadEvent()
    {
        AttackTarget = null;
    }
    
    public override void AIDetectAround(Collider2D[] detectEnemies_){
        if(IsSpecialAttacking){
            return;
        }
        else{
            base.AIDetectAround(detectEnemies_);
        }
    }
    
    public override EnumBTState AIAttackCheck(){  
        if(IsSpecialAttacking){
            return EnumBTState.Running;
        }   
        return base.AIAttackCheck();    
    }
    public override bool AISpecialAttack(){
        if(IsSpecialAttacking) return true;
        if(IsSpecialAttackable && CurrentFaction == Faction.Undead){
            AIStop();
            IsSpecialAttackable = false;

            StartCoroutine(StartSpecialAttackRoutine());
            return true;
        }
        return false;
    }

    protected IEnumerator SpecialAtkCooltimeRoutine(){
        yield return new WaitForSeconds(SpecialAttackCoolTime);
        IsSpecialAttackable = true;
    }
    
    protected IEnumerator StartSpecialAttackRoutine()
    {
        IsSpecialAttacking = true;
        //애니메이션 실행
        CurrentAnim.Play(EXTRA_1);
        
        //애니메이션 끝날때 까지 기다리기.
        AnimationClipOverrides overrides = new AnimationClipOverrides(4);
        (CurrentAnim.runtimeAnimatorController as AnimatorOverrideController)?.GetOverrides(overrides);
        float animLength = overrides[EXTRA_1].length;
        StartCoroutine(ShowRangeCircleRoutine(animLength, SpecialAttackRadius));
        yield return new WaitForSeconds(animLength);
        
        AIStop();
        StartCoroutine(SpecialAtkCooltimeRoutine());
        IsSpecialAttacking = false;       
        
    }
    
    public override bool AISpecialMove(){

        if(MoveDestination == null) return false;
        if(_isSpecialMove){
            Vector3 moveDirection = MoveDestination.position - transform.position;

            AIStop();
            _isSpecialMove = false;

            // Todo : 순간이동 트레일 켜기, 일정시간 이후에 꺼주기
            // 순간이동!
            // transform.DOMove(transform.position + SpecialMoveRange * moveDirection, TelepoteDurationSec)
            //     .SetEase(Ease.OutQuad)
            //     .OnComplete(() => StartCoroutine(SpecialMoveCooltimeRoutine()));
            //
            // StartCoroutine(CreateAfterImageEffects(TelepoteDurationSec, moveDirection));
            
            StartCoroutine(CreateAfterImageEffects(transform.position , moveDirection));
            ManagerRoot.Sound.PlaySfx("Sword Woosh 18", 1f);
            ManagerRoot.Sound.PlaySfx("Small Monster Grunt (Gets hit) 01", .5f);
            
            transform.position = transform.position + SpecialMoveRange * moveDirection;
            StartCoroutine(SpecialMoveCooltimeRoutine());
            return true;
        }
        return false;
    }

    protected IEnumerator SpecialMoveCooltimeRoutine(){
        yield return new WaitForSeconds(SpecialMoveCoolTime);
        _isSpecialMove = true;
    }
    
    private IEnumerator CreateAfterImageEffects(Vector3 originalPos, Vector3 moveDirection)
    {
        float interval = .05f;
        float distanceBetweenImages = 1.0f;
        
        Vector3 targetPosition = originalPos + SpecialMoveRange * moveDirection;
        float totalDistance = Vector3.Distance(originalPos, targetPosition);
        int numberOfImages = Mathf.FloorToInt(totalDistance / distanceBetweenImages);

        Vector3 step = (targetPosition - originalPos) / numberOfImages;
        
        for (int i = 0; i < numberOfImages; i++)
        {
            GameObject createPrefab;
            if (CurrentFaction == Faction.Human)
            {
                createPrefab = afterImageEffectPrefabHuman;
            }
            else
            {
                createPrefab = afterImageEffectPrefabUndead;
            }
            GameObject afterImage = Instantiate(createPrefab, originalPos + i * step, Quaternion.identity);
            afterImage.transform.localScale = CurrentScale;

            if (moveDirection.x < 0)
            {
                afterImage.transform.localScale = new Vector3(-afterImage.transform.localScale.x, afterImage.transform.localScale.y, afterImage.transform.localScale.z);
            }

            Destroy(afterImage, 0.5f);
            yield return new WaitForSeconds(interval);
        }
    }

}
