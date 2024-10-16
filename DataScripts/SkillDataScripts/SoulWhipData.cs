using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(SoulWhipData), fileName = nameof(SoulWhipData))]
public class SoulWhipData : SkillData
{
    public float undeadDealtDamage;
    public float attackSpeedIncreaseDuration;
    public float attackSpeedIncreaseRatio;
}
