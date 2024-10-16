using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/" + nameof(NecroResurrectData), fileName = nameof(NecroResurrectData))]
public class NecroResurrectData : SkillData
{
    [Title("Resurrection")] 
    [SerializeField] private float _deathDuration;
    [SerializeField] private float _resurrectAnimationDuration;
    [Range(0,1)]
    [SerializeField] private float _resurrectHpRatio;
    [SerializeField] private float _invincibleDuration;
    
    [Title("Damage Wave")]
    [SerializeField] private float _damageWaveRadius;
    [SerializeField] private float _minWaveSpeed;
    [SerializeField] private float _maxWaveSpeed;
    [SerializeField] private int _damage;
    
    public float DeathDuration => _deathDuration;
    public float ResurrectAnimationDuration => _resurrectAnimationDuration;
    public float ResurrectHpRatio => _resurrectHpRatio;
    public float DamageWaveRadius => _damageWaveRadius;
    public float MinWaveSpeed => _minWaveSpeed;
    public float MaxWaveSpeed => _maxWaveSpeed;
    public int Damage => _damage;
    public float InvincibleDuration => _invincibleDuration;
}
