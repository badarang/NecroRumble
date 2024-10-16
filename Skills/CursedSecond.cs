using System;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class CursedSecond : SkillBase
{
    CursedSecondData _data;
    Player _player => GameManager.Instance.GetPlayer();

    private void Init()
    {
        _data = LoadData<CursedSecondData>();
        Id    = _data.Id;
        Name  = _data.name;
    }

    private void OnUnitDeath(Unit deadUnit_, AttackInfo attackInfo_)
    {
        if (SkillLevel == 0) return;
        if (deadUnit_.CurrentFaction != Faction.Undead) return;

        // 네크로맨서 쿨타임감소
        if(_player.TryGetComponent<Revive>(out Revive revive)){
            revive.CurrentCooldown -= _data.downReviveCool;
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
        ManagerRoot.Event.onUnitDeath += OnUnitDeath;
        Init();
        
        SkillLevel = 1;
        OnBattleStart();
        
        Debug.Log("CursedSecond 주물 획득 완료");
    }

    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onUnitDeath -= OnUnitDeath;
    }
}
