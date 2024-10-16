using LOONACIA.Unity.Managers;
using UnityEngine;

public class FuryHeart : SkillBase
{
    FuryHeartData _data;

    private void Init()
    {
        _data = LoadData<FuryHeartData>();
        Id    = _data.Id;
        Name  = _data.name;
    }

    public override void OnSkillAttained()
    {
        Init();

        StatModifier mod = new StatModifier(StatType.AttackPerSec, nameof(FuryHeart), _data.MinVelPercent, StatModifierType.Percentage, false);
        ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.SwordMan, mod, false);
        
        ManagerRoot.Event.onIDamageableTakeHit += UpdateHealthVelPercent;
    }


    private void OnUnitSpawn(Unit unit_){
        if (SkillLevel == 0) return;
        if (unit_.CurrentFaction != Faction.Undead) return;
        if (unit_.UnitType != (int)UnitType.SwordMan) return;
        SwordManUnit swordManUnit = (SwordManUnit)unit_;

        if(SkillLevel >= 2){
            swordManUnit.isHaveFuryHeart = true;
        }
    }
    public override void OnSkillUpgrade()
    {
        if (SkillLevel == 2)
        {
            Debug.Log("FuryHeart Level 2 업그레이드");
        }
    }

    
    private void UpdateHealthVelPercent(IDamageable damaged_, AttackInfo attackinfo_, int modifiedDamage_, ref int finaldamage)
    {
        //피격자가 언데드 소드맨이면
        if (damaged_ is Unit { CurrentFaction: Faction.Undead, UnitType: (int) UnitType.SwordMan } undeadSwordMan)
        {
            int healthThreshold = (int) (undeadSwordMan.instanceStats.FinalMaxHp * (_data.MaxHealthThreshold / 100));
            float hpRatio = (float) (undeadSwordMan.CurrentHp - healthThreshold) / (undeadSwordMan.instanceStats.FinalMaxHp - healthThreshold);
            hpRatio = Mathf.Clamp01(hpRatio);
            float fVel = Mathf.Lerp(_data.MaxVelPercent,_data.MinVelPercent, hpRatio);
            int iVel = (int) fVel;

            undeadSwordMan.instanceStats.GetModifierByName(nameof(FuryHeart)).value = iVel;

            if(SkillLevel >= 2){
                if(((SwordManUnit)undeadSwordMan).isHaveFuryHeart && undeadSwordMan.CurrentHp <= modifiedDamage_){
                    ((SwordManUnit)undeadSwordMan).isHaveFuryHeart = false;
                    finaldamage = undeadSwordMan .CurrentHp - 1;
                    StartCoroutine(undeadSwordMan.InvincibleRoutine(_data.invinDuration));
                }
            }
        }
    }
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onIDamageableTakeHit -= UpdateHealthVelPercent;
    }
    public override void OnBattleStart()
    {
        
    }

    public override void OnBattleEnd()
    {
        
    }
}
