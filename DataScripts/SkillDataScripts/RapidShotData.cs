using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(RapidShotData), fileName = nameof(RapidShotData))]
public class RapidShotData : SkillData
{
    [Title("Level 1")]
    public float damageDecreasePercent1;
    public float attackTriggerCount1;
    public float attackTriggerDuration1;
    public float attackSpeedUpPercent1;
    [Title("Level 2")]
    public float damageDecreasePercent2;
    public float attackTriggerCount2;
    public float attackTriggerDuration2;
    public float attackSpeedUpPercent2;
}
