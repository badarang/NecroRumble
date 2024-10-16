using LOONACIA.Unity.Managers;
using UnityEngine;

public class HiddenBlade : SkillBase
{
    HiddenBladeData _data;

    private void Init()
    {
        _data = LoadData<HiddenBladeData>();
        Id    = _data.Id;
        Name  = _data.name;
    }

    private void OnUnitSpawn(Unit unit_){
        if (SkillLevel == 0) return;
        if (unit_.UnitType != (int)UnitType.Assassin) return;
        if (unit_.CurrentFaction != Faction.Undead) return;

        ((AssassinUnit)unit_).SetHiddenBlade((SkillLevel == 1)?_data.additionalAttackCount1:_data.additionalAttackCount2);
    }
    
    private void OnUnitDeath(Unit deadUnit_, AttackInfo attackInfo_)
    {
        if (SkillLevel == 0) return;
        if (attackInfo_.attacker is not Unit) return;
        Unit attackerUnit_ = (Unit)attackInfo_.attacker;
        if (attackerUnit_.UnitType != (int)UnitType.Assassin) return;
        if (attackerUnit_.CurrentFaction != Faction.Undead) return;
        
        AssassinUnit assassinUnit_ = (AssassinUnit)attackerUnit_;

        assassinUnit_.canHiddenBlade = true;
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
            var undeads = ManagerRoot.Unit.GetAllAliveUndeadUnits();
            foreach(var undead in undeads){
                if (undead.UnitType != (int)UnitType.Assassin) continue;
                ((AssassinUnit)undead).SetHiddenBlade(_data.additionalAttackCount2);
            }
            StatModifier mod = new StatModifier(StatType.MaxHp, "HiddenBlade", (-1) * _data.hpDownPercent2, StatModifierType.BaseAddition, true);
            ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.Assassin, mod, false);
            
            Debug.Log("HiddenBlade 스킬 레벨 2 업그레이드 완료");
        }
    }
    
    public override void OnSkillAttained()
    {
        ManagerRoot.Event.onUnitSpawn += OnUnitSpawn;
        ManagerRoot.Event.onUnitDeath += OnUnitDeath;
        Init();
        
        SkillLevel = 1;
        OnBattleStart();
        
        var undeads = ManagerRoot.Unit.GetAllAliveUndeadUnits();
        foreach(var undead in undeads){
            if (undead.UnitType != (int)UnitType.Assassin) continue;
            ((AssassinUnit)undead).SetHiddenBlade(_data.additionalAttackCount1);
        }
        
        StatModifier mod = new StatModifier(StatType.MaxHp, "HiddenBlade", (-1) * _data.hpDownPercent1, StatModifierType.BaseAddition, true);
        ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.Assassin, mod, false);
        
        Debug.Log("HiddenBlade 스킬 레벨 1 획득 완료");
    }
    
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onUnitSpawn -= OnUnitSpawn;
        ManagerRoot.Event.onUnitDeath -= OnUnitDeath;
    }
}
