using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(BoneSpearData), fileName = nameof(BoneSpearData))]
public class BoneSpearData : SkillData
{
    public int requiredMP;
    
    public float maxCooltime;

    public float speed;
    public float damage;
    public float destroyRange;

    public Color32 lineCol = new Color32(20, 150, 200, 100);

    public GameObject boneSpearPrefab;
    public GameObject cuteBoneSpearPrefab;
}
