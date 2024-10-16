using LOONACIA.Unity.Managers;
using UnityEngine;

public class PoisonArrow : SkillBase
{
    PoisonArrowData _data;

    void Start()
    {
        _data = LoadData<PoisonArrowData>();
        Id    = _data.Id;
        Name  = _data.name;
        
        //StatModifier mod = new StatModifier(StatType.AttackDamage, 1)
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
            var dmg = (SkillLevel == 1) ? _data.damagePercent1 : _data.damagePercent2;
            var duration = (SkillLevel == 1) ? _data.damageDuration1 : _data.damageDuration2;
            damagedUnit.TakePoison(duration, dmg);
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
            Debug.Log("PoisonArrow 스킬 레벨 2 업그레이드 완료");
        }
    }
    
    public override void OnSkillAttained()
    {
        ManagerRoot.Event.onIDamageableTakeHit += OnUnitHit;
        SkillLevel = 1;
        Debug.Log("PoisonArrow 스킬 레벨 1 획득 완료");
    }
    
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onIDamageableTakeHit -= OnUnitHit;
    }
}
