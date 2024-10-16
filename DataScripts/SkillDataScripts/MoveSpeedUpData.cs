using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(MoveSpeedUpData), fileName = nameof(MoveSpeedUpData))]
public class MoveSpeedUpData : SkillData
{
    public float increaseMaxSpeed;
    public float increaseMaxAcceleration;
    public float increaseMaxDecceleration;
    public float trailCreateDelay;
    public float trailDamage;
    public GameObject trailPrefab;
}
