using System.Collections;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class VeilOfNight : SkillBase
{
    VeilOfNightData _data;

    private void Init()
    {
        _data = LoadData<VeilOfNightData>();
        Id    = _data.Id;
        Name  = _data.name;
    }

    private void OnUnitSpawn(Unit unit_){        
        if (SkillLevel == 0) return;
        if (unit_.IsDead) return;
        if (unit_.CurrentFaction != Faction.Undead) return;
        
        StartCoroutine(unit_.InvincibleRoutine(_data.invinDuration));

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
        ManagerRoot.Event.onUnitSpawn += OnUnitSpawn;
        Init();
        
        SkillLevel = 1;
        OnBattleStart();

        Debug.Log("Ghost Step 주물 획득 완료");
    }
    
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onUnitSpawn -= OnUnitSpawn;
    }
}
