using LOONACIA.Unity.Managers;
using Unity.VisualScripting.FullSerializer.Internal.Converters;
using UnityEngine;

public class BlackArrow : SkillBase
{
    BlackArrowData _data;
    
    void Init()
    {
        _data = LoadData<BlackArrowData>();
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
        archerManUnit_.BlackArrowLevel = SkillLevel;
        archerManUnit_.BlackArrowShotDelay = (SkillLevel == 1) ? _data.damageDelay1 : _data.damageDelay2;
        archerManUnit_.BlackArrowDamageMultiplier = (SkillLevel == 1) ? _data.damageMultiplier1 : _data.damageMultiplier2;
        
        // if (SkillLevel == 1)
        // {
        //     archerManUnit_.SetSecondAttackTarget();
        //     if (archerManUnit_.secondAttackTarget != null)
        //     {
        //         archerManUnit_.FireExtraProjectile(archerManUnit_.secondAttackTarget);
        //     }
        // }
        // else if (SkillLevel == 2)
        // {
        //     archerManUnit_.SetSecondAttackTarget();
        //     if (archerManUnit_.secondAttackTarget != null)
        //     {
        //         archerManUnit_.FireExtraProjectile(archerManUnit_.secondAttackTarget);
        //     }
        //     archerManUnit_.SetThirdAttackTarget();
        //     if (archerManUnit_.thirdAttackTarget != null)
        //     {
        //         archerManUnit_.FireExtraProjectile(archerManUnit_.thirdAttackTarget);
        //     }
        // }
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
            StatModifier mod = new StatModifier(StatType.AttackPerSec, "BlackArrow", (-1) * _data.attackSpeedDecreasePercent2, StatModifierType.Percentage, true);
            ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, mod, false);
            Debug.Log("BlackArrow 스킬 레벨 2 업그레이드 완료");
        }
    }
    
    public override void OnSkillAttained()
    {
        ManagerRoot.Event.onUnitAttack += OnUnitAttack;
        Init();
        SkillLevel = 1;
        
        StatModifier mod = new StatModifier(StatType.AttackPerSec, "BlackArrow", (-1) * _data.attackSpeedDecreasePercent1, StatModifierType.Percentage, true);
        ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, mod, false);
        
        Debug.Log("BlackArrow 스킬 레벨 1 획득 완료");
    }
    
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onUnitAttack -= OnUnitAttack;
    }
}
