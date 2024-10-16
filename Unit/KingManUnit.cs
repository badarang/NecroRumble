using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using LOONACIA.Unity;
using MoreMountains.Feedbacks;

public class KingManUnit : Unit
{
    
    public float HealCoolTime = 5f;
    public float HealDistance = 3f;
    public int HealAmount = 5;
    public float BashPower = 20f;

    protected override void Init()
    {
        base.Init();
        if(CurrentFaction == Faction.Undead){
            IsSpecialAttackable = true;
            targetLayerMask |= Layers.Player.ToMask();
            // _addMoveWeightBehaviorUndead = new KingManAddMoveWeightBehavior_U();
        }
        else{
            //instanceStats.AttackRange = BaseStats.BaseAttackRange;
        }
    }

    public override void AIDetectAround(Collider2D[] detectEnemies_){
        base.AIDetectAround(detectEnemies_);
        if(CurrentFaction == Faction.Undead){
            AttackTarget = _player.gameObject;
        }
    }
    public override bool AIAttack()
    {
        if(CurrentFaction == Faction.Undead){
            return false;
        }
        else return base.AIAttack();
    }
    public override bool AISpecialAttack(){
        if(IsSpecialAttacking) return true;
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
        if (IsSpecialAttacking || IsDead) return;
        StartCoroutine(StartSpecialAttackAnimRoutine(target_));
    }

    protected IEnumerator SpecialAtkCooltimeRoutine(){
        yield return new WaitForSeconds(HealCoolTime);
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
