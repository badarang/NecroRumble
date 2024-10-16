using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(SoulDashData), fileName = nameof(SoulDashData))]
public class SoulDashData : SkillData
{
    [Title("Level 1")]
    [SerializeField] private float _lv1FearDuration;
    [SerializeField] private float _lv1FearRadius;
    
    [Title("Level 2")]
    [SerializeField] private int _lv2MoveSpeedIncreasePercent;

    [SerializeField] private float _lv2RecallCooldownReduceSecond;

    public float Lv1FearDuration => _lv1FearDuration;
    public float Lv1FearRadius => _lv1FearRadius;
    public int Lv2MoveSpeedIncreasePercent => _lv2MoveSpeedIncreasePercent;
    public float Lv2RecallCooldownReduceSecond => _lv2RecallCooldownReduceSecond;
}
