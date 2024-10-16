using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(UndeadBombData), fileName = nameof(UndeadBombData))]
public class UndeadBombData : SkillData
{
    public GameObject _effectPrefab;
    
    public float _range;
    public float _damage;
    public float _knockBackPower;
}
