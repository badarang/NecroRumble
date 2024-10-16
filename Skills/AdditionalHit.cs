using LOONACIA.Unity.Managers;
using Unity.VisualScripting.FullSerializer.Internal.Converters;
using UnityEngine;

public class AdditionalHit : SkillBase
{
    AdditionalHitData _data;

    void Init()
    {
        _data = LoadData<AdditionalHitData>();
        Id    = _data.Id;
        Name  = _data.name;
    }

    private void OnUnitHit(IDamageable damaged_, AttackInfo attackInfo_, int modifiedDamage_, ref int finalDamage_)
    {
        if (SkillLevel == 0) return;
        if (attackInfo_.attacker is not Unit) return;
        if (((Unit)attackInfo_.attacker).UnitType != (int)UnitType.ArcherMan) return;
        if (damaged_ == null) return;
        if (damaged_ is Unit)
        {
            Unit damagedUnit = (Unit)damaged_;
            if (damagedUnit.IsDead) return;
            if (damagedUnit.CurrentFaction == Faction.Undead) return;
            
            var dmg = (SkillLevel == 1) ? _data.additionalDamagePerAttack1 : _data.additionalDamagePerAttack2;
            AttackInfo newAttackInfo = new AttackInfo(attackInfo_.attacker, dmg);
            
            damagedUnit.TakeDamage(newAttackInfo, new Color32(202, 209, 0, 181), Unit.SPECIALMODE.NOEVENT);
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
            StatModifier mod = new StatModifier(StatType.AttackDamage, "MultiShotDamage", (-1) * _data.damageDecreasePercent2, StatModifierType.Percentage, true);
            ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, mod, false);
            
            StatModifier mod2 = new StatModifier(StatType.AttackPerSec, "MultiShotAttackSpd", _data.attackSpeedPercent2, StatModifierType.Percentage, true);
            ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, mod2, false);
            
            Debug.Log("AdditionalHit 스킬 레벨 2 업그레이드 완료");
        }
    }
    
    public override void OnSkillAttained()
    {
        ManagerRoot.Event.onIDamageableTakeHit += OnUnitHit;
        Init();
        SkillLevel = 1;
        StatModifier mod = new StatModifier(StatType.AttackDamage, "MultiShotDamage", (-1) * _data.damageDecreasePercent1, StatModifierType.Percentage, true);
        ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, mod, false);
        
        StatModifier mod2 = new StatModifier(StatType.AttackPerSec, "MultiShotAttackSpd", _data.attackSpeedPercent1, StatModifierType.Percentage, true);
        ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, mod2, false);
        
        Debug.Log("AdditionalHit 스킬 레벨 1 획득 완료");
    }
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onIDamageableTakeHit -= OnUnitHit;
    }
}
