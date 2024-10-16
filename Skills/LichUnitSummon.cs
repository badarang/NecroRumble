using UnityEngine;
using LOONACIA.Unity.Managers;
using Sirenix.OdinInspector;

public class LichUnitSummon : SkillBase
{
    private Camera _cam => Camera.main;
    private GameObject _lineObject;
    private LineRenderer _lineRenderer;
    private Player _player;
    InputManager _input => ManagerRoot.Input;
    LichUnitSummonData _data;

    void Start()
    {
        _data = LoadData<LichUnitSummonData>();
        Id   = _data.Id;
        Name = _data.Name;
    }

    void Update()
    {
       

    }


    public override void OnBattleStart() {}
    public override void OnBattleEnd() {}
    public override void OnSkillUpgrade() {}
    public override void OnSkillAttained()
    {
    }
}
