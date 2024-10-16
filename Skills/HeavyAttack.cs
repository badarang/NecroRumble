using LOONACIA.Unity.Managers;
using Unity.VisualScripting.FullSerializer.Internal.Converters;
using UnityEngine;

public class HeavyAttack : SkillBase
{
    HeavyAttackData _data;
    
    void Init()
    {
        _data = LoadData<HeavyAttackData>();
        Id    = _data.Id;
        Name  = _data.name;
    }

    private void OnUnitAttack(Unit attackerUnit_, AttackInfo attackInfo_)
    {
        if (SkillLevel == 0) return;
        if (attackerUnit_.IsDead) return;
        if (attackInfo_.attacker == null) return;
        if (attackerUnit_.UnitType != (int)UnitType.ArcherMan) return;
        if (attackerUnit_.CurrentFaction != Faction.Undead) return;
        
        ArcherManUnit archerManUnit_ = (ArcherManUnit)attackerUnit_;
        
        if (SkillLevel == 2)
        {
            archerManUnit_.HeavyAttackSlowPercent = _data.attackSlowPercent2;
            archerManUnit_.HeavyAttackSlowDuration = _data.attackSlowDuration2;
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
            StatModifier rangeMod = new StatModifier(StatType.AttackRange, "HeavyAttackRange", _data.attackRangeIncreasePercent2, StatModifierType.Percentage, true);
            ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, rangeMod, false);
        
            StatModifier damageMod = new StatModifier(StatType.AttackDamage, "HeavyAttackDamage", _data.attackDamageIncreaseFlat2, StatModifierType.BaseAddition, true);
            ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, damageMod, false);
            
            Debug.Log("HeavyAttack 스킬 레벨 2 업그레이드 완료");
        }
    }
    
    public override void OnSkillAttained()
    {
        ManagerRoot.Event.onUnitAttack += OnUnitAttack;
        Init();
        SkillLevel = 1;
        
        StatModifier rangeMod = new StatModifier(StatType.AttackRange, "HeavyAttackRange", _data.attackRangeIncreasePercent1, StatModifierType.Percentage, true);
        ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, rangeMod, false);
        
        StatModifier damageMod = new StatModifier(StatType.AttackDamage, "HeavyAttackDamage", _data.attackDamageIncreaseFlat1, StatModifierType.BaseAddition, true);
        ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, damageMod, false);

        StatModifier finalDamageMod = new StatModifier(StatType.AttackDamage, "HeavyAttackFinalDamage", _data.attackFinalDamageIncreasePercent1, StatModifierType.FinalPercentage, true);
        ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, finalDamageMod, false);
        
        StatModifier attackSpeedMod = new StatModifier(StatType.AttackPerSec, "HeavyAttackAttackSpeed", (-1) * _data.attackSpeedDecreasePercent1, StatModifierType.FinalPercentage, true);
        ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, attackSpeedMod, false);

        
        Debug.Log("HeavyAttack 스킬 레벨 1 획득 완료");
    }
    
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onUnitAttack -= OnUnitAttack;
    }
}
