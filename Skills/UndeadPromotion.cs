using System.Collections;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class UndeadPromotion : SkillBase
{
    UndeadPromotionData _data;

    private void Init()
    {
        _data = LoadData<UndeadPromotionData>();
        Id    = _data.Id;
        Name  = _data.name;
    }

    private void OnUnitSpawn(Unit unit_){
        if (SkillLevel == 0) return;
        if (unit_.CurrentFaction != Faction.Undead) return;

        StartCoroutine(PromotionRoutine(unit_));
    }

    IEnumerator PromotionRoutine(Unit unit_){
        float timer = 0;
        int count = 0;
        while(true){
            timer += Time.deltaTime;
            if(timer >= _data.promotionDeltaTimeSecond){
                timer = 0;
                count++;
                if(unit_ == null) break;

                StatModifier hpMod, attackMod;
                
                if (unit_.IsElite)
                {
                    hpMod = new StatModifier(StatType.MaxHp, "UndeadPromotion", _data.eliteStatIncreaseValue, StatModifierType.Percentage, false);
                    attackMod = new StatModifier(StatType.AttackDamage, "UndeadPromotion", _data.eliteStatIncreaseValue, StatModifierType.Percentage, false);
                }
                else
                {
                    hpMod = new StatModifier(StatType.MaxHp, "UndeadPromotion", _data.statIncreaseValue, StatModifierType.Percentage, false);
                    attackMod = new StatModifier(StatType.AttackDamage, "UndeadPromotion", _data.statIncreaseValue, StatModifierType.Percentage, false);
                }
                
                //언데드 승급 이펙트
                unit_.Feedback.SetCrownEffect(true); 
                ManagerRoot.Sound.PlaySfx("Powerup upgrade 30", .3f);
                GameObject promotionEffect = Resources.Load<GameObject>("Prefabs/Effects/VFXPromotionEffect");
                GameObject effect = Instantiate(promotionEffect, unit_.transform.position + new Vector3(0f, 0.5f, 0f), Quaternion.identity);
                effect.transform.SetParent(unit_.transform);
                Destroy(effect, 2f);
                
                
                ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(unit_, hpMod);
                ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(unit_, attackMod);

                if(count == _data.maxStackCount) break;
            }
            yield return null;
        }
    }

    
    public override void OnSkillAttained()
    {
        ManagerRoot.Event.onUnitSpawn += OnUnitSpawn;
        Init();
        
        SkillLevel = 1;
        OnBattleStart();

        var undeads = ManagerRoot.Unit.GetAllAliveUndeadUnits();
        foreach(var undead in undeads){
            StartCoroutine(PromotionRoutine(undead));
        }
        
        Debug.Log("UndeadPromotion 주물 획득 완료");
    }
    
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onUnitSpawn -= OnUnitSpawn;
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
}
