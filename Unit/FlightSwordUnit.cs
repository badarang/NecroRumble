using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class FlightSwordUnit : Unit
{
    [Header("Auror")]
    public float AurorDamage = 20f; // 검기 데미지
    public float AurorRange = 10f; // 검기 범위
    public float AurorSpeed = 10f; // 검기 속도
    public float AurorCoolTime = 10f; // 검기 쿨타임
    public float AurorDuration = 5f; // 검기 지속 시간

    [Header("SpecialAttack")]
    public float SpecialAttackCoolTime = 10f; // 스킬 쿨타임
    [Header("Undead")]
    public float SpecialAttackDamage_U = 100f; // 언데드 스킬 데미지
    public float SpecialAttackRange_U = 10f;    // 언데드 스킬 범위, 자기 자신 중심
    [Header("Human")]
    public float SpecialAttackDamage_H = 1004f; // 휴면 스킬 데미지
    public float SpecialAttackRange_H = 5f; // 휴면 스킬 범위, AttackTarget 중심
    bool isAurorMode = false;

    GameObject aurorPrefab;
    Sprite aurorSprite_U;
    Sprite aurorSprite_H;
    
    Coroutine _skillCoroutine;
    
    private Vector3 _humanOffset = new Vector3(1f, 0.5f, 0f);
    private Vector3 _undeadOffset = new Vector3(1f, .5f, 0f);


    public override void Start() {
        base.Start();
        reviveAudio = "Dragon TAKING FLIGHT 10 Larger";
        ManagerRoot.Sound.PlaySfx("Aggro 5 Larger", 1f);
        aurorPrefab = ManagerRoot.Resource.Load<GameObject>("Prefabs/Skills/AurorProjectile");
        aurorSprite_U = ManagerRoot.Resource.Load<Sprite>("Sprites/Skills/Auror_U");
        aurorSprite_H = ManagerRoot.Resource.Load<Sprite>("Sprites/Skills/Auror_H");

    }
    protected override void Init()
    {
        base.Init();
        if(CurrentFaction == Faction.Undead){
            UIWinPopup.IsFamilyImage[(int)UIWinPopup.Images.FlightKnightImage] = true;

			SteamUserData.IsDemonRevived = true;
		}
		IsSpecialAttackable = false;
        StartCoroutine(SpecialAtkCooltimeRoutine());
        StartCoroutine(AurorCooltimeRoutine());
    }
    
    public override bool AISpecialAttack(){
        if(IsSpecialAttacking) return true;
        // if(isBerserkMode) return false;
        if(IsSpecialAttackable){
            AIStop();
            IsSpecialAttackable = false;

            StartCoroutine(StartSpecialAttackRoutine());
        }
        return false;
    }

    
    protected IEnumerator StartSpecialAttackRoutine()
    {
        if(IsAttacking){
            IsSpecialAttackable = true;
        } 
        else{
            IsSpecialAttacking = true;
            IsCCImmunity = true;

            float animLength;
            AnimationClipOverrides overrides = new AnimationClipOverrides(4);
            (CurrentAnim.runtimeAnimatorController as AnimatorOverrideController)?.GetOverrides(overrides);
            if(CurrentFaction == Faction.Human){
                CurrentAnim.Play(EXTRA_1);
                animLength = overrides[EXTRA_1].length;
            }           
            else{
                CurrentAnim.Play(EXTRA_2);
                animLength = overrides[EXTRA_2].length;
            } 

            ManagerRoot.Sound.PlaySfx("Large Monster Grunt (Gets hit) 01", 1f);
            ManagerRoot.Sound.PlaySfx("Impact 12", 1f);
            
            //애니메이션 끝날때 까지 기다리기.
            yield return new WaitForSeconds(animLength);
            
            AIStop();
            StartCoroutine(SpecialAtkCooltimeRoutine());
            IsCCImmunity = false;
            IsSpecialAttacking = false;
        }
    }
    protected IEnumerator SpecialAtkCooltimeRoutine(){
        yield return new WaitForSeconds(SpecialAttackCoolTime);
        IsSpecialAttackable = true;
    }

    #region Auror
    protected IEnumerator AurorDurationRoutine(){
        yield return new WaitForSeconds(AurorDuration);
        isAurorMode = false;
        StartCoroutine(AurorCooltimeRoutine());
    }
    protected IEnumerator AurorCooltimeRoutine(){
        yield return new WaitForSeconds(AurorCoolTime);
        isAurorMode = true;
        StartCoroutine(AurorDurationRoutine());
    }

    public override void AnimEvent_HitMoment()
    {
        if(!IsAttacking && IsSpecialAttacking){
            Collider2D[] detectEnemies_ = new Collider2D[100];
            float damage;
            if(CurrentFaction == Faction.Human){
                // Todo : 휴먼, AttackTarget 중심으로 원형 / 가렌궁
                //Create SwordEffect
                if (AttackTarget == null) return;
                GameObject swordEffectGO = Resources.Load<GameObject>("Prefabs/Effects/SwordEffectAll");
                GameObject swordEffect = Instantiate(swordEffectGO, AttackTarget.transform.position + new Vector3(0f, 4f, 0f), Quaternion.identity);
                if (swordEffect.transform.GetChild(1).TryGetComponent(out SwordEffectController swordEffectController))
                {
                    swordEffectController.transform.localPosition = Vector3.zero;
                    swordEffectController.Init(SpecialAttackRange_H);
                }
                
                ManagerRoot.Sound.PlaySfx("Explosion Flesh 5", 1f);
                ManagerRoot.Sound.PlaySfx("Explosion Flesh 6", 1f);
				// ManagerRoot.Input.Vibration(intensity: 0.4f, duration: 0.17f);

				damage = SpecialAttackDamage_H;
                Physics2D.OverlapCircleNonAlloc(AttackTarget.transform.position, SpecialAttackRange_H, detectEnemies_, targetLayerMask);
                
                foreach (var thing in detectEnemies_)
                {
                    if(thing == null) break;
                    if(thing == _collider) continue;

                    if (thing.gameObject.TryGetComponent(out IDamageable damageable))
                    {
                        AttackInfo attackInfo = new (this, damage, attackingMedium: transform);
                        damageable.TakeDamage(attackInfo);
                    }
                }
            }
            else
            {
                if (_skillCoroutine != null) StopCoroutine(_skillCoroutine);
                _skillCoroutine = StartCoroutine(UndeadSpecialSkill());
            }
        }
        else{
            // Attack
            base.AnimEvent_HitMoment();
            // Auror
            if(isAurorMode){
                bool canAttack = AttackTarget != null && (AttackTarget.layer == Layers.Player || !AttackTarget.GetComponent<Unit>().IsDead) && !IsStun;
                if(canAttack ){
                    var offset = (CurrentFaction == Faction.Undead) ? _undeadOffset : _humanOffset;
                    if (!IsFacingRight())
                    {
                        offset.x = Mathf.Abs(offset.x) * (-1);
                    }
                    
                    GameObject auror = ManagerRoot.Resource.Instantiate(aurorPrefab, transform.position + offset, Quaternion.identity);
                    if (auror.TryGetComponent(out AurorProjectile aurorProjectile))
                    {
                        // 적당한 이펙트 소리 넣기?
                        var aurorSounds = new List<string> {"Shadow Punch 2", "Shadow Punch 4"};
                        ManagerRoot.Sound.PlaySfx(aurorSounds[Random.Range(0, aurorSounds.Count)], 1f);
                        
                        aurorProjectile.Init(this, AurorSpeed, AurorRange, AurorDamage);
                        
                        // Todo : 검기 이펙트 더 멋지게

                        // Faction따라 타겟레이어, 검기 스프라이트 바꿔줌 (색만 다름)
                        if(CurrentFaction == Faction.Human)
                            aurorProjectile.SetFactonInfo(aurorSprite_H, targetLayerMask);
                        else
                            aurorProjectile.SetFactonInfo(aurorSprite_U, targetLayerMask);

                        Vector3 direction = AttackTarget.transform.position - transform.position;
                        
                        aurorProjectile.transform.rotation = Quaternion.Euler(0, 0, 0);
                        aurorProjectile.SetDirection(direction);
                        aurorProjectile.SetRotation(direction, false);

                    }
                }
            }
        }
    }

    IEnumerator UndeadSpecialSkill()
    {
        // Todo : 언데드, 자기 자신 중심으로 원형 / 브랜드 W, 워웍 w?
        float damage = SpecialAttackDamage_U;
        Collider2D[] detectEnemies_ = new Collider2D[100];
        
        //기존 CrawlingEffect 방식
        // GameObject CrawlingEffectGO = Resources.Load<GameObject>("Prefabs/Effects/CrawlingEffect");
        // GameObject CrawlingEffect = Instantiate(CrawlingEffectGO, transform.position, Quaternion.identity);
        //         
        // ParticleSystem.MainModule mainParticleSystem = CrawlingEffect.GetComponent<ParticleSystem>().main;
        // mainParticleSystem.startSize = SpecialAttackRange_U * 2;

        if (AttackTarget == null) yield break;
        
        float angleIncrement = 360f / 3; // 120 degrees between each point
        float currentAngle = 0f;

        for (int i = 0; i < 3; i++)
        {
            Vector3 spawnPosition = transform.position + new Vector3(Mathf.Cos(Mathf.Deg2Rad * currentAngle), Mathf.Sin(Mathf.Deg2Rad * currentAngle)) * (SpecialAttackRange_U / 1.5f);
            GameObject swordEffectGO = Resources.Load<GameObject>("Prefabs/Effects/SwordEffectAllPurple");
            GameObject swordEffect = Instantiate(swordEffectGO,spawnPosition + new Vector3(0f, 4f, 0f), Quaternion.identity);
            if (swordEffect.transform.GetChild(1).TryGetComponent(out SwordEffectController swordEffectController))
            {
                swordEffectController.transform.localPosition = Vector3.zero;
                swordEffectController.Init(SpecialAttackRange_U);
            }
            Physics2D.OverlapCircleNonAlloc(spawnPosition, SpecialAttackRange_U, detectEnemies_, targetLayerMask);
            var swordSounds = new List<string> {"Explosion Flesh 2", "Explosion Flesh 5"};
            ManagerRoot.Sound.PlaySfx(swordSounds[Random.Range(0, swordSounds.Count)], 1f);
            ManagerRoot.Sound.PlaySfx("Explosion Flesh 6", 1f);
            
            currentAngle += angleIncrement;
            foreach (var thing in detectEnemies_)
            {
                if(thing == null) break;
                if(thing == _collider) continue;

                if (thing.gameObject.TryGetComponent(out IDamageable damageable))
                {
                    AttackInfo attackInfo = new (this, damage, attackingMedium: transform);
                    damageable.TakeDamage(attackInfo);
                }

                if (thing.gameObject.TryGetComponent(out Unit unit))
                {
                    GameObject flameGO = ManagerRoot.Resource.Load<GameObject>("Prefabs/Effects/VFXFlame");
                    GameObject flameEffect = Instantiate(flameGO, unit.transform.position, Quaternion.identity);
                    flameEffect.transform.SetParent(unit.transform);
                    Destroy(flameEffect, 1f);

                    if (unit.IsDead == false)
                    {
                        var fireSounds = new List<string> {"Flame Whoosh 3", "Flame Whoosh 4", "Flame Whoosh 9"};
                        ManagerRoot.Sound.PlaySfx(fireSounds[Random.Range(0, fireSounds.Count)], 1f);
						// ManagerRoot.Input.Vibration(intensity: 0.4f, duration: 0.17f);
                    }
                }
            }
            
            yield return new WaitForSeconds(0.3f);
        }
    }
    
    #endregion
}
