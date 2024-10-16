using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(GrindUnitData), fileName = nameof(GrindUnitData))]
public class GrindUnitData : SkillData
{

    public int _requiredMP;

    public RangeSelectorType _rangeSelectorType;

    public bool _canMove;
    
    public float speed;
    public float damage;
    public float destroyRange;
    
    public float _afterDelayTime;

    public Color32 lineCol = new Color32(200, 50, 50, 100);
    
    public GameObject grindHookPrefab;
}
