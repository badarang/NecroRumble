using System.Collections.Generic;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using UnityEditor;
using UnityEngine;

public class GiantSkeleton : SkillBase
{
    GiantSkeletonData _data;
    List<SwordManUnit> giantUnitList;
    List<Unit> damageReduceUnit;

    StatModifier damageReduceMod;

    private void Init()
    {
        _data = LoadData<GiantSkeletonData>();
        Id    = _data.Id;
        Name  = _data.name;
        giantUnitList = new List<SwordManUnit>();
        damageReduceUnit = new List<Unit>();
        
        damageReduceMod = new StatModifier(StatType.DamageReduce, "GiantSkeletonDamageReduce", _data.damageReducePercent, StatModifierType.BaseAddition, false);
    }

    private void OnUnitSpawn(Unit unit_){
        if (SkillLevel == 0) return;
        if (unit_.CurrentFaction != Faction.Undead) return;
        if (unit_.UnitType != (int)UnitType.SwordMan) return;
        SwordManUnit swordManUnit = (SwordManUnit)unit_;

        if(Random.Range(0, 100) <= _data.giantProbability){
            StatModifier mod = new StatModifier(StatType.MaxHp, "GiantSkeleton", _data.hpUpMultiflier, StatModifierType.Percentage, false);
            ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(unit_, mod);
            unit_.gameObject.transform.localScale *= _data.giantScaleMultiflier;

            swordManUnit.isGiantUnit = true;
            giantUnitList.Add(swordManUnit);

            if(SkillLevel >= 2){
                unit_._rangeCircle2.SetActive(true);
                unit_._rangeCircle2.transform.localScale *= _data.damageReduceRadius;
            }
        }
    }
    private void Update() {
        
        if(SkillLevel >= 2){
            
            List<Unit> prevDamageReduceUnit = new List<Unit>(damageReduceUnit);
            damageReduceUnit.Clear();
            // 자이언트들 순환
            for(int i = giantUnitList.Count-1; i >= 0; --i){
                SwordManUnit giant = giantUnitList[i];
                // Remove dead giant unit
                if(giant == null || giant.IsDead) {
                    giantUnitList.RemoveAt(i);
                    continue;
                }
                if(giant.ActiveSelf == false) continue;
                // Find units around giant
                var detectUndeads_ = Unit.GetUnitListEcllipse(giant.transform.position, _data.damageReduceRadius+0.3f, _data.damageReduceRadius/2+0.3f, Layers.UndeadUnit.ToMask());
                foreach(var undead in detectUndeads_){
                    if(undead == null) continue;
                    if(undead.TryGetComponent(out Unit unit)){
                        if(unit.IsDead) continue;
                        // 이미 자이언트에게서 데미지감소 효과 받았으면 넘어감
                        if(!damageReduceUnit.Contains(unit)){
                            damageReduceUnit.Add(unit);
                        }
                    }
                }
            }
            // 효과 적용
            foreach(Unit unit in damageReduceUnit){
                prevDamageReduceUnit.Remove(unit);
                unit.Feedback.ApplyGiantSkeletonShieldEffect(true);
                // unit.Feedback.ChangeUnitColor(new Color(1f,1f,0.7f));
                StatModifier mod = unit.instanceStats.GetModifierByName("GiantSkeletonDamageReduce");
                if(mod == null)
                    ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(unit, damageReduceMod);
                else
                    mod.value = _data.damageReducePercent;
                
            }            
            foreach(Unit unit in prevDamageReduceUnit){
                if(unit == null) continue;
                // 효과 제거
                unit.Feedback.ApplyGiantSkeletonShieldEffect(false);
                // unit.Feedback.ChangeUnitColor(Color.white);
                unit.instanceStats.GetModifierByName("GiantSkeletonDamageReduce").value = 0;
            }
        }
    }
    
    public override void OnSkillUpgrade()
    {
        if (SkillLevel == 2)
        {
            for(int i = giantUnitList.Count-1; i >= 0; --i){
                SwordManUnit giant = giantUnitList[i];
                // Remove dead giant unit
                if(giant == null || giant.IsDead) {
                    giantUnitList.RemoveAt(i);
                    continue;
                }
                giant._rangeCircle2.SetActive(true);
                giant._rangeCircle2.transform.localScale *= _data.damageReduceRadius;
            }
            Debug.Log("GiantSkeleton 스킬 레벨 2 업그레이드 완료");
        }
    }
    
    public override void OnSkillAttained()
    {
        ManagerRoot.Event.onUnitSpawn += OnUnitSpawn;
        Init();
        
        SkillLevel = 1;
        OnBattleStart();
        
        Debug.Log("GiantSkeleton 스킬 레벨 1 획득 완료");
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
}
