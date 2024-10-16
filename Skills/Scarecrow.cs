using LOONACIA.Unity.Managers;
using UnityEngine;

public class Scarecrow : SkillBase
{
    ScarecrowData _data;

    void Start()
    {
        _data = LoadData<ScarecrowData>();
        Id    = _data.Id;
        Name  = _data.name;
    }

    private void Execute(Unit deadUnit_, AttackInfo attackInfo_)
    {
        if (deadUnit_   == null) return;
        if (deadUnit_.gameObject.layer != Layers.UndeadCorpse) return;  // 아군 유닛이 죽었을 때만 실행
        if (deadUnit_.isReRevive) return;  // 이미 유닛이 밀짚인형으로 살아났으면 실행 안함.

        deadUnit_.IncrementReReviveCount();

        deadUnit_.gameObject.layer = Layers.UndeadUnit;
        deadUnit_.GetComponent<Collider2D>().isTrigger = true;
        
        var healthRatio = deadUnit_.IsElite ? _data._eliteHpRecoveryRate : _data._hpRecoveryRate;
        int healAmount = (int) (deadUnit_.instanceStats.FinalMaxHp * healthRatio);
        if (healAmount < 1) healAmount = 1;
        
        if( deadUnit_.instanceStats == null ){ Debug.LogWarning("instanceStats == null "); return;}
        deadUnit_.SetCurrentHpWithoutFeedback(healAmount);

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
        ManagerRoot.Event.onUnitDeath += Execute;
    }
    
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onUnitDeath -= Execute;
    }
}
