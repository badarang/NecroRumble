using LOONACIA.Unity.Managers;
using UnityEngine;

public class LapelOfNightmare : SkillBase
{
    LapelOfNightmareData _data;

    private void Init()
    {
        _data = LoadData<LapelOfNightmareData>();
        Id    = _data.Id;
        Name  = _data.name;
    }
    
    private void OnIDamageableTakeHit(IDamageable damaged_, AttackInfo attackInfo_, int modifiedDamage_, ref int finalDamage_)
    {
        if (attackInfo_.attacker is not Unit) return;
        if (damaged_ is not Unit damagedUnit_) return;
        if (SkillLevel == 0) return;
        
        Unit attackerUnit_ = (Unit)attackInfo_.attacker;
        if (attackerUnit_.IsDead) return;
        if (attackInfo_.attacker == null) return;
        if (damagedUnit_.UnitType != (int)UnitType.Assassin) return;
        if (damagedUnit_.CurrentFaction != Faction.Undead) return;
            
        if (SkillLevel > 0)
        {
            var randomFloat = Random.Range(0f, 1f);
            
            if (randomFloat <= _data.fearRatio1)
            {
                var fearAtkInfo = new AttackInfo(damagedUnit_, 0);
                attackerUnit_.TakeFear(1f, damagedUnit_.transform.position, _data.fearDuration1, fearAtkInfo);
            }
        }
    }

    private void OnUnitTakeFear(Unit fearedUnit_, AttackInfo attackInfo_)
    {
        if (SkillLevel == 0) return;
        if (attackInfo_?.attacker is not Unit) return;
        Unit attackerUnit_ = (Unit)attackInfo_.attacker;
        if (attackerUnit_.UnitType != (int)UnitType.Assassin) return;
        if (attackerUnit_.CurrentFaction != Faction.Undead) return;
        
        if (SkillLevel >= 2)
        {
            if (fearedUnit_.CurrentFaction == Faction.Human)
            {
                if (fearedUnit_.IsDead) return;
                if (attackerUnit_ == null) return;
                attackerUnit_.TakeHeal(Mathf.RoundToInt(_data.hpRecoveryValue2), null);
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
            StatModifier mod = new StatModifier(StatType.AttackPerSec, "LapelOfNightmare2", (-1) * _data.attackSpeedDownValue2, StatModifierType.Percentage, true);
            ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.Assassin, mod, false);
            
            Debug.Log("LapelOfNightmare 스킬 레벨 2 업그레이드 완료");
        }
    }
    
    public override void OnSkillAttained()
    {
        ManagerRoot.Event.onIDamageableTakeHit += OnIDamageableTakeHit;
        ManagerRoot.Event.onUnitTakeFear += OnUnitTakeFear;
        Init();
        
        SkillLevel = 1;
        
        StatModifier mod = new StatModifier(StatType.AttackPerSec, "LapelOfNightmare1", (-1) * _data.attackSpeedDownValue1, StatModifierType.Percentage, true);
        ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.Assassin, mod, false);
        
        Debug.Log("LapelOfNightmare 스킬 레벨 1 획득 완료");
    }
    
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        // ManagerRoot.Event.onUnitDeath -= OnUnitDeath;
        // ManagerRoot.Event.onIDamageableTakeHit -= OnIDamageableTakeHit;
    }
}
