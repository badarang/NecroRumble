
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(CorpseAbsorptionData), fileName = nameof(CorpseAbsorptionData))]
public class CorpseAbsorptionData : SkillData
{
    public int replenishMp;

    public float absorbEffectDelay;
    public float nextAbsorbDelay;

}
