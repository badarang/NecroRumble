using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(MultiShotData), fileName = nameof(MultiShotData))]
public class MultiShotData : SkillData
{
    [Title("Level 1")] 
    public float damageDecreasePercent1;
    [Title("Level 2")] 
    public float damageDecreasePercent2;
    
    
}
