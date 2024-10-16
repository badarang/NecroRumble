using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalRuby.LightningBolt;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using LOONACIA.Unity.Managers;
using UnityEngine.InputSystem;
using LOONACIA.Unity;

public enum InputHandlerType
{
	SpaceDownUpInputHandler,
	SpaceDownMouseDownUpInputHandler,
	ReviveNewInputHandler,
}
public enum RevivePriorityType
{
	Closest,
	Oldest,
}

public enum ReviveState
{
	Init,
	CastStart,
	Casting,
	CastEnd,
	AfterDelay,
}
public class Revive : SkillBase
{
	private Player _player;
	private PlayerMovement _playerMove;

	private IInputHandler _inputHandler;
	private RangeSelector _rangeSelector;
	private bool _canCast;

	[ReadOnly] public ReviveState reviveState = ReviveState.Init;


	[SerializeField] private float _reviveRange;
	[ReadOnly, SerializeField] private int _maxReviveCount;
	[ReadOnly, SerializeField] private float _corpseWeight;

	[ReadOnly, SerializeField] private float _afterDelayTimer;

	ReviveData _data;

	public float MaxCooldownMutiflier = 1;
	private float canReviveScaleDelay;
	public float ReviveRange => _reviveRange;
	public bool CanCast
	{
		get => _canCast;
		set
		{
			if (_canCast == value) return;
			_canCast = value;
			_rangeSelector.UpdateSelectedMaterials();
		}
	}
	

	void Start()
	{
		_data = LoadData<ReviveData>();
		Id = _data.Id;
		Name = _data.Name;

		_maxReviveCount = _data._maxReviveCount;

		_player = (GameManager.Instance == null) ? FindObjectOfType<Player>() : GameManager.Instance.GetPlayer();
		_playerMove = GetComponent<PlayerMovement>();

		SetInputHandler(_data._inputHandlerType);
		SetRangeSelector(_data._rangeSelectorType);

	}


	void Update()
	{
		TickCooldown();

		_inputHandler.HandleInput(); //GetInput();
		
		//실제 리바이브 로직
		if (_data._rangeSelectorType == RangeSelectorType.EclipseRangeSelector) 
		{
			//TODO: check revive cooltime
			//TODO: check max undead num
			
			bool canRevive = !IsCooldown && GameManager.Instance.GetRemainUndeadNum() > 0 && _player.CanAction;
			if (canRevive)
			{
				if (RangeSelector.CanDoScale && canReviveScaleDelay <= 0f)
				{
					GameUIManager.Instance.SetReviveDoScale(4);
					canReviveScaleDelay = 1f;
				}
				else
				{
					canReviveScaleDelay -= Time.deltaTime;	
				}
				UpdateEclipseRevive(); //이클립스 방식
			}
			else
			{
				_rangeSelector.ResetValues();
			}
		}
		else UpdateRevive(); //그 외

		CalcAfterDelay();
		CanCast = GetCanCast(false);
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
	
	void CalcAfterDelay()
	{
		if (_afterDelayTimer > 0f)
		{
			_afterDelayTimer -= Time.deltaTime;
		}
	}

	void UpdateRevive()
	{
		switch (reviveState)
		{
			case ReviveState.Init: // 아무것도 안하는 상태
				break;
			case ReviveState.CastStart: // 스페이스바 처음 누른 상태
				// _player.CostMp(_data._requiredMP);
				// SetRangeSelector(_data._rangeSelectorType); // 런타임에서 RangeSelector 바꿀일 있을때만 주석 풀면됨
				// _rangeSelector.Init();
				SetPlayerCanMove(_data._canMove);
				reviveState = ReviveState.Casting;
				break;
			case ReviveState.Casting: // 스페이스바 누르고 있는 상태
				_rangeSelector.RangeSelect(); // preview and charging

				_playerMove.PlayAnimation("NecromancerSpecialAttack"); // 손드는 애니메이션
				break;

			case ReviveState.CastEnd: // 스페이스바 올린 상태
				List<Unit> selectedCorpes = new List<Unit>(_rangeSelector.SelectedQ);

				ReviveUnit(selectedCorpes);
				_playerMove.PlayAnimation("NecromancerIdle"); // 가만히 숨쉬는 애니메이션

				ResetValue();
				reviveState = ReviveState.Init;
				//StartCoroutine(AfterDelayCo());
		
				break;
				// case ReviveState.AfterDelay: // 후딜 상태.
				// break;
		}

	}

	void UpdateEclipseRevive()
	{
		_rangeSelector.RangeSelect(); // preview and charging

		switch (reviveState)
		{
			case ReviveState.Init: // 아무것도 안하는 상태
				break;
			case ReviveState.CastStart: // 스페이스바 처음 누른 상태
				List<Unit> selectedCorpes = new List<Unit>(_rangeSelector.SelectedQ);
				if (IsCooldown || selectedCorpes.Count == 0)
				{
					reviveState = ReviveState.Init;
					return;
				}

				// SetRangeSelector(_data._rangeSelectorType); // 런타임에서 RangeSelector 바꿀일 있을때만 주석 풀면됨 
				// _rangeSelector.Init();
				SetPlayerCanMove(_data._canMove);

				UseReviveUI();

				ReviveUnit(selectedCorpes);

				ResetValue();
				reviveState = ReviveState.Init;

				break;
			case ReviveState.Casting: // 스페이스바 누르고 있는 상태
				break;

			case ReviveState.CastEnd: // 스페이스바 올린 상태
				break;

		}

	}

	// IEnumerator AfterDelayCo() // 현재 후딜 안씀
	// {
	//     //yield return new WaitUntil(() => ); // 모든 애들 다 살아난 후까지 기다림?

	//     while(_afterDelayTimer > 0)
	//     {
	//         _afterDelayTimer -= Time.deltaTime;
	//         yield return null;
	//     }
	//     _afterDelayTimer = _data._afterDelayTime;


	//     ResetValue();
	//     reviveState = ReviveState.Init;
	// }

	private void UseReviveUI()
	{
		MaxCooldown = _data.GetCurSkillData(SkillLevel).Cooldown * MaxCooldownMutiflier;
		CurrentCooldown = MaxCooldown;
		
		GameUIManager.Instance.SetReviveCooldownUI();
	}

	public bool GetCanCast(bool setUndeadUI = true)
	{
		if (IsCooldown)
		{
			// Debug.Log($" cooltime : {CurrentCooldown}");
			return false;
		}

		if (_afterDelayTimer > 0f)
		{
			// Debug.Log($" afterdelay : {_afterDelayTimer}");
			return false;
		}

		// if (_player.CurMp < _data._requiredMP)
		// {
		// 	// Debug.Log($" player mp : {_player.CurrentBonePiles}");
		// 	return false;
		// }

		if (GameManager.Instance.GetRemainUndeadNum() <= 0)
		{
			if (setUndeadUI)
				GameManager.Instance.GameUI.SetUndeadNumUI(true);
			// Debug.Log($" remain undead num : {GameManager.Instance.GetRemainUndeadNum()}");
			return false;
		}

		return true;
	}

	void ReviveUnit(List<Unit> selectedUnits)
	{
		_playerMove.GetComponent<Player>().IncreaseNecroLightIntensity(0f);
		_rangeSelector.ResetValues();

		// ManagerRoot.Input.Vibration(0.3f, 0.8f, true, true);

		ReviveCorpse(selectedUnits[0]);
	}

	void ReviveCorpse(Unit corpse)
	{
		corpse.TurnUndead();
		_player.CostCoprseUnderfoot(corpse); // only underfoot 할때만.사용. todo: 코드 개선 필요

		SteamUserData.CheckReviveCount();
	}

	void ResetValue() // 가변값 초기화
	{
		SetPlayerCanMove(true);

		MaxCooldown = _data.GetCurSkillData(SkillLevel).Cooldown * MaxCooldownMutiflier;
		CurrentCooldown = MaxCooldown;

	}

	public void SetPlayerCanMove(bool canMove_)
	{
		_playerMove.CanMove = canMove_;
	}

	IEnumerator WeightDecreaseCO()
	{
		yield return new WaitForSeconds(1f); // todo: wight 지속시간. 콜라이더 기준으로 바꿔야함
		_corpseWeight -= 3f; // todo: delta weight값을 unit 기준으로 변경 필요
	}

	void SetInputHandler(InputHandlerType type_)
	{
		switch (type_)
		{
			case InputHandlerType.SpaceDownUpInputHandler:
				_inputHandler = new SpaceDownUpInputHandler(this);
				break;
			case InputHandlerType.SpaceDownMouseDownUpInputHandler:
				_inputHandler = new SpaceDownMouseDownUpInputHandler(this);
				break;
			case InputHandlerType.ReviveNewInputHandler:
				_inputHandler = new ReviveNewInputHandler(this);
				break;
		}
	}

	void SetRangeSelector(RangeSelectorType type_)
	{
		switch (type_)
		{
			case RangeSelectorType.CircleRangeSelector:
				_rangeSelector = new CircleRangeSelector(Layers.HumanCorpse.ToMask());
				break;
			case RangeSelectorType.EclipseRangeSelector:
				_rangeSelector = new EclipseRangeSelector(Layers.HumanCorpse.ToMask(), _reviveRange);
				break;
			case RangeSelectorType.ChargingRangeSelector:
				_rangeSelector = new ChargingRangeSelector(Layers.HumanCorpse.ToMask());
				break;
			case RangeSelectorType.FrontSquareRangeSelector:
				_rangeSelector = new FrontSquareRangeSelector(Layers.HumanCorpse.ToMask());
				break;
			case RangeSelectorType.FrontCircleRangeSelector:
				_rangeSelector = new FrontCircleRangeSelector(Layers.HumanCorpse.ToMask());
				break;
			case RangeSelectorType.PointerCircleRangeSelector:
				_rangeSelector = new PointerCircleRangeSelector(Layers.HumanCorpse.ToMask());
				break;
			case RangeSelectorType.UnderfootRangeSelector:
				_rangeSelector = new UnderfootRangeSelector();
				break;
		}
	}



	public override void OnBattleStart() { }
	public override void OnBattleEnd() { }
	public override void OnSkillUpgrade() { }

	public override void OnSkillAttained()
	{
	}
}