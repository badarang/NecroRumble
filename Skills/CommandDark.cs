using System.Collections;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class CommandDark : SkillBase
{
    CommandDarkData _data;
    bool canSkillExecute = true;
    Coroutine _recallCoroutine;

    private void Init()
    {
        _data = LoadData<CommandDarkData>();
        Id    = _data.Id;
        Name  = _data.name;
    }

    private void OnUnitRecall(){        
        if (SkillLevel == 0) return;
        if(!canSkillExecute) return;

        var undeads = ManagerRoot.Unit.GetAllAliveUndeadUnits();
        foreach (var unit in undeads)
        {
            StartCoroutine(unit.InvincibleRoutine(_data.invinDuration));
        }
        canSkillExecute = false;
        if (_recallCoroutine != null)
        {
            StopCoroutine(_recallCoroutine);
        }
        _recallCoroutine = StartCoroutine(CoolDownRoutine(_data.SkillDataList[SkillLevel - 1].Cooldown));

    }

    
    IEnumerator CoolDownRoutine(float cooltime_){
        yield return new WaitForSeconds(cooltime_);
        canSkillExecute = true;
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
        ManagerRoot.Event.onRecallActionEnd += OnUnitRecall;
        Init();
        
        SkillLevel = 1;
        OnBattleStart();

        Debug.Log("CommandDark 주물 획득 완료");
    }
    
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        if (ManagerRoot.Instance == null) return;
        if (ManagerRoot.Event == null) return;
        ManagerRoot.Event.onRecallActionEnd -= OnUnitRecall;
    }
}
