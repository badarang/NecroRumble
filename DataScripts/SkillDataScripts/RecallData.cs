using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
[CreateAssetMenu(menuName = "Data/Skill/"+nameof(RecallData), fileName = nameof(RecallData))]

public class RecallData : SkillData
{
    [TitleGroup("리콜 포메이션 관련")]
    [SerializeField] private float _startingRingRadius;
    [SerializeField] private float _ringRadiusIncrement;
    [SerializeField] private int _startingRingUnitCount;
    [SerializeField] private int _ringUnitCountMultiplier;
    
    [TitleGroup("애니메이션 관련")]
    [SerializeField] private float _maxRandomDelay;
    [SerializeField] private float _jumpDuration;
    [SerializeField] private float _jumpHeight;
    [SerializeField] private float _bounceHeight;
    [SerializeField] private float _bounceDuration;
    [SerializeField] private float _rotateRollbackDuration;
    //[SerializeField] private float 
    
    public float StartingRingRadius => _startingRingRadius;
    public float RingRadiusIncrement => _ringRadiusIncrement;
    public int StartingRingUnitCount => _startingRingUnitCount;
    public int RingUnitCountMultiplier => _ringUnitCountMultiplier;
    
    public float MaxRandomDelay => _maxRandomDelay;
    public float Jumpduration => _jumpDuration;
    public float JumpHeight => _jumpHeight;
    public float BounceHeight => _bounceHeight;
    public float BounceDuration => _bounceDuration;
    public float RotateRollbackDuration => _rotateRollbackDuration;

    public float TotalRecallDuration => _jumpDuration + _bounceDuration;
}
