using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(PoisonArrowData), fileName = nameof(PoisonArrowData))]
public class PoisonArrowData : SkillData
{
    [Title("Level 1")] 
    public float damagePercent1;
    public float damageDuration1;
    
    [Title("Level 2")]
    public float damagePercent2;
    public float damageDuration2;
}
