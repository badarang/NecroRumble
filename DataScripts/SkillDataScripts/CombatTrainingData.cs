using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(CombatTrainingData), fileName = nameof(CombatTrainingData))]
public class CombatTrainingData : SkillData
{
    [Title("Level 1")] 
    public int damageIncreaseNeedCount1;
    public int damageIncreasePercent;
    public float attackSpeedDecreasePercent1;
    public Sprite image1;

    [Title("Level 2")]
    public float additionalDamageIncreaseMultiplier2;
}
