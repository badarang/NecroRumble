using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(GuardDutyData), fileName = nameof(GuardDutyData))]
public class GuardDutyData : SkillData
{
    [Title("Level 1")]
    [SuffixLabel(overlay: true, label: "%")]
    [SerializeField] private float _damageReducePercent;
    [SuffixLabel(overlay: true, label: "%")]
    [SerializeField] private float _atkSpeedModifyingPercent;
    
    [SerializeField, Range(0.05f,.3f)] private float _loopCheckInterval;
    [SerializeField] private float _range;
    
    [Title("Level 2")]
    [SerializeField] private int _takeDamageInsteadPercentage;

    public float DamageReducePercent => _damageReducePercent;
    public float AtkSpeedModifyingPercent => _atkSpeedModifyingPercent;
    public float LoopCheckInterval => _loopCheckInterval;
    public float Range => _range;
    public int TakeDamageInsteadPercentage => _takeDamageInsteadPercentage;
}
