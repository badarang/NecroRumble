using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(HeavyAttackData), fileName = nameof(HeavyAttackData))]
public class HeavyAttackData : SkillData
{
    [Title("Level 1")] 
    public float attackRangeIncreasePercent1;
    public float attackDamageIncreaseFlat1;
    public float attackFinalDamageIncreasePercent1;
    public float attackSpeedDecreasePercent1;
    
    [Title("Level 2")]
    public float attackRangeIncreasePercent2;
    public float attackDamageIncreaseFlat2;
    public float attackSlowPercent2;
    public float attackSlowDuration2;
}
