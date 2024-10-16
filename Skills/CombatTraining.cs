using LOONACIA.Unity.Managers;
using UnityEngine;

public class CombatTraining : SkillBase
{
    CombatTrainingData _data;
    private int _damageIncreaseNeedCount;
    public int CurrentDamageIncreaseAmount = 0; //2번째 스킬 설명창에 표시할 현재 데미지 증가량
    
    void Init()
    {
        _data = LoadData<CombatTrainingData>();
        Id    = _data.Id;
        Name  = _data.name;
    }
    
    private void OnUnitDeath(Unit deadUnit_, AttackInfo attackInfo_)
    {
        if (SkillLevel == 0) return;
        if (attackInfo_.attacker == null) return;
        
        
        if (attackInfo_.attacker.GameObject.TryGetComponent<ArcherManUnit>(out var archerManUnit_))
        {
            if (archerManUnit_.UnitType != (int)UnitType.ArcherMan) return;
            if (archerManUnit_.CurrentFaction != Faction.Undead) return;
            
            _damageIncreaseNeedCount++;
            //Debug.Log("CombatTraining 데미지 증가 카운트 : " + _damageIncreaseNeedCount);
            if (_damageIncreaseNeedCount >= _data.damageIncreaseNeedCount1)
            {
                _damageIncreaseNeedCount = 0;
                CurrentDamageIncreaseAmount += _data.damageIncreasePercent;
                StatModifier mod = new StatModifier(StatType.AttackDamage, "CombatTrainingDamageIncreaseInfinity", _data.damageIncreasePercent, StatModifierType.Percentage, true);
                ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, mod, false);
                
                foreach (var _archer in ManagerRoot.Unit.GetSpecificUnitTypeAliveUnits(UnitType.ArcherMan, Faction.Undead))
                {
                    Vector3 pos = _archer.transform.position + new Vector3(-.25f, 1f, 0);
                    UIImage curImage = ManagerRoot.UI.ShowSceneUI<UIImage>();
                    curImage.SetInitialPosition(pos);
                    curImage.SetInnerImage(_data.image1, .1f);
                    curImage.SetText(_data.damageIncreasePercent.ToString()+"%", Color.black, new Vector3(.5f, 0f, 0f));
                    curImage.SetSize(.75f);
                }
            }
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
        if (SkillLevel == 2)
        {
            StatModifier mod = new StatModifier(StatType.AttackDamage, "CombatTrainingAdditionalDamage", CurrentDamageIncreaseAmount * (_data.additionalDamageIncreaseMultiplier2 - 1), StatModifierType.Percentage, true);
            ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, mod, false);
            Debug.Log("CombatTraining 스킬 레벨 2 업그레이드 완료");
        }
    }
    
    public override void OnSkillAttained()
    {
        ManagerRoot.Event.onUnitDeath += OnUnitDeath;
        Init();
        SkillLevel = 1;

        StatModifier mod = new StatModifier(StatType.AttackPerSec, "CombatTraining", (-1) * _data.attackSpeedDecreasePercent1, StatModifierType.Percentage, true);

        ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, mod, false);
        
        Debug.Log("CombatTraining 스킬 레벨 1 획득 완료");
    }
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onUnitDeath -= OnUnitDeath;
    }
}
