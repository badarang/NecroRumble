using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(AdditionalHitData), fileName = nameof(AdditionalHitData))]
public class AdditionalHitData : SkillData
{
    [Title("Level 1")] 
    public float damageDecreasePercent1;
    public float attackSpeedPercent1;
    public float additionalDamagePerAttack1;
    [Title("Level 2")] 
    public float damageDecreasePercent2;
    public float attackSpeedPercent2;
    public float additionalDamagePerAttack2;
}
