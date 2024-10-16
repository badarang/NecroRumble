using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(FearOfBloodData), fileName = nameof(FearOfBloodData))]
public class FearOfBloodData : SkillData
{
    [Title("Level 1")]
    public float fearDuration1;
    public float hpDownPercent1;
    public float fearRange1;
    
    [Title("Level 2")]
    public float fearStateDamageMultiplier2;
    public float hpDownPercent2;
}
