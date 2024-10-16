using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LOONACIA.Unity.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

public class Chest : ItemBase
{
	protected override void Start()
	{
		base.Start();
		SetValues(0f, 0f, 2.5f);
	}
	
	protected override void HandleCollectEvent()
	{
		var artifactLeftCount = UIRewardPopup.GetListsByGrade(5, 0).Count;

		if (artifactLeftCount > 0)
		{
			GameUIManager.Instance.ShowRewardPopupByRankDispersion(5, 0); // 특별 주물
		}
		else //모든 주물 획득했을 경우
		{
			//텍스트 UI 표시
			const string BALLOON_TEXT = "All Undead DMG +20%";
			GameUIManager.Instance.CreateNumberBalloon(BALLOON_TEXT, transform.position, Color.white, 1.2f, 2.5f);
			
			//실제 스탯 상승 적용
			var modifier = new StatModifier(StatType.AttackDamage, "Max Chest Modifier", 20, StatModifierType.FinalPercentage, true);
			ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.SwordMan, modifier, false);
			ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, modifier, false);
			ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.Assassin, modifier, false);
		}

		gameObject.SetActive(false);
		Destroy(gameObject);
	}
    
}
