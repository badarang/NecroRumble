using LOONACIA.Unity.Managers;
using UnityEngine;

public class BlackGrimoire : SkillBase
{
    BlackGrimoireData _data;
    Player _player = GameManager.Instance.GetPlayer();

    private void Init()
    {
        _data = LoadData<BlackGrimoireData>();
        Id    = _data.Id;
        Name  = _data.name;
    }

    private void OnUnitDeath(Unit deadUnit_, AttackInfo attackInfo_)
    {
        if (SkillLevel == 0) return;
        if (deadUnit_.CurrentFaction != Faction.Undead) return;

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
        
        // 네크로맨서 쿨타임감소
        if(_player.TryGetComponent<Revive>(out Revive revive)){
            revive.MaxCooldownMutiflier *= 1 - (_data.downReviveCoolPercent/100);
        }

        // 유닛 체력 감소
        StatModifier mod = new StatModifier(StatType.MaxHp, "BlackGrimoire", (-1)*_data.downUnitHp, StatModifierType.FinalPercentage, true);
        ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.SwordMan, mod, false);
        ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, mod, false);
        ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.Assassin, mod, false);

        // 유닛 최대수 증가
        GameManager.Instance.MaxUndeadNum += GetUndeadNumAddition(_data.maxUnitNumMultiflier);
        GameManager.Instance.GameUI.SetUndeadNumUI();


        Debug.Log("BlackGrimoire 주물 획득 완료");
    }
    
    //언데드 개체수 얼마나 얻어야 할지 계산
    public static int GetUndeadNumAddition(float multiplier)
    {
        return Mathf.FloorToInt(GameManager.Instance.MaxUndeadNum * multiplier);
    }
    
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onUnitDeath -= OnUnitDeath;
    }
}
