using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class BloodThirst : SkillBase
{
    private BloodThirstData _data;
    
    public override void OnSkillAttained()
    {
        Init();

        StatModifier mod = new StatModifier(StatType.HealthDrain, nameof(BloodThirst), _data.MinDrainPercent, StatModifierType.BaseAddition, false);
        ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.SwordMan, mod, false);
        
        ManagerRoot.Event.onIDamageableTakeHit += UpdateHealthDrainPercent;
    }

    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onIDamageableTakeHit -= UpdateHealthDrainPercent;
    }


    public override void OnSkillUpgrade()
    {
        if (SkillLevel == 2)
        {
            //해골 전사가 받는 모든 회복 효과 +n%
            ManagerRoot.Event.onUnitSpawn += AddHealingMultiplierToUnit; //앞으로 태어날 언데드 소드맨에게 효과 부여
            //현재 존재하는 언데드 소드맨에게 효과 부여
            foreach (Unit unit in ManagerRoot.Unit.GetAllAliveUndeadUnits())
            {
                AddHealingMultiplierToUnit(unit);
            }
            
            Debug.Log($"{nameof(BloodThirst)} leveled up to 2");
        }
    }

    private void AddHealingMultiplierToUnit(Unit newUnit_)
    {
        if (newUnit_.UnitType == (int) UnitType.SwordMan && newUnit_.CurrentFaction == Faction.Undead)
        {
            newUnit_.healingMultiplier += _data.Lv2HealingMultiplier - 1;
        }
    }

    public override void OnBattleStart()
    {
        
    }

    public override void OnBattleEnd()
    {
        
    }

    private void Init()
    {
        _data = LoadData<BloodThirstData>();
        Id = _data.Id;
        Name = _data.name;
    }
    
    private void UpdateHealthDrainPercent(IDamageable damaged_, AttackInfo attackinfo_, int modifiedDamage_, ref int finaldamage_)
    {
        //피격자가 언데드 소드맨이면
        if (damaged_ is Unit { CurrentFaction: Faction.Undead, UnitType: (int) UnitType.SwordMan } undeadSwordMan)
        {
            int healthThreshold = (int) (undeadSwordMan.instanceStats.FinalMaxHp * (_data.MaxDrainHealthThreshold / 100));
            float hpRatio = (float) (undeadSwordMan.CurrentHp - healthThreshold) / (undeadSwordMan.instanceStats.FinalMaxHp - healthThreshold);
            hpRatio = Mathf.Clamp01(hpRatio);
            float fDrain = Mathf.Lerp(_data.MaxDrainPercent,_data.MinDrainPercent, hpRatio);
            int iDrain = (int) fDrain;

            undeadSwordMan.instanceStats.GetModifierByName(nameof(BloodThirst)).value = iDrain;
        }
    }
}
