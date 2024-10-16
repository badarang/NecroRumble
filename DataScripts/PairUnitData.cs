using System;
using LOONACIA.Unity.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Unit/" + nameof(PairUnitData), fileName = nameof(PairUnitData))]
public class PairUnitData : ScriptableObject
{
    
    [SerializeField] private UnitAggroStats _unitAggroStats = new();
    [SerializeField] private HumanUnitStats _humanUnitStats = new();
    [SerializeField, HorizontalGroup(width: .7f)] private UndeadUnitStats _undeadUnitStats = new();
    [SerializeField] private UnitInfo _unitInfo;
    
    [ShowInInspector,ReadOnly,HorizontalGroup,HideLabel]
    private HumanUnitStats HumanUnitStats => _humanUnitStats; //인스펙터에서 언데드 유닛과 비교용
    
    public BaseUnitStats GetStats(Faction faction_)
    {
        BaseUnitStats result = faction_ == Faction.Human ? _humanUnitStats : _undeadUnitStats;
        return result;
    }
    
    public UnitAggroStats UnitAggroStats => _unitAggroStats;
    public UnitInfo UnitInfo => _unitInfo;
}

[System.Serializable]
public abstract class BaseUnitStats
{
    [SerializeField, HideLabel, Title("Base Max HP",horizontalLine: false)] private int _baseMaxHp;
    [SerializeField, HideLabel, Title("Base Move Speed",horizontalLine: false)] private float _baseMoveSpeed;
    [SerializeField, HideLabel, Title("Base Attack Range",horizontalLine: false)] private float _baseAttackRange;
    [SerializeField, HideLabel, Title("Base Attack Per Sec",horizontalLine: false)] private float _baseAttackPerSec;
    [PropertySpace(SpaceAfter = 30)]
    [SerializeField, HideLabel, Title("Base Attack Damage",horizontalLine: false)] private int _baseAttackDamage;
    
    public int BaseMaxHp => _baseMaxHp;
    public float BaseMoveSpeed => _baseMoveSpeed;
    public float BaseAttackRange => _baseAttackRange;
    public float BaseAttackPerSec => _baseAttackPerSec;
    public int BaseAttackDamage => _baseAttackDamage;
}

[System.Serializable]
public struct UnitInfo
{
    [SerializeField] private UnitType _unitType;
    [SerializeField] private string _unitName;
    [SerializeField] private string _unitDescription;
    public UnitType UnitType => _unitType;
    public string UnitName => _unitName;
    public string UnitDescription => _unitDescription;
}

[System.Serializable]
public class HumanUnitStats : BaseUnitStats
{
    [Header("어그로 관련")]
    [SerializeField, Range(-2f,2f)] private float _presenceAggroWeight;
    
    public float PresenceAggroWeight => _presenceAggroWeight;
} 

[System.Serializable]
public class UndeadUnitStats : BaseUnitStats
{
    [SerializeField] private int _baseMaxAggroNum;
    [SerializeField] private int _baseMaxHatred;
    
    public int BaseMaxAggroNum => _baseMaxAggroNum;
    public int BaseMaxHatred => _baseMaxHatred;
}

[Serializable]
public class UnitAggroStats
{
    [TitleGroup("언데드, 인간 공용")]
    [Tooltip("타게팅하고 있던 적에게 부여할 보너스의 가중치. 0이면 때리고 있던 적이든 아니든 아무 상관 없다.")]
    [SerializeField, Range(0, 2)] private float _aggroInertiaWeight;
    [Tooltip("감지 범위 내 상대 지휘관급 유닛에게 부여할 보너스의 가중치. 0이면 지휘관급 유닛인지 여부가 영향을 끼치지 않는다.")]
    [SerializeField, Range(-2, 2)] private float _aggroCommanderWeight;
    [Tooltip("감지 범위 내 상대 원거리 유닛에게 부여할 보너스의 가중치. 0이면 원거리 유닛인지 여부가 영향을 끼치지 않는다.")]
    [SerializeField, Range(-2, 2)] private float _aggroRangedDealerWeight;
    [TitleGroup("인간 전용")]
    [Tooltip("인간 유닛 전용. 언데드 유닛중 어그로 수용량이 많이 남아있을수록 부여하는 어그로 보너스의 가중치. 0이면 어그로 수용량의 남은 양이 영향을 끼치지 않는다.")]
    [SerializeField, Range(-2, 2)] private float _aggroPresenceWeight_h; // only human
    
    public float AggroInertiaWeight => _aggroInertiaWeight;
    public float AggroCommanderWeight => _aggroCommanderWeight;
    public float AggroRangedDealerWeight => _aggroRangedDealerWeight;
    public float AggroPresenceWeight_h => _aggroPresenceWeight_h;
}