using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class SkeletonWeight : SkillBase
{
    SkeletonWeightData _data;

    void Start()
    {
        _data = LoadData<SkeletonWeightData>();
        Id    = _data.Id;
        Name  = _data.name;
    }

    private void OnUnitRecallLanded()
    {
        List<Unit> undeadUnits = ManagerRoot.Unit.GetAllAliveUndeadUnits();
        foreach (Unit unit in undeadUnits)
        {
            GameObject crackParticle = ManagerRoot.Instantiate(_data._effectPrefab, unit.transform.position, Quaternion.identity);
            crackParticle.GetComponent<CrackEffectController>().Init(unit.IsFacingRight(), .5f);
            GameObject crackSpreadParticle = ManagerRoot.Instantiate(_data._effectRangePrefab, unit.transform.position, Quaternion.identity);
            Debug.Log("CrackSpreadParticle : " + crackSpreadParticle);
            unit.TakeDamageCircleRange(_data._strikeRange, _data._strikeDamage);
            
            Destroy(crackParticle, 7f);
            Destroy(crackSpreadParticle, 1f);
        }
    }

    public override void OnBattleStart()
    {
        
    }
    public override void OnBattleEnd()
    {
        
    }
    public override void OnSkillUpgrade()
    {
    }
    public override void OnSkillAttained()
    {
        ManagerRoot.Event.onRecallLanded += OnUnitRecallLanded;
    }
    
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onRecallLanded -= OnUnitRecallLanded;
    }
}
