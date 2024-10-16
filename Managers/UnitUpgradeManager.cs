using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class UnitUpgradeManager
{
    private List<StatModifier> _swordManUpgrades = new(); //상시, 그리고 스폰시 적용되는 업그레이드들 리스트
    private List<StatModifier> _swordManRecallUpgrades = new(); //리콜시 적용되는 업그레이드들 리스트
    private List<StatModifier> _swordManDisplayUpgrades = new(); //유닛 강화 창에 표시될 스탯 보정치 리스트
    
    private List<StatModifier> _archerManUpgrades = new();
    private List<StatModifier> _archerManRecallUpgrades = new();
    private List<StatModifier> _archerManDisplayUpgrades = new();
    
    private List<StatModifier> _assassinUpgrades = new();
    private List<StatModifier> _assassinRecallUpgrades = new();
    private List<StatModifier> _assassinDisplayUpgrades = new();
    
    public void Clear()
    {
        _swordManUpgrades.Clear();
        _swordManRecallUpgrades.Clear();
        _swordManDisplayUpgrades.Clear();
        
        _archerManUpgrades.Clear();
        _archerManRecallUpgrades.Clear();
        _archerManDisplayUpgrades.Clear();
        
        _assassinUpgrades.Clear();
        _assassinRecallUpgrades.Clear();
        _assassinDisplayUpgrades.Clear();
    }

    public void Init()
    {
        ManagerRoot.Event.onUnitSpawn += OnUnitSpawn;
        ManagerRoot.Event.onRecallActionEnd += OnRecallActionEnd;
    }
    
    public void AddUnitUpgrade(UnitType unitType_, StatModifier statModifier_, bool isRecallUpgrade_)
    {
        var clonedModifier = new StatModifier(statModifier_.statType, statModifier_.modifierName, statModifier_.value,
            statModifier_.type, statModifier_.shouldBeDisplayed, statModifier_.duration);
        switch (unitType_)
        {
            case UnitType.SwordMan:
                if (isRecallUpgrade_) _swordManRecallUpgrades.Add(clonedModifier);
                else
                {
                    _swordManUpgrades.Add(clonedModifier);
                }
                break;
            case UnitType.ArcherMan:
                if (isRecallUpgrade_) _archerManRecallUpgrades.Add(clonedModifier);
                else _archerManUpgrades.Add(clonedModifier);
                break;
            case UnitType.Assassin:
                if (isRecallUpgrade_) _assassinRecallUpgrades.Add(clonedModifier);
                else _assassinUpgrades.Add(clonedModifier);
                break;
        }
        
        //상시 효과들만 현재 유닛들에게 적용
        if (!isRecallUpgrade_ && clonedModifier.duration > float.MaxValue)
        {
            var allUnits = ManagerRoot.Unit.GetAllAliveUndeadUnits();
            foreach (var unit in allUnits)
            {
                if (unit.PairUnitData.UnitInfo.UnitType == unitType_)
                {
                    ApplyUpgradeToSingleUnit(unit, clonedModifier, false);
                }
            }
            
            //디스플레이용 리스트에 추가
            TryAddModifierToDisplayList(unitType_, clonedModifier);
        }
    }

    public void OnUnitSpawn(Unit newUnit_)
    {
        if (newUnit_.CurrentFaction != Faction.Undead) return; //언데드에게만 적용
        
        List<StatModifier> upgrades = new();
        switch (newUnit_.PairUnitData.UnitInfo.UnitType)
        {
            case UnitType.SwordMan:
                upgrades = _swordManUpgrades;
                break;
            case UnitType.ArcherMan:
                upgrades = _archerManUpgrades;
                break;
            case UnitType.Assassin:
                upgrades = _assassinUpgrades;
                break;
        }
        
        foreach (var upgrade in upgrades)
        {
            ApplyUpgradeToSingleUnit(newUnit_, upgrade, false);
        }
    }
    
    public void OnRecallActionEnd()
    {
        List<StatModifier> upgrades = new();
        var allUnits = ManagerRoot.Unit.GetAllAliveUndeadUnits();

        foreach (var unit in allUnits)
        {
            switch (unit.PairUnitData.UnitInfo.UnitType)
            {
                case UnitType.SwordMan:
                    upgrades = _swordManRecallUpgrades;
                    break;
                case UnitType.ArcherMan:
                    upgrades = _archerManRecallUpgrades;
                    break;
                case UnitType.Assassin:
                    upgrades = _assassinRecallUpgrades;
                    break;
            }

            foreach (var upgrade in upgrades)
            {
                ApplyUpgradeToSingleUnit(unit, upgrade, false);
            }
        }
    }

    public void TweakSingleUnitModValueByName(Unit unit_, string name_, float value_)
    {
        var stat = unit_.instanceStats;
        if (stat.GetModifierByName(name_) is { } mod)
        {
            mod.value = value_;
        }
        else
        {
            Debug.LogWarning($"{unit_.name}에 {name_}이라는 이름의 StatModifier가 없음");
        }
    }

    public void TweakUnitUpgradeValueByName(UnitType unitType_, string name_, float value_)
    {
        List<StatModifier> allUnitUpgrades = new();
        switch (unitType_)
        {
            case UnitType.SwordMan:
                allUnitUpgrades.AddRange(_swordManUpgrades);
                allUnitUpgrades.AddRange(_swordManRecallUpgrades);
                break;
            case UnitType.ArcherMan:
                allUnitUpgrades.AddRange(_archerManUpgrades);
                allUnitUpgrades.AddRange(_archerManRecallUpgrades);
                break;
            case UnitType.Assassin:
                allUnitUpgrades.AddRange(_assassinUpgrades);
                allUnitUpgrades.AddRange(_assassinRecallUpgrades);
                break;
        }
        var findByName = allUnitUpgrades.Where(x => x.modifierName == name_).ToList();
        if (findByName.Count() > 1)
        {
            Debug.LogWarning($"{unitType_}에 {name_}이라는 이름의 StatModifier가 여러개 있음");
        }
        else if (findByName.Count() == 1)
        {
            findByName.First().value = value_;
        }
        else
        {
            Debug.LogWarning($"{unitType_}에 {name_}이라는 이름의 StatModifier가 없음");
        }
    }
    

    public void ApplyUpgradeToSingleUnit(Unit unit_, StatModifier mod_, bool usedFromOutside = true)
    {
        //스탯별 강화 적용
        switch (mod_.statType)
        {
            case StatType.MaxHp:
                unit_.instanceStats.AddMaxHpModifier(mod_);
                break;
            case StatType.MaxHatred:
                unit_.instanceStats.AddMaxHatredModifier(mod_);
                break;
            case StatType.MoveSpeed:
                unit_.instanceStats.AddMoveSpeedModifier(mod_);
                break;
            case StatType.AttackRange:
                unit_.instanceStats.AddAttackRangeModifier(mod_);
                break;
            case StatType.AttackPerSec:
                unit_.instanceStats.AddAttackPerSecModifier(mod_);
                break;
            case StatType.AttackDamage:
                unit_.instanceStats.AddAttackDamageModifier(mod_);
                break;
            case StatType.MaxAggroNum:
                unit_.instanceStats.AddMaxAggroNumModifier(mod_);
                break;
            case StatType.DamageReduce:
                unit_.instanceStats.AddDamageReduceModifier(mod_);
                break;
            case StatType.HealthDrain:
                unit_.instanceStats.AddHealthDrainModifier(mod_);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (usedFromOutside)
        {
            TryAddModifierToDisplayList(unit_, mod_);
        }
    }

    public float GetDisplayStat(UnitType unitType_, StatType statType_, out float modifiedBase_,
        out float percentageAddition_)
    {
        //유닛 타입의 디스플레이 보정치 리스트 가져오기
        var displayModifiers = unitType_ switch
        {
            UnitType.SwordMan => _swordManDisplayUpgrades,
            UnitType.ArcherMan => _archerManDisplayUpgrades,
            UnitType.Assassin => _assassinDisplayUpgrades,
            _ => throw new ArgumentOutOfRangeException(nameof(unitType_), unitType_, null)
        };
        
        //유닛의 데이터 가져오기
        var unitData = unitType_ switch
        {
            UnitType.SwordMan => ManagerRoot.Resource.Load<PairUnitData>($"Data/Unit/{nameof(SwordManUnit)}Data"),
            UnitType.ArcherMan => ManagerRoot.Resource.Load<PairUnitData>($"Data/Unit/{nameof(ArcherManUnit)}Data"),
            UnitType.Assassin => ManagerRoot.Resource.Load<PairUnitData>($"Data/Unit/{nameof(AssassinUnit)}Data"),
        };

        if (unitData == null) throw new NullReferenceException($"{unitType_}에 해당하는 UnitData가 없음");

        var baseStat = unitData.GetStats(Faction.Undead) as UndeadUnitStats;

        var targetStat = statType_ switch
        {
            StatType.AttackDamage => baseStat.BaseAttackDamage,
            StatType.AttackPerSec => baseStat.BaseAttackPerSec,
            StatType.AttackRange => baseStat.BaseAttackRange,
            StatType.DamageReduce => 0,
            StatType.HealthDrain => 0,
            StatType.MaxAggroNum => baseStat.BaseMaxAggroNum,
            StatType.MaxHp => baseStat.BaseMaxHp,
            StatType.MoveSpeed => baseStat.BaseMoveSpeed,
            _ => throw new ArgumentOutOfRangeException(nameof(statType_), statType_, null)
        };
        
        //보정치들 중 baseAddition만 합산
        modifiedBase_ = displayModifiers.Where(mod => mod.statType == statType_ && mod.type == StatModifierType.BaseAddition)
            .Sum(mod => mod.value) + targetStat;
        
        //보정치들 중 percentageAddition만 합산
        var percentage = displayModifiers.Where(mod => mod.statType == statType_ && mod.type == StatModifierType.Percentage)
            .Sum(mod => mod.value);
        percentageAddition_ = modifiedBase_ * (percentage / 100f);

        //FinalPercentage 전부 연산
        var final = (modifiedBase_ + percentageAddition_);
        displayModifiers.ForEach(mod =>
        {
            if (mod.statType == statType_ && mod.type == StatModifierType.FinalPercentage)
            {
                final *= (1 + mod.value / 100f);
            }
        });

        return final;
    }
    
    private void TryAddModifierToDisplayList(Unit unit_, StatModifier modifier_)
    {
        //유닛 언데드인지 확인
        if (unit_.CurrentFaction != Faction.Undead) return;
        //유닛 엘리트면 리턴
        if (unit_.IsElite) return;
        
        //이름으로 추가하기
        TryAddDisplayModifierByName(unit_.PairUnitData.UnitInfo.UnitType, modifier_);
    }

    private void TryAddDisplayModifierByName(UnitType unitType_, StatModifier modifier_)
    {
        var targetList = unitType_ switch
        {
            UnitType.SwordMan => _swordManDisplayUpgrades,
            UnitType.ArcherMan => _archerManDisplayUpgrades,
            UnitType.Assassin => _assassinDisplayUpgrades,
            _ => throw new ArgumentOutOfRangeException(nameof(unitType_), unitType_, null)
        };
        
        if (targetList.Select(mod => mod.modifierName).Contains(modifier_.modifierName)) return; //이미 같은 이름이 있으면 추가하지 않음.
        
        TryAddModifierToDisplayList(unitType_, modifier_);
    }
    
    private void TryAddModifierToDisplayList(UnitType unitType_, StatModifier modifier_)
    {
        if (unitType_ is not (UnitType.SwordMan or UnitType.ArcherMan or UnitType.Assassin)) return; //기본 3종 유닛만
        if (modifier_.shouldBeDisplayed == false) return; //표시되어야 하는 스탯만
        if (modifier_.duration < float.MaxValue) return; //상시 효과만
        
        var targetList = unitType_ switch
        {
            UnitType.SwordMan => _swordManDisplayUpgrades,
            UnitType.ArcherMan => _archerManDisplayUpgrades,
            UnitType.Assassin => _assassinDisplayUpgrades,
            _ => throw new ArgumentOutOfRangeException(nameof(unitType_), unitType_, null)
        };
        if (targetList.Contains(modifier_)) return; //이미 추가되어있는 스탯이면 추가하지 않음
        
        targetList.Add(modifier_);
    }
}
