using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using LOONACIA.Unity.Managers;
using Pathfinding;
using UnityEngine;

public class HorseManUnit : Unit
{
    [Header("돌진 설정")]
    [Tooltip("돌진 시 이동 속도")]
    public float chargeMoveSpeed = 5f; 

    [Tooltip("특수 공격 감지 범위")]
    public float SpecialAttackDetectRadius = 10f; 

    [Tooltip("넉백 파워")]
    public float chargeKnockBackPower = 100f;

    [Tooltip("넉다운 범위")]
    public float chargeKnockDownRadius = 2f; 

    [Tooltip("차지 쿨타임")]
    public float ChargeCoolTime = 1f; 

    [Tooltip("돌진 준비 시간")]
    public float ChargeEnergyTime = 3f; 

    [Tooltip("돌진 유지 시간")]
    public float chargeAttackTime = 1.5f; 

    private float speedMultiplier = 1f;
    private float _soundPlayDelay = 0f;
    private Vector2 _chargeDirection;
    private float SetDirectionTime;
    private readonly List<Unit> _unitsAttackedPerCharge = new();
    private bool _hasHitPlayer;

    public override void Start()
    {
        base.Start();
        reviveAudio = "Small Monster Breathing Larger 01"; 
        IsSpecialAttackable = true;
        SetDirectionTime = ChargeEnergyTime - 0.5f; 
    }

    protected override void Init()
    {
        base.Init();
        if (CurrentFaction == Faction.Undead)
        {
            UIWinPopup.IsFamilyImage[(int)UIWinPopup.Images.HorseManImage] = true;
            SteamUserData.IsHorsemanRevived = true;
        }
    }

    public override void Update()
    {
        base.Update();
        SetAttackTarget(); 
        HandleSpecialAttack();
        AdjustSpeedMultiplier();
    }

    // 공격 타겟 설정
    private void SetAttackTarget()
    {
        if (AttackTarget != null) return;

        AttackTarget = CurrentFaction == Faction.Human
            ? GameManager.Instance.GetPlayer().gameObject
            : GetStrongestEnemyTarget();
    }

    private GameObject GetStrongestEnemyTarget()
    {
        float maxHp = 0;
        GameObject target = null;

        foreach (var unit in ManagerRoot.Unit.GetAllAliveHumanUnits())
        {
            if (unit.GetBaseStats().BaseMaxHp > maxHp)
            {
                maxHp = unit.GetBaseStats().BaseMaxHp;
                target = unit.gameObject;
            }
        }
        return target;
    }

    // 특수 공격 처리
    private void HandleSpecialAttack()
    {
        if (IsDead || !IsWalking) return;

        if (IsSpecialAttacking)
        {
            PlayFootstepSound();
            DetectAndAttackEnemies();
        }
        else if (IsSpecialAttackable)
        {
            CheckForSpecialAttackTrigger();
        }
    }

    private void PlayFootstepSound()
    {
        if (_soundPlayDelay > 0f)
        {
            _soundPlayDelay -= Time.deltaTime;
            return;
        }

        string sound = CurrentFaction == Faction.Human 
            ? "Horse Footsteps Metal_4" 
            : "Horse Footsteps Metal_2";
        
        ManagerRoot.Sound.PlaySfx(sound, 1f);
        _soundPlayDelay = 0.3f;
    }

    private void DetectAndAttackEnemies()
    {
        var detectEnemies_ = new Collider2D[100];
        Physics2D.OverlapCircleNonAlloc(transform.position, DetectRadius, detectEnemies_, targetLayerMask);

        foreach (var enemy in detectEnemies_)
        {
            if (enemy == null || enemy == _collider) continue;

            AttackInfo atkInfo = new(this, instanceStats.FinalAttackDamage, attackingMedium: transform);

            if (_collider.Distance(enemy).distance <= chargeKnockDownRadius)
            {
                HandleEnemyHit(enemy, atkInfo);
            }
        }
    }

    private void HandleEnemyHit(Collider2D enemy, AttackInfo atkInfo)
    {
        if (enemy.TryGetComponent<Unit>(out var unit) && !_unitsAttackedPerCharge.Contains(unit))
        {
            _unitsAttackedPerCharge.Add(unit);
            unit.TakeDamage(atkInfo);
            PlayImpactSound();
            unit.TakeKnockDown(transform, chargeKnockBackPower);
        }
        else if (enemy.TryGetComponent<Player>(out var player) && !_hasHitPlayer)
        {
            _hasHitPlayer = true;
            player.TakeDamage(atkInfo);
            PlayImpactSound();
            player.TakeKnockDown(transform, chargeKnockBackPower, 2f);
        }
    }

    private void PlayImpactSound()
    {
        var attackSounds = new List<string>
        {
            "Punch Impact (Flesh) 3",
            "Punch Impact (Flesh) 5",
            "Punch Impact (Flesh) 6"
        };
        ManagerRoot.Sound.PlaySfx(attackSounds[UnityEngine.Random.Range(0, attackSounds.Count)], 1f);
    }

    private void CheckForSpecialAttackTrigger()
    {
        if (AttackTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, AttackTarget.transform.position);

        if (distanceToPlayer <= SpecialAttackDetectRadius)
        {
            AISpecialAttack();
        }
    }

    private void AdjustSpeedMultiplier()
    {
        if (speedMultiplier <= 1f) return;

        speedMultiplier = Mathf.Max(1f, speedMultiplier - Time.deltaTime * 0.5f);
    }

    public override void TurnUndeadEvent() => AttackTarget = null;

    public override void AIDetectAround(Collider2D[] detectEnemies_)
    {
        if (!IsSpecialAttacking) base.AIDetectAround(detectEnemies_);
    }

    public override EnumBTState AIAttackCheck()
    {
        if (IsSpecialAttacking && IsWalking)
        {
            SetVelocity(chargeMoveSpeed * _chargeDirection);
            return EnumBTState.Running;
        }
        return base.AIAttackCheck();
    }

    public override bool AISpecialAttack()
    {
        if (!IsSpecialAttackable || IsSpecialAttacking) return false;

        AIStop();
        IsSpecialAttackable = false;
        StartCoroutine(StartSpecialAttackRoutine());

        return true;
    }

    protected IEnumerator SpecialAtkCooltimeRoutine()
    {
        yield return new WaitForSeconds(ChargeCoolTime);
        IsSpecialAttackable = true;
    }

    protected IEnumerator StartSpecialAttackRoutine()
    {
        IsSpecialAttacking = true;
        IsCCImmunity = true;
        CurrentAnim.Play(JUMP);
        GetComponent<Collider2D>().isTrigger = true;

        PlayChargeSound();
        TriggerChargeEffect();
        yield return new WaitForSeconds(SetDirectionTime);

        SetDirectionToTarget();
        yield return new WaitForSeconds(ChargeEnergyTime - SetDirectionTime);

        StartCharge();
        yield return new WaitForSeconds(chargeAttackTime);

        EndCharge();
    }

    private void PlayChargeSound()
    {
        string sound = CurrentFaction == Faction.Human 
            ? "Dragon Breath Fire_12" 
            : "Small Monster Breathing Larger 01";
        
        ManagerRoot.Sound.PlaySfx(sound, 1f);
    }

    private void TriggerChargeEffect()
    {
        if (TryGetComponent(out FeedbackController feedback))
        {
            feedback.SetChargeEffect(ChargeEnergyTime);
        }
    }

    private void StartCharge()
    {
        IsWalking = true;
        IsCCImmunity = false;
        speedMultiplier = 2f;

        SetVelocity(chargeMoveSpeed * speedMultiplier * _chargeDirection);
        _unitsAttackedPerCharge.Clear();
        _hasHitPlayer = false;
        //TODO: 게임패드 진동 넣을지 말지 여부 결정
        // ManagerRoot.Input.Vibration(0.3f, chargeAttackTime, true, false);
    }

    private void EndCharge()
    {
        GetComponent<Collider2D>().isTrigger = false;
        IsSpecialAttacking = false;
        AIStop();
        StartCoroutine(SpecialAtkCooltimeRoutine());
    }

    private void SetDirectionToTarget()
    {
        if (AttackTarget == null) return;
        _chargeDirection = (AttackTarget.transform.position - transform.position).normalized;
    }
}
