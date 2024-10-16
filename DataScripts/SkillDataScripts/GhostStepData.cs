using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(GhostStepData), fileName = nameof(GhostStepData))]
public class GhostStepData : SkillData
{
    [Title("Level 1")]
    public float duration1;
    public float velocityMultiplier1;
    public float fearDuration1;
    public float fearCoolTime1;
    public float fearRange1;
    
    [Title("Level 2")]
    public float additionalFearDurationSecond;
}
