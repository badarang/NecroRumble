using LOONACIA.Unity.Managers;
using UnityEngine;

public class ImmortalWarrior : SkillBase
{
    ImmortalWarriorData _data;
    Player _player => GameManager.Instance.GetPlayer();

    private void Init()
    {
        _data = LoadData<ImmortalWarriorData>();
        Id    = _data.Id;
        Name  = _data.name;
    }

    private void OnUnitSpawn(Unit unit_){
        if (SkillLevel < 1) return;
        if (unit_.UnitType != (int)UnitType.SwordMan) return;
        if (unit_.CurrentFaction != Faction.Undead) return;

        // 네크로맨서 쿨타임감소
        if(_player.TryGetComponent<Revive>(out Revive revive)){
            revive.CurrentCooldown -= _data.necroMancerReviveCoolTimeReturnValue;
        }
    }
    
    private void OnUnitDeath(Unit deadUnit_, AttackInfo attackInfo_)
    {
        if (SkillLevel < 2) return;
        if (deadUnit_.UnitType != (int)UnitType.SwordMan) return;
        if (deadUnit_.CurrentFaction != Faction.Undead) return;

        // 네크로맨서 체력 회복
        _player.TakeHeal(_data.necroMancerHpUpValue);
    }

    public override void OnBattleStart()
    {
        
    }
    public override void OnBattleEnd()
    {
        
    }    
    public override void OnSkillUpgrade()
    {
        if (SkillLevel == 2)
        {
            Debug.Log("Immortal Warrior 스킬 레벨 2 업그레이드 완료");
        }
    }
    
    public override void OnSkillAttained()
    {
        ManagerRoot.Event.onUnitSpawn += OnUnitSpawn;
        ManagerRoot.Event.onUnitDeath += OnUnitDeath;
        Init();
        
        SkillLevel = 1;
        OnBattleStart();
        
        Debug.Log("Immortal Warrior 스킬 레벨 1 획득 완료");
    }
    
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onUnitSpawn -= OnUnitSpawn;
        ManagerRoot.Event.onUnitDeath -= OnUnitDeath;
    }
}
