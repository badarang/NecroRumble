using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(SkeletonWeightData), fileName = nameof(SkeletonWeightData))]
public class SkeletonWeightData : SkillData
{
    public GameObject _effectPrefab;
    public GameObject _effectRangePrefab;
    public float _strikeDamage;
    public float _strikeRange;
}
