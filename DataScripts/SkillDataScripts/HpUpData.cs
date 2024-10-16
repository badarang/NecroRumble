using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(HpUpData), fileName = nameof(HpUpData))]
public class HpUpData : SkillData
{
    [Title("Level 1")]
    public int increaseMaxHp;
    [Title("Level 2")]
    public int increaseMaxHp2;
}
