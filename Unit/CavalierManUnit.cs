using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using BehaviorDesigner.Runtime;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;

public class CavalierManUnit : Unit
{
    [Header("Charge")]
    public float chargeMoveSpeed = 5f;
    public float chargeKnockBackPower = 100f;
    public float ChargeCoolTime = 1f;
    public float chargeKnockDownRadius = 3;
    private float _chargeTime = 1.5f; // duration from first knockback unit
    Vector2 _chargeDirection;


    public override void Start()
    {
        base.Start();
        IsSpecialAttackable = true;
    }

    public override void AIDetectAround(Collider2D[] detectEnemies_){
        if(IsSpecialAttacking){
            foreach (var thing in detectEnemies_)
            {
                if(thing == null) break;
                if(thing == _collider) continue;

                // 돌진중 적이랑 부딫혔다면
                bool isCrashWithEnemy = IsExistInLayerMask(thing.gameObject.layer, targetLayerMask) && _collider.Distance(thing).distance <= chargeKnockDownRadius;
                if(isCrashWithEnemy){
                    if( thing.gameObject.TryGetComponent(out Unit unit) && unit != null)
                        unit.TakeKnockDown(transform, chargeKnockBackPower);
                }
            }
        }
        else{
            base.AIDetectAround(detectEnemies_);
        }
    }
    
    public override EnumBTState AIAttackCheck(){  
        if(IsSpecialAttacking){
            SetVelocity(chargeMoveSpeed * _chargeDirection);
            CurrentAnim.Play(JUMP);
            return EnumBTState.Running;
        }   
        return base.AIAttackCheck();    
    }
    public override bool AISpecialAttack(){
        if(IsSpecialAttacking) return true;
        if(IsSpecialAttackable && AttackTarget.layer != Layers.Player){
            AIStop();
            IsSpecialAttackable = false;
            
            _chargeDirection = (AttackTarget.transform.position - transform.position).normalized;

            StartCoroutine(StartSpecialAttackRoutine());
            // ShakeRotate();
            return true;
        }
        return false;
    }

    protected IEnumerator SpecialAtkCooltimeRoutine(){
        yield return new WaitForSeconds(ChargeCoolTime);
        IsSpecialAttackable = true;
    }
    
    protected IEnumerator StartSpecialAttackRoutine()
    {
        IsSpecialAttacking = true;
        
        AttackingTarget = null;
        IsWalking = false;
        //애니메이션 실행
        CurrentAnim.Play(JUMP);
        SetVelocity(chargeMoveSpeed * _chargeDirection);
        Flip(_chargeDirection.x < 0);

        yield return new WaitForSeconds(_chargeTime);
        
        _chargeDirection *= -1;
        Flip(_chargeDirection.x < 0);

        yield return new WaitForSeconds(_chargeTime);
        
        IsSpecialAttacking = false;
        AIStop();
        StartCoroutine(SpecialAtkCooltimeRoutine());
    }
    
}