using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityRigidbody;
using UnityEngine;

public class ShieldManUnit : Unit
{
    public float BashCoolTime = 5f;
    // public float BashPower = 20f;
    float _bashPower = 10f;

    protected override void Init()
    {
        base.Init();
        IsSpecialAttackable = true;
    }

    public override bool AISpecialAttack(){
        if(IsSpecialAttackable){
            AIStop();
            IsSpecialAttackable = false;
            TrySpecialAttack(AttackTarget);
            StartCoroutine(SpecialAtkCooltimeRoutine());
            return true;
        }
        return false;
    }

    public void TrySpecialAttack(GameObject target_)
    {
        SetVelocity(_bashPower * (target_.transform.position - transform.position));
        StartCoroutine(StartSpecialAttackAnimRoutine(target_));
    }

    protected IEnumerator SpecialAtkCooltimeRoutine(){
        yield return new WaitForSeconds(BashCoolTime);
        IsSpecialAttackable = true;
    }
    
    protected IEnumerator StartSpecialAttackAnimRoutine(GameObject target_)
    {
        IsSpecialAttacking = true;
        
        //_currentTarget = target_.GetComponent<Unit>();
        AttackingTarget = target_;
        //애니메이션 실행
        CurrentAnim.Play(EXTRA_1);

        //애니메이션 끝날때 까지 기다리기.
        AnimationClipOverrides overrides = new AnimationClipOverrides(4);
        (CurrentAnim.runtimeAnimatorController as AnimatorOverrideController)?.GetOverrides(overrides);
        float animLength = overrides[EXTRA_1].length;
        yield return new WaitForSeconds(animLength);
        
        AttackingTarget = null;
        IsSpecialAttacking = false;
    }

}
