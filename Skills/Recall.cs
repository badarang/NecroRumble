using System;
using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class Recall : SkillBase
{
	private Player _player;
	private RecallData _data;
	
	public float cooldownAddition = 0;
	
	private void Start()
	{    
		_player = (GameManager.Instance == null) ? FindObjectOfType<Player>() : GameManager.Instance.GetPlayer();
	   
		_data = LoadData<RecallData>();

	}

	public void ExecuteRecallCommand()
	{
		if (Player.currentCommand == EnumPlayerCommand.Recall || ManagerRoot.Unit.CountUndeadUnit() < 1) return;
		if (IsCooldown)
		{
			GameUIManager.Instance.SetRecallDoScale();
			GameUIManager.Instance.SetSkillCoolDownText();
			return;
		}
		
		Player.currentCommand = EnumPlayerCommand.Recall;
		List<Unit> units = ManagerRoot.Unit.GetAllAliveUndeadUnits();
		List<Vector2> positionList = RtsUtilities.GetOrderedPositionOffsetsForUnits(units, transform.position,
			_data.StartingRingRadius, _data.RingRadiusIncrement, _data.StartingRingUnitCount, 
			_data.RingUnitCountMultiplier, units.Count);

		for (int i = 0; i < units.Count; i++)
		{
			units[i].StartRecall(positionList[i], _data, i == 0);
		}
		ManagerRoot.Event.onRecallStarted?.Invoke();
		
		//몇 초 뒤 RecallCommand를 끝내기
		StartCoroutine(EndRecallCoroutine());

		MaxCooldown = _data.GetCurSkillData(SkillLevel).Cooldown + cooldownAddition;
		CurrentCooldown = MaxCooldown;
		UseRecallUI();
	}

	void Update()
	{
		TickCooldown();
	}

	void TickCooldown()
	{
		if (IsCooldown)
		{
			#if UNITY_EDITOR
			if (_player.IsGodMode) IsCooldown = false;
			#endif
			CurrentCooldown -= Time.deltaTime;
			if(CurrentCooldown < 0)
			{
				IsCooldown = false;
			}
		}
	}
	private IEnumerator EndRecallCoroutine()
	{
		var beforeJumpTime = .2f;
		yield return new WaitForSeconds(_data.TotalRecallDuration - beforeJumpTime);
		ManagerRoot.Event.onRecallLanded?.Invoke();
		yield return new WaitForSeconds(beforeJumpTime);

		Player.currentCommand = EnumPlayerCommand.None;
		ManagerRoot.Event.onRecallActionEnd?.Invoke();
	}
	
	private void UseRecallUI()
	{
		GameUIManager.Instance.SetRecallCooldownUI();
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
	}
}
