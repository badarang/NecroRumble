using System;
using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public partial class Unit
{
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    private string _currentState;

    //애니메이터의 State 이름.
    protected const string IDLE = "Idle";
    protected const string WALK = "Walk";
    protected const string ATTACK = "Attack";
    protected const string TAKE_HIT = "TakeHit";
    protected const string DEATH = "Death";
    protected const string ENTRY = "Entry";
    protected const string JUMP = "Jump";
    protected const string GET_UP = "GetUp";
    protected const string EXTRA_1 = "Extra_1";
    protected const string EXTRA_2 = "Extra_2";
    protected const string RUNAWAY = "Runaway";
    
    protected void ChangeAnimationState(string newState)
    {
        if (_currentState == newState) return;
        
        CurrentAnim.Play(newState);
        
        _currentState = newState;
    }

    void UpdateAnimations()
    {
        if (IsDead || IsAttacking)
        {
            return;
        }
        else if (IsWalking)
        {
            ChangeAnimationState(WALK);
        }
        
    }

    IEnumerator PlayDeathAnimation()
    {
        CurrentAnim.Play(DEATH);
        float speed = originAnimatorSpeed * Statics.UnitDyingSpeedMultiplier;
        CurrentAnim.speed = speed;

        List<string> bodyFallSoundList = new List<string>()
        {
            "Body Fall 1",
            "Body Fall 2",
            "Body Fall 13",
            "Body Fall 19",
        };
        
        var length = GetClipLength(DEATH) / speed;
        yield return new WaitForSeconds(.1f);
        ManagerRoot.Sound.PlaySfx(bodyFallSoundList[UnityEngine.Random.Range(0, bodyFallSoundList.Count)], 1f);
        yield return new WaitForSeconds(length - .1f);      


        //애니메이션 속도 원상복귀. (if 다른데서 속도 건들지 않았으면)
        if (Math.Abs(CurrentAnim.speed - speed) <= Mathf.Epsilon)
        {
            CurrentAnim.speed = originAnimatorSpeed;
        }
    }
    
    
    IEnumerator PlayTurnUndeadAnimation(bool isReRevive_ = false)
    {
        CurrentFaction = Faction.Undead;
        InitCircleShadow();
        IsReviving = true;
        CurrentAnim.SetFloat("speed", -1); //역재생
        CurrentAnim.speed = 1;
        CurrentAnim.Play(DEATH, -1, 1f);

        //재생 속도 부활 시간에 맞추기
        float animSpeed = GetClipLength(DEATH) / Statics.RevivingTime;
        CurrentAnim.speed = animSpeed;

        yield return new WaitForSeconds(Statics.RevivingTime);
        
        CurrentAnim.SetFloat("speed", 1);
        CurrentAnim.Play(IDLE);
        CurrentAnim.speed = originAnimatorSpeed;
        _rigid.mass = _mass;
        _turnUndeadCoroutine = null;
        IsReviving = false;
        
        //첫 소환인 경우 onUnitSpawn 이벤트 호출
        if (!isReRevive_) ManagerRoot.Event.onUnitSpawn?.Invoke(this);
    }

    [Button]
    [InfoBox("Unit 스크립트 이름 + 액션 이름의 형태를 가진 클립을 Resources/Animations 폴더에서 찾아 자동으로 할당합니다.")]
    private void AutoFindAnimations()
    {
        var hAnim = gameObject.FindChild("@HumanModel").GetComponent<Animator>();
        var uAnim = gameObject.FindChild("@UndeadModel").GetComponent<Animator>();

        for (int k = 0; k < 2; k++)
        {
            var anim = k == 0 ? hAnim : uAnim;
            if (anim == null) continue;
            AnimationClipOverrides overrides =
                new AnimationClipOverrides(anim.runtimeAnimatorController.animationClips.Length);
            (anim.runtimeAnimatorController as AnimatorOverrideController)?.GetOverrides(overrides);
            for (int i = 0; i < overrides.Count; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var unitName = GetType().Name;
                    //스크립트 이름에서 Unit 제거
                    if (unitName.EndsWith("Unit")) unitName = unitName.Substring(0, unitName.Length - 4);
                    var acitonName = overrides[i].Key.name;
                    //액션 명명 규칙 예외처리
                    if (acitonName == "Death") acitonName = "Die";
                    else if (acitonName == "TakeHit") acitonName = "Hit";
                    //액션 이름에 _H, _U 우선순위에 따라 붙이기
                    var f = Mathf.Abs(2 * k - j);
                    var addOn = f == 0 ? "_H" : f == 1 ? "" : "_U"; //아무튼 휴먼이면 H먼저, 언데드면 U먼저 우선순위로 찾음.
                    string targetName = unitName + acitonName + addOn;
                    //실제 클립 가져오기
                    AnimationClip clip = ResourceManager.LoadAny(targetName, typeof(AnimationClip)) as AnimationClip;
                    if (clip != null)
                    {
                        overrides[overrides[i].Key.name] = clip;
                        break;
                    }
                }
            }
            (anim.runtimeAnimatorController as AnimatorOverrideController)?.ApplyOverrides(overrides);
            #if UNITY_EDITOR
            //시작 스프라이트 할당
            var firstIdleSprite = overrides["Idle"]?.AllSprites()?[0];
            anim.GetComponent<SpriteRenderer>().sprite = firstIdleSprite;
            #endif
        }
    }

    protected float GetClipLength(string stateName_)
    {
        AnimationClipOverrides overrides = new AnimationClipOverrides(4);
        (CurrentAnim.runtimeAnimatorController as AnimatorOverrideController)?.GetOverrides(overrides);
        float animLength = overrides[stateName_].length;

        return animLength / CurrentAnim.GetFloat("atkAnimSpd");
    }
}
