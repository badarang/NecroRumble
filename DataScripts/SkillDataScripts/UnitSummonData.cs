
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(UnitSummonData), fileName = nameof(UnitSummonData))]
public class UnitSummonData : SkillData
{
    [SerializeField] private int remainRound;
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private int summonCount;
    [SerializeField] private bool isEveryWave;
    
    
    public int RemainRound => remainRound;
    public GameObject UnitPrefab => unitPrefab;
    public int SummonCount => summonCount;
    public bool IsEveryWave => isEveryWave;
}
