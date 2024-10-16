using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(LapelOfNightmareData), fileName = nameof(LapelOfNightmareData))]
public class LapelOfNightmareData : SkillData
{
    [Title("Level 1")]
    public float hpUpRatio1;
    [Range(0f,1f)]
    public float fearRatio1;
    public float fearDuration1;
    public float attackSpeedDownValue1;
    
    [Title("Level 2")]
    public float hpRecoveryValue2;
    public float attackSpeedDownValue2;
}
