using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(CommandDarkData), fileName = nameof(CommandDarkData))]
public class CommandDarkData : SkillData
{
    public float invinDuration;
    
}
