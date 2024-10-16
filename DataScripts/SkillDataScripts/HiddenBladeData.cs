using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(HiddenBlade), fileName = nameof(HiddenBlade))]
public class HiddenBladeData : SkillData
{
    [Title("Level 1")]
    public int additionalAttackCount1;
    public float hpDownPercent1;
    
    [Title("Level 2")]
    public int additionalAttackCount2;
    public float hpDownPercent2;
}
