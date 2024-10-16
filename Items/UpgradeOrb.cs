using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

public class UpgradeOrb : ItemBase
{
    private StatModifier _statModifier;
    protected override void Start()
    {
        base.Start();
        Init();
    }

    private void Init()
    {
        var upgradeOrbData = ManagerRoot.Resource.Load<UpgradeOrbData>("Data/" + nameof(UpgradeOrbData));
        var randomIndex = Random.Range(0, upgradeOrbData.PossibleUpgrades.Count);
        _statModifier = upgradeOrbData.PossibleUpgrades[randomIndex];      
        SetValues(14f, 5f, 1f);
    }
    
    private void ShowBalloonNotification()
    {
        var finalText = "";
        var statType = "";
        statType = _statModifier.statType switch
        {
            StatType.MaxHp => "^text_popup_orb_Maxhp",
            StatType.AttackDamage => "^text_popup_orb_Attack",
            StatType.AttackPerSec => "^text_popup_orb_Attackspeed",
            StatType.MoveSpeed => "언데드 이동 속도",
            _ => ""
        };
        if (statType == "") return;
        
        finalText = $"{ManagerRoot.I18N.getValue(statType)} +{Mathf.RoundToInt(_statModifier.value)}%";
        GameUIManager.Instance.CreateNumberBalloon(finalText, transform.position, Color.white, 1.2f, 2.5f);
    }
    
    protected override void HandleCollectEvent()
    {
        //스탯 상승
        ShowBalloonNotification();
        
        //실제 스탯 상승 적용
        var statType = _statModifier.statType;
        if (statType == StatType.MaxHp || statType == StatType.AttackDamage || statType == StatType.AttackPerSec || statType == StatType.MoveSpeed)
        {
            ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.SwordMan, _statModifier, false);
            ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, _statModifier, false);
            ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.Assassin, _statModifier, false);
        }
        
        ManagerRoot.Sound.PlaySfx("Powerup upgrade 6", 1f);
        
        //게임 오브젝트 삭제
        gameObject.SetActive(false);
        ManagerRoot.Resource.Release(gameObject);
    }
}
