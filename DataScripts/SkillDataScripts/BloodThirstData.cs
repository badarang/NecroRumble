using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(BloodThirstData), fileName = nameof(BloodThirstData))]
public class BloodThirstData : SkillData
{
    [Title("Level 1")]
    [SuffixLabel(label:"%", overlay: true), MinMaxSlider(0,100, true)]
    [SerializeField] private Vector2Int _minMaxDrainPercent;
    [SuffixLabel(label:"%", overlay: true)]
    [SerializeField] private int _maxDrainHealthThreshold;
    
    [Title("Level 2")]
    [SerializeField] private float _lv2HealingMultiplier;
    
    public float MinDrainPercent => _minMaxDrainPercent.x;
    public float MaxDrainPercent => _minMaxDrainPercent.y;
    public float MaxDrainHealthThreshold => _maxDrainHealthThreshold;
    public float Lv2HealingMultiplier => _lv2HealingMultiplier;
}
