using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(ScarecrowData), fileName = nameof(ScarecrowData))]
public class ScarecrowData : SkillData
{
    public GameObject _effectPrefab;

    [Range(0f, 1f)]
    public float _hpRecoveryRate;
    [Range(0f, 1f)]
    public float _eliteHpRecoveryRate;
}
