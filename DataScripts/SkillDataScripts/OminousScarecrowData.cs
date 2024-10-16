using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(OminousScarecrowData), fileName = nameof(OminousScarecrowData))]
public class OminousScarecrowData : SkillData
{
    public int thresholdUnitNum;

    [SuffixLabel(label:"%", overlay: true)]
    public float downReviveCoolPercent;
    
}
