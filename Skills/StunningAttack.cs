using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class StunningAttack : SkillBase
{
    private StunningAttackData _data;
    
    private Dictionary<Unit, int> _attackCountDict = new();
    
    private List<Unit> _strongStunSwordmans = new();
    
    public override void OnSkillAttained()
    {
        Init();
        
        ManagerRoot.Event.onIDamageableTakeHit += OnIDamageableTakeDamage;
    }


    public override void OnSkillUpgrade()
    {
        if (SkillLevel == 2)
        {
            //복귀 시 해골 전사의 첫 공격은 기절 n초간
            ManagerRoot.Event.onRecallActionEnd += ReadyStrongStunAttack;
        }
    }

    private void ReadyStrongStunAttack()
    {
        //모든 해골 전사 리스트 가져오기
        _strongStunSwordmans = ManagerRoot.Unit.GetAllAliveUndeadUnits().FindAll(unit => unit.UnitType == (int) UnitType.SwordMan);
    }

    public override void OnBattleStart()
    {
        
    }

    public override void OnBattleEnd()
    {
        
    }
    
    private void OnIDamageableTakeDamage(IDamageable damaged_, AttackInfo attackinfo_, int modifiedDamage_, ref int finalDamage_)
    {
        //언데드 유닛이면
        if (attackinfo_.attacker is Unit attackingUnit && attackingUnit.CurrentFaction == Faction.Undead)
        {
            //소드맨이면
            if (attackingUnit.UnitType == (int)UnitType.SwordMan)
            {
                //공격 횟수를 센다.
                if (_attackCountDict.ContainsKey(attackingUnit))
                {
                    int count = ++_attackCountDict[attackingUnit];
                    //공격 횟수가 스턴 공격 횟수에 도달하면 스턴을 건다.
                    if (count >= _data.NumOfAtkInCycle)
                    {
                        (damaged_ as Unit)?.TakeKnockDown(attackingUnit.transform, duration_: _data.StunDuration, power_: 0f);
                        _attackCountDict[attackingUnit] = 0;
                    }
                }
                else
                {
                    _attackCountDict.Add(attackingUnit, 1);
                }

                if (SkillLevel >= 2)
                {
                    if (_strongStunSwordmans.Contains(attackingUnit)) //복귀 한 뒤 공격한 적 없는 소드맨이면
                    {
                        (damaged_ as Unit)?.TakeKnockDown(attackingUnit.transform, duration_: _data.Lv2StunDuration, power_: 0f);
                        _strongStunSwordmans.Remove(attackingUnit);
                    }
                }
            }
        }
    }

    private void Init()
    {
        _data = LoadData<StunningAttackData>();
        Id = _data.Id;
        Name = _data.name;
    }
    
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onIDamageableTakeHit -= OnIDamageableTakeDamage;
    }
}
