using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;

public class InputManager
{
	#region Vector2 Input
	public float MoveHorizontal { get; private set; }
	public float MoveVertical   { get; private set; }
	public bool  MoveButton     { get; private set; }
	public float AimHorizontal  { get; private set; }
	public float AimVertical    { get; private set; }
	public bool  AimButton      { get; private set; }
	#endregion
	#region Button Input
	public bool PauseButton { get; private set; }
	public bool   JumpButton { get; private set; }
	public bool AttackButton { get; private set; }
	public bool   DashButton { get; private set; }
	public bool ReviveButton { get; private set; }
	public bool BoneSpearButton { get; private set; }
	// public bool CorpseAbsorptionButton { get; private set; }
	public bool RecallOrderButton { get; private set; }
	// public bool PortalButton { get; private set; }
	public bool DisplayUnitHpButton { get; private set; }
	public bool GrindButton { get; private set; }

	// Debug
	public bool ToggleButton { get; private set; }
	public bool ReturnButton { get; private set; }

	// UI
	public bool SubmitButton { get; private set; }

	#endregion
	#region User Callback functions
	// public Action OnJumpPressed;
	// public Action OnJumpReleased;
	public Action OnPausePressed;
	public Action OnPauseReleased;
	// public Action OnAttackPressed;
	// public Action OnAttackReleased;
	// public Action OnDashPressed;
	// public Action OnDashReleased;
	public Action OnRevivePressed;
	public Action OnReviveReleased;
	public Action OnBoneSpearPressed;
	public Action OnBoneSpearReleased;
	// public Action OnCorpseAbsorptionPressed;
	// public Action OnCorpseAbsorptionReleased;
	public Action OnRecallOrderPressed;
	public Action OnRecallOrderReleased;
	// public Action OnPortalPressed;
	// public Action OnPortalReleased;
	public Action OnDisplayUnitHpPressed;
	public Action OnDisplayUnitHpReleased;
	public Action OnGrindPressed;
	public Action OnGrindReleased;

	// debug
	public Action OnTogglePressed;
	public Action OnToggleReleased;
	public Action OnReturnPressed;
	public Action OnReturnReleased;

	// UI
	public Action OnSubmitPressed;
	public Action OnSubmitReleased;

	private InputActions _InputActions;
	private InputAction _moveAction;
	private InputAction _pauseAction;
	private InputAction _aimAction;
	// private InputAction _jumpAction;
	// private InputAction _attackAction;
	// private InputAction _dashAction;
	private InputAction _reviveAction;
	private InputAction _boneSpearAction;
	// private InputAction _corpseAbsorptionAction;
	private InputAction _recallOrderAction;
	// private InputAction _portalAction;
	private InputAction _displayUnitHpAction;
	private InputAction _grindAction;
	
	// debug
	private InputAction _toggleAction;
	private InputAction _returnAction;

	// UI
	private InputAction _submitAction;


	#endregion
	public Gamepad gamepad;
	private DeviceChangeListener.ControlDeviceType _lastUsedInputType;

	public DeviceChangeListener.ControlDeviceType LastUsedInputType
	{
		get => _lastUsedInputType;
		set
		{
			_lastUsedInputType = value;
		}
	}
	

	public void Init()
	{
		MoveHorizontal = 0;
		MoveVertical   = 0;
		AimHorizontal  = 0;
		AimVertical    = 0;
		JumpButton     = false;
		AttackButton   = false;
		ReviveButton   = false;
		BoneSpearButton   = false;
		// CorpseAbsorptionButton   = false;
		RecallOrderButton   = false;
		// PortalButton   = false;
		DisplayUnitHpButton   = false;
		GrindButton   = false;

		ToggleButton   = false;
		ReturnButton   = false;

		SubmitButton   = false;

	   _InputActions  = new InputActions();
		_moveAction    = _InputActions.Player.Move;
		_aimAction     = _InputActions.Player.Aim;

		_pauseAction  = _InputActions.Player.Pause;
		_reviveAction  = _InputActions.Player.Revive;
		_boneSpearAction  = _InputActions.Player.BoneSpear;
		// _corpseAbsorptionAction  = _InputActions.Player.CorpseAbsorption;
		_recallOrderAction  = _InputActions.Player.RecallOrder;
		// _portalAction  = _InputActions.Player.Portal;
		_displayUnitHpAction  = _InputActions.Player.DisplayUnitHp;
		_grindAction  = _InputActions.Player.Grind;


		// _jumpAction = _InputActions.Player.Jump;
		// _attackAction = _InputActions.Player.Attack;
		// _dashAction = _InputActions.Player.Dash;

		// Debug
		_toggleAction  = _InputActions.Debug.Toggle;
		_returnAction  = _InputActions.Debug.Return;

		// UI
		_submitAction  = _InputActions.UI.Submit;

		EnableActions();
		EnableUIActions();
		EnableDebugActions();

		gamepad = Gamepad.current;
	}

	public void Clear()
	{
		DisableActions();
		DisableUIActions();
		DisableDebugActions();
	}

	// Call When Enter UI Popup
	public void EnableUIMode()
	{
		Debug.Log( $"InputManager | Enable UI Mode" );

		DisableActions();
		EnableUIActions();
	}
	public void DisableUIMode()
	{
		Debug.Log( $"InputManager | Disable UI Mode" );

		EnableActions();
		DisableUIActions();
	}
	public void EnableDebugMode()
	{
		DisableActions();
		DisableUIActions();
	}
	public void DisableDebugMode()
	{
		EnableActions();
		EnableUIActions();
	}

	public void EnableDebugActions() // debug controller에서 on
	{
		Debug.Log( $"InputManager | Enable Debug Actions" );

		LinkEnableNonInteractionAction(_toggleAction,   OnToggle,  OnCancelToggle);
		LinkEnableNonInteractionAction(_returnAction,   OnReturn,  OnCancelReturn);
	}
	public void DisableDebugActions()
	{
		Debug.Log( $"InputManager | Disable Debug Actions" );

		UnlinkDisableNonInteractionAction(_toggleAction, OnToggle, OnCancelToggle);
		UnlinkDisableNonInteractionAction(_returnAction, OnReturn, OnCancelReturn);
	}

	private void EnableUIActions()
	{
		Debug.Log( $"InputManager | Enable UI Actions" );

		if (EventSystem.current.TryGetComponent<InputSystemUIInputModule>(out var inputModule))
		{
			inputModule.enabled = true;
		}

		//LinkEnableNonInteractionAction(_submitAction,   OnSubmit,  OnCancelSubmit);
	}
	private void DisableUIActions()
	{
		Debug.Log( $"InputManager | Disable UI Actions" );

		if (EventSystem.current.TryGetComponent<InputSystemUIInputModule>(out var inputModule))
		{
			inputModule.enabled = false;
		}

		//UnlinkDisableNonInteractionAction(_submitAction, OnSubmit, OnCancelSubmit);
	}

	private void EnableActions()
	{
		Debug.Log( $"InputManager | Enable Game Actions" );

		LinkEnableNonInteractionAction(_moveAction,   OnMove,   OnCancelMove);
		LinkEnableNonInteractionAction(_aimAction,    OnAim,    OnCancelAim);
		LinkEnableNonInteractionAction(_pauseAction, OnPause, OnCancelPause);
		LinkEnableNonInteractionAction(_reviveAction, OnRevive, OnCancelRevive);
		LinkEnableNonInteractionAction(_boneSpearAction, OnBoneSpear, OnCancelBoneSpear);
		// LinkEnableNonInteractionAction(_corpseAbsorptionAction, OnCorpseAbsorption, OnCancelCorpseAbsorption);
		LinkEnableNonInteractionAction(_recallOrderAction, OnRecallOrder, OnCancelRecallOrder);
		// LinkEnableNonInteractionAction(_portalAction, OnPortal, OnCancelPortal);
		LinkEnableNonInteractionAction(_displayUnitHpAction, OnDisplayUnitHp, OnCancelDisplayUnitHp);
		LinkEnableNonInteractionAction(_grindAction, OnGrind, OnCancelGrind);
		// LinkEnableNonInteractionAction(_jumpAction, OnJump, OnCancelJump);
		// LinkEnableNonInteractionAction(_attackAction, OnAttack, OnCancelAttack);
		// LinkEnableNonInteractionAction(_dashAction, OnDash, OnCancelDash);
	}
	private void DisableActions()
	{
		Debug.Log( $"InputManager | Disable Game Actions" );

		UnlinkDisableNonInteractionAction(_moveAction,   OnMove,   OnCancelMove);
		UnlinkDisableNonInteractionAction(_aimAction,    OnAim,    OnCancelAim);
		UnlinkDisableNonInteractionAction(_pauseAction, OnPause, OnCancelPause);
		UnlinkDisableNonInteractionAction(_reviveAction, OnRevive, OnCancelRevive);
		UnlinkDisableNonInteractionAction(_boneSpearAction, OnBoneSpear, OnCancelBoneSpear);
		// UnlinkDisableNonInteractionAction(_corpseAbsorptionAction, OnCorpseAbsorption, OnCancelCorpseAbsorption);
		UnlinkDisableNonInteractionAction(_recallOrderAction, OnRecallOrder, OnCancelRecallOrder);
		// UnlinkDisableNonInteractionAction(_portalAction, OnPortal, OnCancelPortal);
		UnlinkDisableNonInteractionAction(_displayUnitHpAction, OnDisplayUnitHp, OnCancelDisplayUnitHp);
		UnlinkDisableNonInteractionAction(_grindAction, OnGrind, OnCancelGrind);
		// UnlinkDisableNonInteractionAction(_jumpAction, OnJump, OnCancelJump);
		// UnlinkDisableNonInteractionAction(_attackAction, OnAttack, OnCancelAttack);
		// UnlinkDisableNonInteractionAction(_dashAction, OnDash, OnCancelDash);
	}
	public IEnumerator DisableInputDeltaTime(float deltaTime_){
		DisableActions();
		yield return new WaitForSeconds(deltaTime_);
		EnableActions();
	}
	#region Util functions for InputManager
	private void LinkEnableNonInteractionAction(
		InputAction action,
		Action<InputAction.CallbackContext> callback,
		Action<InputAction.CallbackContext> cancelCallback)
	{
		action.Enable();
		LinkNonInteractionAction(action, callback, cancelCallback);
	}
	private void LinkNonInteractionAction(
		InputAction action,
		Action<InputAction.CallbackContext> callback,
		Action<InputAction.CallbackContext> cancelCallback)
	{
		action.performed -= callback;
		action.performed += callback;
		action.canceled  -= cancelCallback;
		action.canceled  += cancelCallback;
	}
	private void UnlinkDisableNonInteractionAction(
		InputAction action,
		Action<InputAction.CallbackContext> callback,
		Action<InputAction.CallbackContext> cancelCallback)
	{
		action.Disable();
		UnlinkNonInteractionAction(action, callback, cancelCallback);
	}
	private void UnlinkNonInteractionAction(
		InputAction action,
		Action<InputAction.CallbackContext> callback,
		Action<InputAction.CallbackContext> cancelCallback)
	{
		action.performed -= callback;
		action.performed += cancelCallback;
	}
	#endregion
	#region Callback functions for axis input (Vector2)
	private void OnMove(InputAction.CallbackContext context)
	{
		MoveHorizontal = context.ReadValue<Vector2>().x;
		MoveVertical   = context.ReadValue<Vector2>().y;
		if (!AimButton)
		{
			AimHorizontal = context.ReadValue<Vector2>().x;
			AimVertical   = context.ReadValue<Vector2>().y;
		}
		MoveButton = true;
	}
	private void OnCancelMove(InputAction.CallbackContext context)
	{
		MoveHorizontal = 0;
		MoveVertical   = 0;
		if(!AimButton)
		{
			AimHorizontal = 0;
			AimVertical   = 0;
		}
		MoveButton = false;
	}
	private void OnPause(InputAction.CallbackContext context)
	{
		Debug.Log("InputManager | OnPause");
		PauseButton = true;
		OnPausePressed?.Invoke();
	}
	private void OnCancelPause(InputAction.CallbackContext context)
	{
		PauseButton = false;
		OnPauseReleased?.Invoke();
	}
	private void OnRevive(InputAction.CallbackContext context)
	{
		//Debug.Log("OnRevive");
		ReviveButton = true;
		OnRevivePressed?.Invoke();
	}
	private void OnCancelRevive(InputAction.CallbackContext context)
	{
		ReviveButton = false;
		OnReviveReleased?.Invoke();
	}
	private void OnBoneSpear(InputAction.CallbackContext context)
	{
		//Debug.Log("OnBoneSpear");
		BoneSpearButton = true;
		OnBoneSpearPressed?.Invoke();
	}
	private void OnCancelBoneSpear(InputAction.CallbackContext context)
	{
		BoneSpearButton = false;
		OnBoneSpearReleased?.Invoke();
	}
	// private void OnCorpseAbsorption(InputAction.CallbackContext context)
	// {
	//     //Debug.Log("OnCorpseAbsorption");
	//     CorpseAbsorptionButton = true;
	//     OnCorpseAbsorptionPressed?.Invoke();
	// }
	// private void OnCancelCorpseAbsorption(InputAction.CallbackContext context)
	// {
	//     CorpseAbsorptionButton = false;
	//     OnCorpseAbsorptionReleased?.Invoke();
	// }
	private void OnRecallOrder(InputAction.CallbackContext context)
	{
		//Debug.Log("OnRecallOrder");
		RecallOrderButton = true;
		OnRecallOrderPressed?.Invoke();
	}
	private void OnCancelRecallOrder(InputAction.CallbackContext context)
	{
		RecallOrderButton = false;
		OnRecallOrderReleased?.Invoke();
	}
	// private void OnPortal(InputAction.CallbackContext context)
	// {
	//     //Debug.Log("OnPortal");
	//     PortalButton = true;
	//     OnPortalPressed?.Invoke();
	// }
	// private void OnCancelPortal(InputAction.CallbackContext context)
	// {
	//     PortalButton = false;
	//     OnPortalReleased?.Invoke();
	// }
	private void OnDisplayUnitHp(InputAction.CallbackContext context)
	{
		// Debug.Log("OnDisplayUnitHp");
		DisplayUnitHpButton = true;
		OnDisplayUnitHpPressed?.Invoke();
	}
	private void OnCancelDisplayUnitHp(InputAction.CallbackContext context)
	{
		DisplayUnitHpButton = false;
		OnDisplayUnitHpReleased?.Invoke();
	}
	private void OnGrind(InputAction.CallbackContext context)
	{
		// Debug.Log("OnGrind");
		GrindButton = true;
		OnGrindPressed?.Invoke();
	}
	private void OnCancelGrind(InputAction.CallbackContext context)
	{
		GrindButton = false;
		OnGrindReleased?.Invoke();
	}

	private void OnAim(InputAction.CallbackContext context)
	{
		AimHorizontal = context.ReadValue<Vector2>().x;
		AimVertical   = context.ReadValue<Vector2>().y;
		AimButton     = true;
	}
	private void OnCancelAim(InputAction.CallbackContext context)
	{
		AimHorizontal = 0;
		AimVertical = 0;
		AimButton = false;
	}

	// Debug
	private void OnToggle(InputAction.CallbackContext context)
	{
		Debug.Log("InputManager | OnDebugToggle");
		ToggleButton = true;
		OnTogglePressed?.Invoke();
	}
	private void OnCancelToggle(InputAction.CallbackContext context)
	{
		ToggleButton = false;
		OnToggleReleased?.Invoke();
	}

	private void OnReturn(InputAction.CallbackContext context)
	{
		Debug.Log("InputManager | OnDebugReturn");
		ReturnButton = true;
		OnReturnPressed?.Invoke();
	}
	private void OnCancelReturn(InputAction.CallbackContext context)
	{
		ReturnButton = false;
		OnReturnReleased?.Invoke();
	}

	// UI
	private void OnSubmit(InputAction.CallbackContext context)
	{
		SubmitButton = true;
		OnSubmitPressed?.Invoke();
	}
	private void OnCancelSubmit(InputAction.CallbackContext context)
	{
		SubmitButton = false;
		OnSubmitReleased?.Invoke();
	}

	// private void OnJump(InputAction.CallbackContext context)
	// {
	//     JumpButton = true;
	//     OnJumpPressed?.Invoke();
	// }
	// private void OnCancelJump(InputAction.CallbackContext context)
	// {
	//     JumpButton = false;
	//     OnJumpReleased?.Invoke();
	// }
	// private void OnAttack(InputAction.CallbackContext context)
	// {
	//     AttackButton = true;
	//     OnAttackPressed?.Invoke();
	// }
	// private void OnCancelAttack(InputAction.CallbackContext context)
	// {
	//     AttackButton = false;
	//     OnAttackReleased?.Invoke();
	// }
	// private void OnDash(InputAction.CallbackContext context)
	// {
	//     DashButton = true;
	//     OnDashPressed?.Invoke();
	// }
	// private void OnCancelDash(InputAction.CallbackContext context)
	// {
	//     DashButton = false;
	//     OnDashReleased?.Invoke();
	// }


	#endregion
	
	/// <summary>
	/// Active Gamepad's vibration with Intensity and Duration
	/// </summary>
	/// <param name="duration"> (min) 0.1f ~ (max) 5.0f, time per try </param>
	/// <param name="intensity"> (min) 0.1f ~ (max) 1.0f, default = 0.5, off at 0 </param>
	public void Vibration(float intensity = 0.5f, float duration = 0.2f, bool isAdvance = false, bool isHeavy = false)
	{
		if (isAdvance)
		{
			CoroutineHandler.StartCoroutine(StartVibration(isHeavy, duration, intensity));
		}
		else
		{
			CoroutineHandler.StartCoroutine(StartVibration(duration, intensity));
		}
	}
	
	public void StopVibration()
	{
		gamepad?.SetMotorSpeeds(0, 0);
	}
	
	IEnumerator StartVibration(float duration_, float intensity_)
	{
		gamepad?.SetMotorSpeeds(intensity_, intensity_);
		yield return new WaitForSecondsRealtime(duration_);
		StopVibration();
	}

	IEnumerator StartVibration(bool isHeavy, float duration_, float intensity_)
	{
		if (isHeavy)
		{
			gamepad?.SetMotorSpeeds(intensity_, intensity_ / 3);
		}
		else
		{
			gamepad?.SetMotorSpeeds(intensity_ / 3, intensity_);
		}
		yield return new WaitForSecondsRealtime(duration_);
		StopVibration();
	}
	
}