using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(BloodFangData), fileName = nameof(BloodFangData))]
public class BloodFangData : SkillData
{
    public GameObject _effectPrefab;

    
    [Range(0,1)]
    public float hpRecoveryRate;
    [Range(0,1)]
    public float eliteHpRecoveryRate;

    public bool useFixedHealAmount;
    public int fixedHealAmount;
}
