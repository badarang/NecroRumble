using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(BlackGrimoireData), fileName = nameof(BlackGrimoireData))]
public class BlackGrimoireData : SkillData
{    
    [SuffixLabel(label:"%", overlay: true)]
    public float downReviveCoolPercent;
    [SuffixLabel(label:"%", overlay: true)]
    public float downUnitHp;
    public float maxUnitNumMultiflier;
}
