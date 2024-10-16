using System;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class SoulWhip : SkillBase
{
    SoulWhipData _data;

    void Start()
    {
        _data = LoadData<SoulWhipData>();
        Id    = _data.Id;
        Name  = _data.name;
    }

    private void OnUnitSoulWhip(Unit unit, AttackInfo attackInfo_)
    {
        if (unit.TryGetComponent(out IDamageable damageable))
        {
            attackInfo_.damage = _data.undeadDealtDamage;
            damageable.TakeDamage(attackInfo_, new Color32(150, 100, 255, 255), Unit.SPECIALMODE.NONE);
            StatModifier mod = new StatModifier(StatType.AttackPerSec, "SoulWhip", _data.attackSpeedIncreaseRatio, StatModifierType.Percentage, false, _data.attackSpeedIncreaseDuration);
            ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(unit, mod);
            
            unit.Feedback.StartAttackSpeedEffectCoroutine(isUp: true, _data.attackSpeedIncreaseDuration);
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
    }
    public override void OnSkillAttained()
    {
        ManagerRoot.Event.onUnitSoulWhip += OnUnitSoulWhip;
    }
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onUnitSoulWhip -= OnUnitSoulWhip;
    }
}
