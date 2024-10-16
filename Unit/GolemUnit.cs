using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LOONACIA.Unity.Managers;
using Unity.VisualScripting;
using UnityEngine;

public class GolemUnit : Unit
{
    public float SpecialAttackCoolTime = 5f; // 두손 내려치기 쿨타임
    public float SpecialAttackRange = 3f; // 두손 내려치기 공격 범위
    public float SpecialAttackDamage = 1f; // 두손 내려치기 공격 데미지
    
    float _jumpRange = 4f; // 점프 어택 타겟 선택 범위, 최대 점프 범위
    float _jumpDuration = 0.5f; // 점프 시간
    float _jumpHeight = 1.5f; // 점프 높이

    protected override void Init()
    {
        base.Init();
        reviveAudio = "Dragon Roar 09 Larger";
        StartCoroutine(SpecialAtkCooltimeRoutine());
        IsSpecialAttacking = false;
        
        if(CurrentFaction == Faction.Undead){
            UIWinPopup.IsFamilyImage[(int)UIWinPopup.Images.GolemImage] = true;

			SteamUserData.IsGolemRevived = true;
		}

	}
    
    Collider2D[] GetTargetListEcllipse(Vector3 centerPoint, float _width, float _height)
    {
        var targetUnits = Physics2D.OverlapAreaAll(
            new Vector2(centerPoint.x - _width, centerPoint.y - _height),
            new Vector2(centerPoint.x + _width, centerPoint.y + _height),
            targetLayerMask
        );
        return targetUnits;
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
        if(IsSpecialAttackable){
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
        IsCCImmunity = true;
        if(CurrentFaction == Faction.Human)
        {
            //애니메이션 실행
            CurrentAnim.Play(EXTRA_1);
            ManagerRoot.Sound.PlaySfx("Sword Woosh 18", 1f);
            
            //애니메이션 끝날때 까지 기다리기.
            AnimationClipOverrides overrides = new AnimationClipOverrides(4);
            (CurrentAnim.runtimeAnimatorController as AnimatorOverrideController)?.GetOverrides(overrides);
            float animLength = overrides[EXTRA_1].length - .3f;
            yield return new WaitForSeconds(animLength);
            
            var punchSounds = new string[]{"Earth Punch 1", "Earth Punch 2","Earth Punch 3", "Earth Punch 5"};
            ManagerRoot.Sound.PlaySfx(punchSounds[Random.Range(0, punchSounds.Length)], 1f);
            
            yield return new WaitForSeconds(.1f);
            CreateParticles();
        }
        else
        {
            for(int i = 0; i < 2; ++i)
            {
                GameObject target = GetFarthestTarget();
                if(target != null)
                {
                    var jumpSounds = new string[]{"Heavy Sword Swing 13", "Heavy Sword Swing 15"};
                    ManagerRoot.Sound.PlaySfx(jumpSounds[Random.Range(0, jumpSounds.Length)], 1f);
                    JumpTo(target.transform.position);
                    //애니메이션 실행
                    CurrentAnim.Play(EXTRA_1);

                    //애니메이션 끝날때 까지 기다리기.
                    AnimationClipOverrides overrides = new AnimationClipOverrides(4);
                    (CurrentAnim.runtimeAnimatorController as AnimatorOverrideController)?.GetOverrides(overrides);
                    float animLength = overrides[EXTRA_1].length;
                    yield return new WaitForSeconds(animLength + _jumpDuration);
                }
            }

        }

        
        AIStop();
        StartCoroutine(SpecialAtkCooltimeRoutine());
        IsCCImmunity = false;
        IsSpecialAttacking = false;       
        
    }

    public override void AnimEvent_HitMoment()
    {
        if(!IsSpecialAttacking){
            base.AnimEvent_HitMoment();
            var punchSounds = new string[]{"Earth Punch 1", "Earth Punch 2", "Earth Punch 3", "Earth Punch 5"};
            ManagerRoot.Sound.PlaySfx(punchSounds[Random.Range(0, punchSounds.Length)], 1f);
        }
        else{
            Collider2D[] targetList = GetTargetListEcllipse(transform.position, SpecialAttackRange, SpecialAttackRange * 0.7f);
            
            foreach(var target in targetList)
            {
                if(target == null) continue;

                if (target.TryGetComponent<IDamageable>(out IDamageable damageable))
                {
                    AttackInfo atkInfo = new AttackInfo(this, SpecialAttackDamage, attackingMedium: GetAttackTransformByUnitType());
                    ManagerRoot.Event.onUnitAttack?.Invoke(this, atkInfo);
                    damageable.TakeDamage(atkInfo);
                    damageable.TakeKnockDownJump(transform, 20f);
                }
            }
        }
    }

    GameObject GetFarthestTarget(){
        Collider2D[] targetList = GetTargetListEcllipse(transform.position, _jumpRange, _jumpRange);
        GameObject farthestTarget = null;
        float dist = 0;
        foreach(Collider2D target in targetList){
            if(target == null) break;
            float tmpDist = target.Distance(_collider).distance;
            if(tmpDist > dist){
                dist = tmpDist;
                farthestTarget = target.gameObject;
            }
        }
        return farthestTarget;

    }
	void JumpTo(Vector2 worldDest_)
	{
        //Flip 설정
        bool shouldFaceLeft = worldDest_.x < transform.position.x;
        _isStopAutoFlip = true; //회전 애니메이션 끝나고 false로 돌려줌.
        Flip(shouldFaceLeft);
        
		//그림자를 포함한 전체 오브젝트가 목적지로 이동함.
        transform.DOMove(worldDest_, _jumpDuration)
            .SetEase(Ease.Linear);
		
		
		//그림자를 제외한 몸체 부분은 로컬 포지션상으로 제자리 점프함.
		_bodyTransform.DOLocalJump(Vector3.zero, _jumpHeight, 1, _jumpDuration)
			.SetEase(Ease.Linear)
			.AppendCallback(() =>
			{
				ManagerRoot.Sound.PlaySfx("Impact Earthquake 2", 1f);
                ManagerRoot.Sound.PlaySfx("BoomSoundEffect_Edit", 1f);
			})
            .OnComplete(() =>
                {
                    _isStopAutoFlip = false;
                    if (Vector2.Distance(transform.position, _player.transform.position) < 10f)
                    {
                        CameraShake.Shake(0.12f, .12f);
                    }
					// ManagerRoot.Input.Vibration(0.7f);
                    CreateParticles();
                }
            );

		//Flip 설정
		// bool shouldFaceLeft = _player.transform.position.x + dest_.x < transform.position.x;
		// _isStopAutoFlip = true; //회전 애니메이션 끝나고 false로 돌려줌.
		// Flip(shouldFaceLeft);

		// //몸체의 회전 애니메이션
		// int direction = shouldFaceLeft ? 1 : -1;
		// _bodyTransform.DOLocalRotate(new(0, 0, 90 * direction), _jumpDuration)
    }
    
    private void CreateParticles()
    {
        var offset = IsFacingRight() ? new Vector3(1f, 0f, 0f) : new Vector3(-1f, 0f, 0f);
                    
        //Create CloudEffect
        GameObject cloudEffectGO = ManagerRoot.Resource.Load<GameObject>("Prefabs/Effects/VFXCloudEffect");
        GameObject cloudEffect = Instantiate(cloudEffectGO, transform.position + offset, Quaternion.identity);
        cloudEffect.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        Destroy(cloudEffect, 1f);
                    
        //Create DirtEffect
        GameObject dirtEffectGO = ManagerRoot.Resource.Load<GameObject>("Prefabs/Effects/DirtEffect");
        GameObject dirtEffect = Instantiate(dirtEffectGO, transform.position + offset, Quaternion.identity);
        dirtEffect.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        Destroy(dirtEffect, 1f);
                    
        //Create CrackParticle
        GameObject crackParticleGO = ManagerRoot.Resource.Load<GameObject>("Prefabs/Effects/CrackParticle");
        GameObject crackParticle = Instantiate(crackParticleGO, transform.position + offset, Quaternion.identity);
        crackParticle.GetComponent<CrackEffectController>().Init(IsFacingRight(), fadeBurn: true, isUndead: CurrentFaction == Faction.Undead);
        Destroy(crackParticle, 7f);
    }

}
