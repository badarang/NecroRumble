using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(BlackArrowData), fileName = nameof(BlackArrowData))]
public class BlackArrowData : SkillData
{
    [Title("Level 1")] 
    public float attackSpeedDecreasePercent1;
    public float damageDelay1;
    public float damageMultiplier1;
    
    [Title("Level 2")]
    public float attackSpeedDecreasePercent2;
    public float damageDelay2;
    public float damageMultiplier2;
}
