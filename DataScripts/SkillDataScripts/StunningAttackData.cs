using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(StunningAttackData), fileName = nameof(StunningAttackData))]
public class StunningAttackData : SkillData
{
    /// <summary>
    /// 몇 공격마다 한번 공격하는지. 5이면 4번은 일반공격, 한번은 스턴 공격.
    /// </summary>
    [Title("Level 1")]
    [SerializeField] private int _numOfAtkInCycle;
    [SerializeField] private float _stunDuration;
    
    [Title("Level 2")]
    [SerializeField] private float _lv2StunDuration;
    
    public int NumOfAtkInCycle => _numOfAtkInCycle;
    public float StunDuration => _stunDuration;
    public float Lv2StunDuration => _lv2StunDuration;
}
