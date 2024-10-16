using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(FuryHeartData), fileName = nameof(FuryHeartData))]
public class FuryHeartData : SkillData
{
    [Header("Level 1")]
    [SuffixLabel(label:"%", overlay: true), MinMaxSlider(0,100, true)]
    [SerializeField] private Vector2Int _minMaxVelPercent;
    [SuffixLabel(label:"%", overlay: true)]
    [SerializeField] private int _maxHealthThreshold;
    
    public float MinVelPercent => _minMaxVelPercent.x;
    public float MaxVelPercent => _minMaxVelPercent.y;
    public float MaxHealthThreshold => _maxHealthThreshold;

    [Header("Level 2")]
    public float invinDuration;
}
