using LOONACIA.Unity.Managers;
using UnityEngine;

public class ThirdHit : SkillBase
{
    ThirdHitData _data;

    private void Init()
    {
        _data = LoadData<ThirdHitData>();
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
            damagedUnit.ThirdHitCount++;
            Debug.Log($"ThirdHitCount: {damagedUnit.ThirdHitCount}");
            
            if (damagedUnit.ThirdHitCoroutine != null)
            {
                damagedUnit.StopCoroutine(damagedUnit.ThirdHitCoroutine);
            }
            damagedUnit.ThirdHitCoroutine = damagedUnit.StartCoroutine(damagedUnit.SetThirdHitCoroutine(3f));
            damagedUnit.ThirdHitDamageMultiplier = (SkillLevel == 1) ? _data.damagePercent1 : _data.damagePercent2;
            damagedUnit.ThirdHitDamageMultiplierElite = (SkillLevel == 1) ? _data.damagePercentElite1 : _data.damagePercentElite2;
            if (damagedUnit.TryGetComponent(out FeedbackController feedbackController_))
            {
                feedbackController_.ControlThirdHitEffect(damagedUnit.ThirdHitCount);
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
            StatModifier mod = new StatModifier(StatType.AttackDamage, "ThirdHit", (-1) * _data.damageDecreasePercent1, StatModifierType.Percentage, true);
            ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, mod, false);
            Debug.Log("ThirdHit 스킬 레벨 2 업그레이드 완료");
        }
    }
    
    public override void OnSkillAttained()
    {
        ManagerRoot.Event.onIDamageableTakeHit += OnUnitHit;
        Init();
        
        SkillLevel = 1;
        
        StatModifier mod = new StatModifier(StatType.AttackDamage, "ThirdHit", (-1) * _data.damageDecreasePercent2, StatModifierType.Percentage, true);
        ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, mod, false);
        
        Debug.Log("ThirdHit 스킬 레벨 1 획득 완료");
    }
    
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onIDamageableTakeHit -= OnUnitHit;
    }
}
