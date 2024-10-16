using LOONACIA.Unity.Managers;
using UnityEngine;

public class FearOfBlood : SkillBase
{
    FearOfBloodData _data;

    private void Init()
    {
        _data = LoadData<FearOfBloodData>();
        Id    = _data.Id;
        Name  = _data.name;
    }

    private void OnUnitDeath(Unit deadUnit_, AttackInfo attackInfo_)
    {
        if (SkillLevel == 0) return;
        if (attackInfo_.attacker is not Unit) return;
        Unit attackerUnit_ = (Unit)attackInfo_.attacker;
        if (attackerUnit_.UnitType != (int)UnitType.Assassin) return;
        if (attackerUnit_.CurrentFaction != Faction.Undead) return;
        
        
        var units = ManagerRoot.Unit.GetUnitsInCircle(deadUnit_.transform.position, _data.fearRange1, Faction.Human);
        foreach (var unit in units)
        {
            if (unit.IsDead) continue;
            AttackInfo attackInfo = new AttackInfo(attackerUnit_, 0);
            unit.TakeFear(1f, attackerUnit_.transform.position, _data.fearDuration1, attackInfo);
        }
    }

    private void OnIDamageableTakeHit(IDamageable damaged_, AttackInfo attackInfo_, int modifiedDamage_, ref int finalDamage_)
    {
        if (attackInfo_.attacker is not Unit) return;
        if (SkillLevel == 0) return;
        
        Unit attackerUnit_ = (Unit)attackInfo_.attacker;
        if (attackerUnit_.IsDead) return;
        if (attackInfo_.attacker == null) return;
        if (attackerUnit_.UnitType != (int)UnitType.Assassin) return;
        if (attackerUnit_.CurrentFaction != Faction.Undead) return;
            
        if (SkillLevel == 2)
        {
            if (damaged_ is Unit damagedUnit_)
            {
                if (damagedUnit_.CurrentFaction == Faction.Undead) return;
                if (damagedUnit_.IsDead) return;
                if (damagedUnit_.IsFearState)
                {
                    finalDamage_ = Mathf.RoundToInt((modifiedDamage_ * _data.fearStateDamageMultiplier2));
                }
            }
        }
    }
    
    public override void OnBattleStart()
    {
        
    }
    public override void OnBattleEnd()
    {
        
    }
    public override void OnSkillUpgrade()
    {
        if (SkillLevel == 2)
        {
            StatModifier mod = new StatModifier(StatType.MaxHp, "FearOfBlood", (-1) * _data.hpDownPercent2, StatModifierType.BaseAddition, true);
            ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.Assassin, mod, false);
            
            Debug.Log("FearOfBlood 스킬 레벨 2 업그레이드 완료");
        }
    }
    
    public override void OnSkillAttained()
    {
        ManagerRoot.Event.onUnitDeath += OnUnitDeath;
        ManagerRoot.Event.onIDamageableTakeHit += OnIDamageableTakeHit;
        Init();
        
        SkillLevel = 1;
        
        StatModifier mod = new StatModifier(StatType.MaxHp, "FearOfBlood", (-1) * _data.hpDownPercent1, StatModifierType.BaseAddition, true);
        ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.Assassin, mod, false);
        
        Debug.Log("FearOfBlood 스킬 레벨 1 획득 완료");
    }
    
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onUnitDeath -= OnUnitDeath;
        ManagerRoot.Event.onIDamageableTakeHit -= OnIDamageableTakeHit;
    }
}
