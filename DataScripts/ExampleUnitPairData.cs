using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Unit/" + nameof(ExampleUnitPairData), fileName = nameof(ExampleUnitPairData))]
public class ExampleUnitPairData : PairUnitData
{
    [SerializeField] private float _specialSkillCooltime;
    
    public float SpecialSkillCooltime => _specialSkillCooltime;
}
