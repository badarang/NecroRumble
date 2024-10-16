using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(ThirdHitData), fileName = nameof(ThirdHitData))]
public class ThirdHitData : SkillData
{
    [Title("Level 1")] 
    public float damagePercent1;
    public float damagePercentElite1;
    public float damageDecreasePercent1;
    [Title("Level 2")] 
    public float damagePercent2;
    public float damagePercentElite2;
    public float damageDecreasePercent2;
}
