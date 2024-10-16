using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using Sirenix.Serialization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SkillManager
{
    private List<SkillData> _allSkillData = new ();
    private List<SkillBase> _playerSkills = new ();
    private Dictionary<uint, Type> _skillCache = new();

    private List<NormalUnitUpgradeData> _swordManNormalUpgradeDataList = new();
    private List<NormalUnitUpgradeData> _archerManNormalUpgradeDataList = new();
    private List<NormalUnitUpgradeData> _assassinNormalUpgradeDataList = new();

    public List<SkillData> AllSkillData => _allSkillData;
    public List<SkillBase> PlayerSkill => _playerSkills;
    public List<NormalUnitUpgradeData> SwordManNormalUpgradeDataList => _swordManNormalUpgradeDataList;
    public List<NormalUnitUpgradeData> ArcherManNormalUpgradeDataList => _archerManNormalUpgradeDataList;
    public List<NormalUnitUpgradeData> AssassinNormalUpgradeDataList => _assassinNormalUpgradeDataList;

    public void Init()
    {
        PlayerSkill.Clear();
        _swordManNormalUpgradeDataList.Clear();
        _archerManNormalUpgradeDataList.Clear();
        _assassinNormalUpgradeDataList.Clear();

        GetAllSkillTypesAndData();
        
        #if UNITY_EDITOR
        //AddOrUpgradeSkill(0);
        #endif
    }

    // public List<SkillData> GetPotentialSkillRewards(uint choiceSize_)
    // {
    //     var tmp_Pool = new List<SkillData>(_allSkillData);
    //     var tmp_Rewards = new List<SkillData>();
    //     
    //     for (int i = 0; i < choiceSize_; i++)
    //     {
    //         if (tmp_Pool.Count == 0) return tmp_Rewards; //더 이상 뽑을 스킬이 없으면 지금까지 뽑은 것만 반환
    //         
    //         var random = UnityEngine.Random.Range(0, tmp_Pool.Count);
    //         tmp_Rewards.Add(tmp_Pool[random]);
    //         tmp_Pool.RemoveAt(random);
    //     }
    //
    //     return tmp_Rewards;
    // }

    public void OnBattleStart()
    {
        foreach (var skill in PlayerSkill)
        {
            skill.OnBattleStart();
        }
    }
    
    public void OnBattleEnd()
    {
        foreach (var skill in PlayerSkill)
        {
            skill.OnBattleEnd();
        }
    }
    
    public bool TryFindSkillTypeById(uint id_, out Type skillType_)
    {
        return _skillCache.TryGetValue(id_, out skillType_);
    }
    
    public void AddOrUpgradeSkill(uint id_, UnitType upgradingUnit_ = UnitType.None)
    {
        if( _skillCache.TryGetValue(id_, out var skillType_)) //스킬 베이스를 상속받는 스크립트중 id_와 일치하는 것이 있으면
        {
            AddOrUpgradeSkill(skillType_); //스킬 추가 (주물, 유닛 전용 강화)
        }
        else if (AllSkillData.Find(x => x.Id == id_) is NormalUnitUpgradeData nuuData)
        {
            //일반 유닛 강화 (공용)
            if (upgradingUnit_ == UnitType.None)
            {
                Debug.LogWarning($"SkillManager | AddOrUpgradeSkill: {id_} is NormalUnitUpgradeData but upgradingUnit_ is None.");
                return;
            }

            //유닛별 공용 업그레이드 리스트에 추가
            AddNormalUpgradeDataToList(upgradingUnit_, nuuData);

            //실제 스탯에 업그레이드 적용
            ApplyNormalUnitUpgrades(upgradingUnit_, nuuData);
        }
        else
        {
            Debug.LogWarning($"SkillManager | AddOrUpgradeSkill: {id_} is not found.");
        }
    }
    
    private void AddOrUpgradeSkill(Type skillType_)
    {
        Debug.Log($"SkillManager | AddOrUpgradeSkill | {skillType_.Name}");
        if (AlreadyHaveSkill(skillType_, out var skillInstance)) // 이미 가지고 있는 스킬이면
        {
            skillInstance.UpgradeSkill();
        }
        else // 가지고 있지 않은 스킬이면
        {
            //스킬 EmptyObject에 담아서 생성
            GameObject newGO = new GameObject(skillType_.Name);
            newGO.transform.SetParent(GameObject.Find("@SkillObjects").transform);
            newGO.transform.localPosition = Vector3.zero;
            var newSkillInstance = newGO.AddComponent(skillType_) as SkillBase;
            
            //생성한 스킬 인스턴스 리스트에 추가
            PlayerSkill.Add(newSkillInstance);

            // 스킬 처음 얻을때 이벤트 실행
            newSkillInstance.OnSkillAttained();
        }
    }
    
    private void GetAllSkillTypesAndData()
    {
        _allSkillData.Clear();
        _skillCache.Clear();
        
        //Get All SkillTypes
        var skillTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(type => typeof(SkillBase).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract);

        foreach (var type in skillTypes)
        {
            var data = ManagerRoot.Resource.Load<SkillData>($"Data/Skill/{type.Name}Data");
            if (data == null)
            {
                Debug.LogWarning($"SkillManager | {type.Name}Data is not found.");
                continue;
            }
            _skillCache.TryAdd(data.Id, type);
            _allSkillData.Add(data);
        }
        
        //모든 Normal Unit Upgrade Data 가져오기
        var normalUnitUpgrades = Resources.LoadAll<NormalUnitUpgradeData>("Data/Skill/NormalUnitUpgrades");
        _allSkillData.AddRange(normalUnitUpgrades);
    }

    private bool AlreadyHaveSkill(Type skillType_, out SkillBase skillInstance)
    {
        foreach (var skill in PlayerSkill)
        {
            if (skill.GetType() == skillType_)
            {
                skillInstance = skill;
                return true;
            }
        }

        skillInstance = null;
        return false;
    }

    private void ApplyNormalUnitUpgrades(UnitType unitType_, NormalUnitUpgradeData data_)
    {
        //데이터 내 모든 modifier UnitUpgradeManager를 통해 적용
        foreach (var modifier in data_.StatModifiers)
        {
            ManagerRoot.UnitUpgrade.AddUnitUpgrade(unitType_, modifier, data_.IsRecallUpgrade);
        }
    }

    /// <summary>
    /// 플레이어 스킬로 등록된 스킬들 중 유닛 타입에 해당하는 스킬들의 데이터를 반환. UnitType.None을 입력하면 주물을 반환한다.
    /// </summary>
    /// <param name="unitType_">UnitType.None일 경우 주물을 반환한다. 기본 유닛 3종을 제외한 다른 유닛 입력시 null 반환</param>
    /// <returns></returns>
    public List<SkillData> GetSkillListByType(UnitType unitType_)
    {
        //스킬 인덱스의 앞자리 숫자가 몇이 되어야 하는지 판별
        int unitIndex = -1;
        switch (unitType_)
        {
            case UnitType.None:
                unitIndex = 0;
                break;
            case UnitType.SwordMan:
                unitIndex = 2;
                break;
            case UnitType.ArcherMan:
                unitIndex = 3;
                break;
            case UnitType.Assassin:
                unitIndex = 4;
                break;
        }

        //기본 3종 유닛이나 None 이외의 유닛 타입 입력 시 null 반환.
        if (unitIndex == -1) return null;

        //조건에 맞는 스킬들의 Id 리스트 저장.
        var ids = PlayerSkill
            .Where(skill => Mathf.FloorToInt(skill.Id / 100f) == unitIndex)
            .Where(skill => skill.Id >= 10) //기본 네크로맨서 행동 제외.
            .Select(skill => skill.Id)
            .ToList();

        //Id에 맞는 데이터들 가져오기
        List<SkillData> dataList = _allSkillData
            .Where(data => ids.Contains(data.Id))
            .Where(data => data.SkillGrade > 0)
            .ToList();

        return dataList;
    }
    
    public List<SkillDataAttribute> GetAllUpgradesByUnitType(UnitType unitType_)
    {
        List<SkillDataAttribute> result = new();
        
        //주물 or 전용강화 ID 리스트 가져오기
        var skillIds = GetSkillListByType(unitType_)
            .Select(x => x.Id);
        
        //주물 or 전용강화 SkillBase 가져오기
        var skillBases = PlayerSkill
            .Where(skill => skillIds.Contains(skill.Id))
            .ToList();
        
        //주물 or 전용강화 스킬 베이스의 레벨에 따라 데이터 어트리뷰트 가져오기
        foreach (var skill in skillBases)
        {
            //스킬의 데이터 가져오기.
            var data = _allSkillData
                .Where(data => data.SkillGrade > 0)
                .FirstOrDefault(data => data.Id == skill.Id);

            //데이터 널 체크. 걸리는 경우 없어야함.
            if (data == null)
            {
                Debug.LogWarning($"SkillManager | GetAllUpgradesByUnitType: Data of ID {skill.Id} is not found.");
                continue;
            }
            
            //레벨 아래로 모든 스킬 어트리뷰트 가져오기
            for (int i = 1; i < skill.SkillLevel + 1; i++)
            {
                var attribute = data.GetCurSkillData(i);
                if (attribute != null)
                {
                    result.Add(attribute);
                }
            }
        }
        
        //공용 강화 데이터 어트리뷰트 가져오기
        var normalUpgradeAttributes = unitType_ switch
        {
            UnitType.SwordMan => _swordManNormalUpgradeDataList.Select(x => x.GetCurSkillData(1)),
            UnitType.ArcherMan => _archerManNormalUpgradeDataList.Select(x => x.GetCurSkillData(1)),
            UnitType.Assassin => _assassinNormalUpgradeDataList.Select(x => x.GetCurSkillData(1)),
            _ => null
        };
        
        if (normalUpgradeAttributes != null)
        {
            result.AddRange(normalUpgradeAttributes);
        }
        
        return result;
    }

    //공용 업그레이드 획득시마다 호출되는 함수. 기본 3종 유닛별 공용 업그레이드 리스트에 데이터를 추가함.
    private void AddNormalUpgradeDataToList(UnitType unitType_, NormalUnitUpgradeData data_)
    {
        switch (unitType_)
        {
            case UnitType.SwordMan:
                _swordManNormalUpgradeDataList.Add(data_);
                break;
            case UnitType.ArcherMan:
                _archerManNormalUpgradeDataList.Add(data_);
                break;
            case UnitType.Assassin:
                _assassinNormalUpgradeDataList.Add(data_);
                break;
            default:
                break;
        }
    }
}
