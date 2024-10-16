using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(GiantSkeletonData), fileName = nameof(GiantSkeletonData))]
public class GiantSkeletonData : SkillData
{
    [Title("Level 1")]
    [SuffixLabel(overlay: true, label: "%")]
    public int hpUpMultiflier;
    [SuffixLabel(overlay: true, label: "%")]
    public int giantProbability;
    public float giantScaleMultiflier;
    
    [Title("Level 2")]
    [SuffixLabel(overlay: true, label: "%")]
    public int damageReducePercent;
    public float damageReduceRadius;
}
