using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(ImmortalWarriorData), fileName = nameof(ImmortalWarriorData))]
public class ImmortalWarriorData : SkillData
{
    [Title("Level 1")]
    public float necroMancerReviveCoolTimeReturnValue;
    
    [Title("Level 2")]
    public float necroMancerHpUpValue;
}
