using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Data/" + nameof(PlayerData), fileName = nameof(PlayerData))]
public class PlayerData : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed;
    
    [Header("Stat")]
    [SerializeField] private int _maxHP;
    [SerializeField] private int _maxMP;
    [SerializeField] private float _boneRegenPerSec;

    [Header("Revive")] 
    [SerializeField] private float _necroRadiusIncreaseDelay;
    
    public float MoveSpeed => _moveSpeed;
    
    public int MaxHP => _maxHP;
    public int MaxMP => _maxMP;
    public float BoneRegenPerSec => _boneRegenPerSec;
    
    public float NecroRadiusIncreaseDelay => _necroRadiusIncreaseDelay;
}
