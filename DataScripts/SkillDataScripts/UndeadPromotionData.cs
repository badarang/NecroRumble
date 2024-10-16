using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(UndeadPromotionData), fileName = nameof(UndeadPromotionData))]
public class UndeadPromotionData : SkillData
{
    public int maxStackCount;
    [SuffixLabel(label:"%", overlay: true)]
    public int statIncreaseValue;
    public int eliteStatIncreaseValue;
    public float promotionDeltaTimeSecond;
    
}
