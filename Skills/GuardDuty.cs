using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using TMPro;
using UnityEngine;

public class GuardDuty : SkillBase
{
    private GuardDutyData _data;
    private Player _player;
    private List<Unit> _swordMansInEffect = new();
    private Coroutine _loopCheckRoutine;


    public override void OnSkillAttained()
    {
        Init();
    }
    
    public override void OnSkillUpgrade()
    {
        if (SkillLevel == 2)
        {
            //플레이어가 피해를 입을 때마다 주변 전사 유닛이 피해를 대신 입음.
            ManagerRoot.Event.onIDamageableTakeHit += SwordMansTakeDamageForPlayer;
        }
    }

    private void SwordMansTakeDamageForPlayer(IDamageable damaged_, AttackInfo attackinfo_, int modifieddamage_, ref int finaldamage_)
    {
        if (damaged_ is Player)
        {
            //유닛당 대신 받아야 하는 피해량 계산
            int damagePortion = (int) (finaldamage_ * _data.TakeDamageInsteadPercentage / 100f);
            if (damagePortion <= 0) damagePortion = 1;
            
            //플레이어가 피해를 입었을 때 주변 전사 유닛들에게 피해를 대신 받게 함.
            foreach (var swordMan in _swordMansInEffect)
            {
                //플레이어가 받는 피해 경감
                finaldamage_ -= damagePortion;
                if (finaldamage_ < 0) finaldamage_ = 0;
                
                //스워드맨 피해 받기
                var atkInfo = new AttackInfo(attackinfo_.attacker, damagePortion);
                swordMan.TakeDamage(atkInfo);
            }
        }
    }

    public override void OnBattleStart()
    {
    }

    public override void OnBattleEnd()
    {
        
    }
    
    private void OnNewSwordManEffected(List<Unit> swordMans_)
    {
        foreach (var swordMan in swordMans_)
        {
            //스킬 효과 적용
            if (swordMan.instanceStats.GetModifierByName(nameof(GuardDuty)) is { } mod)
            {
                //이미 GuardDuty라는 이름을 가진 StatModifier가 있으면 값만 바꿔줌
                mod.value = _data.DamageReducePercent;
            }
            else
            {
                //GuardDuty라는 이름을 가진 StatModifier가 없으면 새로 만들어서 추가
                StatModifier newMod = new StatModifier(StatType.DamageReduce, nameof(GuardDuty), _data.DamageReducePercent, StatModifierType.BaseAddition, false);
                ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(swordMan, newMod);
            }
            
            //비주얼 이펙트 추가
            SetVisualEffect(swordMan, true);
        }
    }

    private void OnSwordMansEffectRemoved(List<Unit> swordMans_)
    {
        foreach (var swordMan in swordMans_)
        {
            //스킬 효과 제거
            if (swordMan.instanceStats.GetModifierByName(nameof(GuardDuty)) is { } mod)
            {
                mod.value = 0;
            }
            else
            {
                Debug.LogWarning("GuardDuty 스킬 효과 제거 시도했으나 해당 스킬 효과가 없음");
            }
            
            //비주얼 이펙트 제거
            SetVisualEffect(swordMan, false);
        }
    }

    private void SetVisualEffect(Unit swordMan_, bool effectOn_)
    {
        if (effectOn_) //비주얼 이펙트 켜기
        {
            //실제 비주얼 이펙트 켜는 로직
            swordMan_.Feedback.ApplyShieldEffect(true);
        }
        else //비주얼 이펙트 끄기
        {
            //실제 비주얼 이펙트 끄는 로직
            swordMan_.Feedback.ApplyShieldEffect(false);
        }
    }
    
    private IEnumerator CheckSwordmanInRangeLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(_data.LoopCheckInterval);
        while (true)
        {
            //플레이어 주변 범위 내에 있는 스워드맨들을 찾아서 저장
            List<Unit> temp_swordMansInRange = 
                Physics2D.OverlapCircleAll(_player.transform.position, _data.Range, Layers.UndeadUnit.ToMask())
                    .Select(x => x.GetComponent<Unit>())
                    .Where(x => x.UnitType == (int)UnitType.SwordMan)
                    .ToList();
            
            //이전에 저장한 스워드맨들과 비교해서 새로 추가된 스워드맨들과 제거된 스워드맨들을 찾음
            var newSwordMans = temp_swordMansInRange.Except(_swordMansInEffect);
            var removedSwordMans = _swordMansInEffect.Except(temp_swordMansInRange);
            OnNewSwordManEffected(newSwordMans.ToList());
            OnSwordMansEffectRemoved(removedSwordMans.ToList());

            _swordMansInEffect = temp_swordMansInRange;
            
            yield return wait;
        }
    }
    
    private void Init()
    {
        _data = LoadData<GuardDutyData>();
        _player = GameManager.Instance.GetPlayer();
        Id    = _data.Id;
        Name  = _data.name;
        
        //공격 속도 디스어드밴티지 적용
        StatModifier mod = new StatModifier(StatType.AttackPerSec, nameof(GuardDuty)+" atkSpeed loss", _data.AtkSpeedModifyingPercent, StatModifierType.Percentage, false);
        ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.SwordMan, mod, false);
        
        //반복 체크 루틴 시작
        if (_loopCheckRoutine != null) StopCoroutine(_loopCheckRoutine);
        _loopCheckRoutine = StartCoroutine(CheckSwordmanInRangeLoop());
    }

    
}
