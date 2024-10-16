using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(WraithOfButcherData), fileName = nameof(WraithOfButcherData))]
public class WraithOfButcherData : SkillData
{
    [SerializeField] private float _spawnDistance;
    [SerializeField] private float _spawnDistanceRandomness;
    [SerializeField] private uint _spawnCount;
    
    public float SpawnDistance => _spawnDistance;
    public float SpawnDistanceRandomness => _spawnDistanceRandomness;
    public uint SpawnCount => _spawnCount;
    
    
}
