using LOONACIA.Unity.Managers;
using UnityEngine;

public class OminousScarecrow : SkillBase
{
    OminousScarecrowData _data;
    Player _player => GameManager.Instance.GetPlayer();

    bool isMultified = false;

    private void Init()
    {
        _data = LoadData<OminousScarecrowData>();
        Id    = _data.Id;
        Name  = _data.name;
    }

    private void OnUnitSpawn(Unit Unit_){
        if (SkillLevel == 0) return;
        if (Unit_.CurrentFaction != Faction.Undead) return;

        CheckUnitNum();
    }
    
    private void OnUnitDeath(Unit deadUnit_, AttackInfo attackInfo_)
    {
        if (SkillLevel == 0) return;
        if (deadUnit_.CurrentFaction != Faction.Undead) return;

        CheckUnitNum();
    }
    
    private void OnUnitGrind(Unit Unit_)
    {
        if (SkillLevel == 0) return;
        if (Unit_.CurrentFaction != Faction.Undead) return;

        CheckUnitNum();
    }
    
    private void CheckUnitNum()
    {
        if (SkillLevel == 0) return;
        if (!_player.TryGetComponent(out Revive revive)) return;
        
        if(ManagerRoot.Unit.CountUndeadUnit() < _data.thresholdUnitNum) //언데드 유닛이 충분히 적으면
        {
            if (isMultified) return;
            // 네크로맨서 쿨타임감소
            revive.MaxCooldownMutiflier -= Mathf.Abs(_data.downReviveCoolPercent/100);
            isMultified = true;
        }
        else if(isMultified) //언데드 유닛 너무 많고 쿨타임 감소 효과가 적용중이면
        {
            // 네크로맨서 쿨타임 원상복귀
            revive.MaxCooldownMutiflier += Mathf.Abs(_data.downReviveCoolPercent/100);
            isMultified = false;
        }
    }
    
    public override void OnSkillAttained()
    {
        SkillLevel = 1;
        Init();
        
        ManagerRoot.Event.onUnitDeath += OnUnitDeath;
        ManagerRoot.Event.onUnitSpawn += OnUnitSpawn;
        ManagerRoot.Event.onUnitGrind += OnUnitGrind;
        
        CheckUnitNum();
        
        Debug.Log("CursedSecond 주물 획득 완료");
    }
    
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onUnitDeath -= OnUnitDeath;
        ManagerRoot.Event.onUnitSpawn -= OnUnitSpawn;
        ManagerRoot.Event.onUnitGrind -= OnUnitGrind;
    }

    public override void OnSkillUpgrade()
    {
    }

    public override void OnBattleStart()
    {
    }

    public override void OnBattleEnd()
    {
    }
}
