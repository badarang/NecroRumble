using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using LOONACIA.Unity.Pool;
using UnityEngine.PlayerLoop;
using TMPro;
using UnityEngine.Serialization;

public class GameUIManager : MonoBehaviour
{
	UIOverlayCanvasScene _UIOverlayCanvasScene;
	UIRewardPopup _UIRewardPopup;
	UIPausePopup _UIPausePopup;
	UIDefeatPopup _UIDefeatPopup;
	UIWinPopup _UIWinPopup;
		
	UIKeymapPopup _uIKeymapPopup;
	UISettingPopup _uISettingsPopup;
	private UIScene _UIDeviceScene;
	UIUnitExpPanel _UIUnitExpPanel;
	public List<Vector2> UnitExpPanelPosList = new List<Vector2>();

	public bool IsReward { get; set; } = false;
	public bool IsKeymapPopup { get; set; } = false;
	public bool IsSettingsPopup { get; set; } = false;

	public static bool DisplayHPBar = true;
	Player _player;

	public static GameUIManager Instance;
	private void Awake()
	{
		#region Singleton
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else
		{
			Destroy(this.gameObject);
		}
		#endregion

	}

	public void Init()
	{
		_player = GameManager.Instance.GetPlayer();
		SetUndeadNumUI();

		if (_UIUnitExpPanel != null)
		{
			ManagerRoot.Resource.Release(_UIUnitExpPanel.gameObject);
		}
		_UIUnitExpPanel = ManagerRoot.UI.ShowSceneUI<UIUnitExpPanel>(); // todo 구조 개선 필요
		DisplayHPBar = true;
	}

	public void Clear() { }

	public void ToggleDisplayHPbar()
	{
#if UNITY_EDITOR
		if (ManagerRoot.Input.DisplayUnitHpButton)
		{
			DisplayHPBar = !DisplayHPBar;
			Debug.Log("display hp bar " + DisplayHPBar);
		}
#endif
	}

	// todo: 구조개선 필요 with UIRewardPopup
	public int _rewardRank = 2;
	public int _rewardDispersion = 2;
	public void ShowRewardPopupByRankDispersion(int rank_, int dispersion_, UnitType unitType_ = UnitType.None)
	{
		IsReward = true;
		//시간 멈추기 (UIRewardButton의 OnSubmit에서 해제)
		Time.timeScale = 0;


		_rewardRank = rank_;
		_rewardDispersion = dispersion_;
		
		
		//UIRewardPopup에 어떤 유닛 표시해야하는지 전달
		UIRewardPopup.upgradedUnit = unitType_;
		_UIRewardPopup = ManagerRoot.UI.ShowPopupUI<UIRewardPopup>();
		ManagerRoot.Input.EnableUIMode();
	}

	public void SetPauseScreen(bool pause_)
	{
		if (pause_)
		{
			_UIPausePopup = ManagerRoot.UI.ShowPopupUI<UIPausePopup>();
			GameManager.Instance.GetPlayer().CanAction = false;
			ManagerRoot.Input.EnableUIMode();
		}
		else
		{
			//ManagerRoot.UI.ClosePopupUI(_UIPausePopup);
			ManagerRoot.UI.ClearAllPopup( );

			GameManager.Instance.GetPlayer().CanAction = true;
			ManagerRoot.Input.DisableUIMode();
		}
	}
	public void ShowSettingsPopup(UIBase uIBase_)
	{
		Debug.Log($"GameUIManager | ShowSettingsPopup | uIBase_:{uIBase_}");
		
		IsSettingsPopup = true;
		_uISettingsPopup = ManagerRoot.UI.ShowPopupUI<UISettingPopup>(usePool: false);
		_uISettingsPopup.SetParentPopup(uIBase_);
	}

	public void CloseSettingsPopup()
	{
		Debug.Log($"GameUIManager | CloseSettingsPopup | IsSettingsPopup:{IsSettingsPopup}");
		
		if (IsSettingsPopup)
		{
			ManagerRoot.UI.ClosePopupUI(_uISettingsPopup);
			_uISettingsPopup= null;
			IsSettingsPopup = false;
		}
	}

	public void ShowKeymapPopup(UIBase uIBase_)
	{
		Debug.Log($"GameUIManager | ShowKeymapPopup | uIBase_:{uIBase_}");
		
		IsKeymapPopup = true;
		_uIKeymapPopup = ManagerRoot.UI.ShowPopupUI<UIKeymapPopup>(usePool: false);
		_uIKeymapPopup.SetParentPopup(uIBase_);
	}

	public void CloseKeymapPopup()
	{
		Debug.Log($"GameUIManager | CloseKeymapPopup | IsKeymapPopup:{IsKeymapPopup}");
		
		if (IsKeymapPopup)
		{
			ManagerRoot.UI.ClosePopupUI(_uIKeymapPopup);
			IsKeymapPopup = false;
		}

	}

	public void SetDefeatScreen(bool defeat_)
	{
		if (defeat_)
		{
			_UIDefeatPopup = ManagerRoot.UI.ShowPopupUI<UIDefeatPopup>();
			GameManager.Instance.GetPlayer().CanAction = false;
			ManagerRoot.Input.EnableUIMode();
		}
		else
		{
			ManagerRoot.UI.ClosePopupUI(_UIDefeatPopup);
			GameManager.Instance.GetPlayer().CanAction = true;
			ManagerRoot.Input.DisableUIMode();
		}
	}
	public void SetWinScreen(bool win_)
	{
		if (win_)
		{
			_UIWinPopup = ManagerRoot.UI.ShowPopupUI<UIWinPopup>();
			GameManager.Instance.GetPlayer().CanAction = false;
			ManagerRoot.Input.EnableUIMode();
		}
		else
		{
			ManagerRoot.UI.ClosePopupUI(_UIWinPopup);
			GameManager.Instance.GetPlayer().CanAction = true;
			ManagerRoot.Input.DisableUIMode();
		}
	}

	public void SetDeviceChangePopup()
	{
		if (_UIDeviceScene != null)
		{
			Destroy(_UIDeviceScene.gameObject);
		}
		_UIDeviceScene = ManagerRoot.UI.ShowSceneUI<UIDeviceScene>();

	}

	public void CloseDeviceChangedPopup()
	{
		if (_UIDeviceScene == null)
		{
			return;
		}

		Destroy(_UIDeviceScene.gameObject);

	}

	public void CreateNumberBalloon(string text, Vector3 createPos, Color32 color, float fontSize = 1f, float duration = 1.5f)
	{
		var newBalloon = ManagerRoot.UI.ShowSceneUI<UINumberBalloon>();
		newBalloon.Duration = duration;
		newBalloon.SetInitialPosition(new Vector3(createPos.x, createPos.y + 2f, 0f));
		newBalloon.SetText(text.ToString(), color);
		newBalloon.SetSize(fontSize);
	}


	public void SetHpUI(float _curHp, float maxHp)
	{
		if (_UIOverlayCanvasScene == null)
		{
			_UIOverlayCanvasScene = FindAnyObjectByType<UIOverlayCanvasScene>();
		}
		_UIOverlayCanvasScene?.SetHp(_curHp, maxHp);
	}
	public void SetMpUI(float curMp_, float maxMp_)
	{
		if (_UIOverlayCanvasScene == null)
		{
			_UIOverlayCanvasScene = FindAnyObjectByType<UIOverlayCanvasScene>();
		}
		// _UIOverlayCanvasScene?.SetMp(curMp_, maxMp_);
	}
	public void SetHpShakeUI(float power_, float duration_)
	{
		if (_UIOverlayCanvasScene == null)
		{
			_UIOverlayCanvasScene = FindAnyObjectByType<UIOverlayCanvasScene>();
		}
		_UIOverlayCanvasScene?.SetShakeHp(power_, duration_);
	}
	public void SetMpShakeUI(float power_, float duration_)
	{
		if (_UIOverlayCanvasScene == null)
		{
			_UIOverlayCanvasScene = FindAnyObjectByType<UIOverlayCanvasScene>();
		}
		// _UIOverlayCanvasScene?.SetShakeMp(power_, duration_);
	}
	
	public void SetRecallCooldownUI()
	{
		if (_UIOverlayCanvasScene == null)
		{
			_UIOverlayCanvasScene = FindAnyObjectByType<UIOverlayCanvasScene>();
		}
		_UIOverlayCanvasScene?.SetRecallCooldown();
	}
	
	public void SetReviveCooldownUI()
	{
		if (_UIOverlayCanvasScene == null)
		{
			_UIOverlayCanvasScene = FindAnyObjectByType<UIOverlayCanvasScene>();
		}
		_UIOverlayCanvasScene?.SetReviveCooldown();
	}
	
	public void SetSpearCooldownUI()
	{
		if (_UIOverlayCanvasScene == null)
		{
			_UIOverlayCanvasScene = FindAnyObjectByType<UIOverlayCanvasScene>();
		}
		_UIOverlayCanvasScene?.SetSpearCooldown();
	}
	
	public void SetGrindCooldownUI()
	{
		if (_UIOverlayCanvasScene == null)
		{
			_UIOverlayCanvasScene = FindAnyObjectByType<UIOverlayCanvasScene>();
		}
		_UIOverlayCanvasScene?.SetGrindCooldown();
	}


	public void SetTimeUI(string time_)
	{
		if (_UIOverlayCanvasScene == null)
		{
			_UIOverlayCanvasScene = FindAnyObjectByType<UIOverlayCanvasScene>();
		}
		_UIOverlayCanvasScene?.SetTimeText(time_);
	}

	public TMP_Text GetTimeText()
	{
		if (_UIOverlayCanvasScene == null)
		{
			_UIOverlayCanvasScene = FindAnyObjectByType<UIOverlayCanvasScene>();
		}

		if (_UIOverlayCanvasScene == null) return null;


		return _UIOverlayCanvasScene.GetTimeText();
	}

	public void SetDarkEssenceUI(uint darkEssence_)
	{
		// if (_UIOverlayCanvasScene == null)
		// {
		// 	_UIOverlayCanvasScene = FindAnyObjectByType<UIOverlayCanvasScene>();
		// }
		// _UIOverlayCanvasScene?.SetDarkEssenceText(darkEssence_);
	}
	public void SetUndeadNumUI(bool isRedColor = false)
	{
		if (_UIOverlayCanvasScene == null)
		{
			_UIOverlayCanvasScene = FindAnyObjectByType<UIOverlayCanvasScene>();
		}
		int curNum = ManagerRoot.Unit.CountUndeadUnit();
		int maxNum = GameManager.Instance.MaxUndeadNum;
		_UIOverlayCanvasScene?.SetUndeadNumText(curNum, maxNum, isRedColor);
		if (isRedColor)
		{
			_UIOverlayCanvasScene?.SetUndeadNumDoScale();
		}
	}
    
	public void SetHpBarDoScale()
	{
		if (_UIOverlayCanvasScene == null)
		{
			_UIOverlayCanvasScene = FindAnyObjectByType<UIOverlayCanvasScene>();
		}

		_UIOverlayCanvasScene?.SetHpBarDoScale();
	}
	
	public void SetRecallDoScale()
	{
		if (_UIOverlayCanvasScene == null)
		{
			_UIOverlayCanvasScene = FindAnyObjectByType<UIOverlayCanvasScene>();
		}

		_UIOverlayCanvasScene?.SetRecallDoScale();
	}
	
	public void SetReviveDoScale(int lateMultiplier = 1)
	{
		if (_UIOverlayCanvasScene == null)
		{
			_UIOverlayCanvasScene = FindAnyObjectByType<UIOverlayCanvasScene>();
		}

		_UIOverlayCanvasScene?.SetReviveDoScale(lateMultiplier);
	}
    
	
	public void SetSpearDoScale()
	{
		if (_UIOverlayCanvasScene == null)
		{
			_UIOverlayCanvasScene = FindAnyObjectByType<UIOverlayCanvasScene>();
		}

		_UIOverlayCanvasScene?.SetSpearDoScale();
	}
	
	public void SetGrindDoScale()
	{
		if (_UIOverlayCanvasScene == null)
		{
			_UIOverlayCanvasScene = FindAnyObjectByType<UIOverlayCanvasScene>();
		}

		_UIOverlayCanvasScene?.SetGrindDoScale();
	}

	public void UpdateLvExpInfo(UnitType unitType_, int deltaLevel_, int deltaExp_)
	{
		if (_UIUnitExpPanel == null)
		{
			_UIUnitExpPanel = FindAnyObjectByType<UIUnitExpPanel>();
		}

		_UIUnitExpPanel.UpdateInfo(unitType_, deltaLevel_, deltaExp_);
	}
	
	public void AddUnitExpPanelPosList(Vector2 pos_)
	{
		UnitExpPanelPosList.Add(pos_);
	}

	public void SetExpPanelBlink(int unitType_, int blinkCount_)
	{
		if (_UIUnitExpPanel == null)
		{
			_UIUnitExpPanel = FindAnyObjectByType<UIUnitExpPanel>();
		}

		_UIUnitExpPanel.SetUIBlink(unitType_, blinkCount_);
	}
	
	// public void SetKeyMapInfos(bool isActive_)
	// {
	// 	_UIOverlayCanvasScene.SetKeyMapInfos(isActive_);
	// }

	public void SetSkillCoolDownText(int mode = 0)
	{
		if (_UIOverlayCanvasScene == null)
		{
			_UIOverlayCanvasScene = FindAnyObjectByType<UIOverlayCanvasScene>();
		}

		_UIOverlayCanvasScene?.SetSkillCoolDownText(mode);
	}
}
