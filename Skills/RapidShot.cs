using LOONACIA.Unity.Managers;
using Unity.VisualScripting.FullSerializer.Internal.Converters;
using UnityEngine;

public class RapidShot : SkillBase
{
    RapidShotData _data;
    private Coroutine _rapidShotCoroutine;

    void Init()
    {
        _data = LoadData<RapidShotData>();
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
        
        archerManUnit_.RapidShotCount++;
        
        if (SkillLevel == 1)
        {
            if (archerManUnit_.RapidShotCount >= _data.attackTriggerCount1)
            {
                archerManUnit_.RapidShotCount = 0;
                
                if (archerManUnit_.instanceStats.GetModifierByName(nameof(RapidShot)+" main") is {} originalMod)
                {
                    originalMod.duration = _data.attackTriggerDuration1;
                }
                else
                {
                    StatModifier mod = new StatModifier(StatType.AttackPerSec, nameof(RapidShot)+" main", _data.attackSpeedUpPercent1, StatModifierType.Percentage, false, _data.attackTriggerDuration1);
                    archerManUnit_.instanceStats.AddAttackPerSecModifier(mod); 
                }
            }
        }
        else if (SkillLevel == 2)
        {
            if (archerManUnit_.RapidShotCount >= _data.attackTriggerCount2)
            {
                archerManUnit_.RapidShotCount = 0;
                if (archerManUnit_.instanceStats.GetModifierByName(nameof(RapidShot)+" main") is {} originalMod)
                {
                    originalMod.duration = _data.attackTriggerDuration2;
                }
                else
                {
                    StatModifier mod = new StatModifier(StatType.AttackPerSec, nameof(RapidShot)+" main", _data.attackSpeedUpPercent2, StatModifierType.Percentage, false, _data.attackTriggerDuration2);
                    archerManUnit_.instanceStats.AddAttackPerSecModifier(mod); 
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
            StatModifier mod = new StatModifier(StatType.AttackDamage, "RapidShot", (-1) * _data.damageDecreasePercent2, StatModifierType.Percentage, true);
            ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, mod, false);
            
            Debug.Log("RapidShot 스킬 레벨 2 업그레이드 완료");
        }
    }
    
    public override void OnSkillAttained()
    {
        ManagerRoot.Event.onUnitAttack += OnUnitAttack;
        Init();
        SkillLevel = 1;
        
        StatModifier mod = new StatModifier(StatType.AttackDamage, "RapidShot", (-1) * _data.damageDecreasePercent1, StatModifierType.Percentage, true);
        ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, mod, false);
        
        Debug.Log("RapidShot 스킬 레벨 1 획득 완료");
    }
    
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onUnitAttack -= OnUnitAttack;
    }
}
