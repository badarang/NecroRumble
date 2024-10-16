using LOONACIA.Unity.Managers;
using UnityEngine;

public class GhostStep : SkillBase
{
    GhostStepData _data;

    private void Init()
    {
        _data = LoadData<GhostStepData>();
        Id    = _data.Id;
        Name  = _data.name;
    }

    private void OnUnitSpawn(Unit unit_){
        if(IsCanExecuteSkill(unit_)){
            ExecuteSkill(unit_);
        }
    }

    bool IsCanExecuteSkill(Unit unit_){
        if (SkillLevel == 0) return false;
        if (unit_.IsDead) return false;
        if (unit_.UnitType != (int)UnitType.Assassin) return false;
        if (unit_.CurrentFaction != Faction.Undead) return false;

        return true;
    }
    
    private void ExecuteSkill(Unit unit_)
    {        
        AssassinUnit assassinManUnit_ = (AssassinUnit)unit_;
        
        assassinManUnit_.StartGhostStepCoolTimeRoutine(_data.SkillDataList[0].Cooldown, _data.duration1, _data.velocityMultiplier1, _data.fearRange1, _data.fearDuration1, _data.fearCoolTime1);
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
            // 공포 지속 효과 상승
            Unit.additionalFearDuration += _data.additionalFearDurationSecond;
            Debug.Log("Ghost Step 스킬 레벨 2 업그레이드 완료");
        }
    }
    
    public override void OnSkillAttained()
    {
        ManagerRoot.Event.onUnitSpawn += OnUnitSpawn;
        Init();
        
        SkillLevel = 1;

        var undeads = ManagerRoot.Unit.GetAllAliveUndeadUnits();
        foreach(var undead in undeads){
            if(IsCanExecuteSkill(undead)){
                ExecuteSkill(undead);
            }
        }
        
        Debug.Log("Ghost Step 스킬 레벨 1 획득 완료");
    }
    
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onUnitSpawn -= OnUnitSpawn;
    }


}
