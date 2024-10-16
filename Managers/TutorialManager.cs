using System;
using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
	private CameraController _cameraController;
	private DeviceChangeListener _deviceChangeListener => FindObjectOfType<DeviceChangeListener>();
	
	TutorialScene _tutorialScene;
	PortalController _portalController;
	Player _player;
	BoneSpear _boneSpear;
	GrindUnit _grindUnit;

	private Vector2 _zoomPos;
	private Transform _zoomTarget;
	private UIText _guideText;
	private float _zoomDuration = 2f;
	private bool _isUnitStop = false;
	//private UIImage _curImage;
	private List<UIImage> _curImages = new List<UIImage>();

	ImageData _imageData;
	Sprite _sprite;

	// current stored 
	ImageData.SituationType _situationType;
	Transform _transform;
	Vector2 _offset;
	float _duration;

	public bool isShowGuideImage = false;
	public bool IsPortalReady    = false;

	public enum EnumTutorialState
	{
		Init,
		BoneSpearEvent,
		TurnUndeadEvent,
		SecondManComeEvent,
		SecondManKillEvent,
		GrindEvent,
		OpenBoxEvent,
		ShowPortalEvent,
		CompleteTutorial,
	}
	public EnumTutorialState state = EnumTutorialState.Init;
	public bool completeBoneSpearEvent = false;
	public bool completeTurnUndeadEvent = false;
	public bool completeSecondManComeEvent = false;
	public bool completeShowPortalEvent = false;
	public bool completeSecondManKill = false;
	public bool completeTutorial = false;

	[SerializeField] private GameObject _chest;

	Unit _firstUnit;
	Unit _secondUnit;

	public bool IsUnitStop
	{
		get => _isUnitStop;
		set => _isUnitStop = value;
	}
	public static TutorialManager Instance;
	private void Awake()
	{
		#region Singleton
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(this.gameObject);
		}
		#endregion

	}

	public void Init()
	{
		Debug.Log("TutorialManager Init");

		if( Camera.main.TryGetComponent(out CameraController cameraController_) )
		{
			_cameraController = cameraController_;   
		}else{
			Debug.LogWarning("CameraController is null");
		}

		_imageData = ManagerRoot.Resource.Load<ImageData>("Data/UI/ImageData");

		_tutorialScene = SceneManagerEx.CurrentScene as TutorialScene;
		_portalController = _tutorialScene.portalController;
		GameObject portal = ManagerRoot.Resource.Load<GameObject>("Prefabs/Effects/@Portal");
		_portalController = Instantiate(portal).GetComponent<PortalController>();
		_portalController.transform.position = Statics.PortalPosition;
		_portalController.gameObject.SetActive(false);
		_deviceChangeListener.evtDeviceChanged += Reshow;
		
		_player = GameManager.Instance.GetPlayer();
		_boneSpear = _player.gameObject.GetComponent<BoneSpear>();
		_grindUnit = _player.gameObject.GetComponent<GrindUnit>();
		_grindUnit.isFirstGrind = false;
		UIRewardPopup.isRewardSelected = false;
		_chest.SetActive(false);

		state = EnumTutorialState.Init;
		ManagerRoot.Unit.InstantiateUnit(UnitType.ArcherMan, new Vector3(-4, 9, 0), Faction.Human, out var unit);
		
		StatModifier mod1 = new StatModifier(StatType.AttackDamage, "TutorialModifier1", -80f, StatModifierType.Percentage, false);
		ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(unit, mod1);
		
		StatModifier mod2 = new StatModifier(StatType.MaxHp, "TutorialModifier2", -80f, StatModifierType.Percentage, false);
		ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(unit, mod2);
		
		unit.AddComponent<TutorialUnitController>();
	}

	public void Clear()
	{
		if( _deviceChangeListener ){
			_deviceChangeListener.evtDeviceChanged -= Reshow;
		}
	}


	void Update()
	{
		switch (state)
		{
			case EnumTutorialState.Init:
				
				_firstUnit = GetNearestHuman(7f);
				if (_firstUnit != null)
				{
					ShowThrowBoneSpearTutorial(_firstUnit.transform.position);
					// IsBoneSpearTutorialAlreadyStarted = true;
					state = EnumTutorialState.BoneSpearEvent;
				}
				break;

			case EnumTutorialState.BoneSpearEvent:
				BoneSpearEvent();

				break;

			// 첫번째 사람이 죽은 상황
			case EnumTutorialState.TurnUndeadEvent:

				// 첫번쨰 사람에게 TurnUndead()를 실행했다는 것
				FirstEncounterEvent();
				break;

			case EnumTutorialState.SecondManComeEvent:
				
				if (_secondUnit.IsDead)
				{
					ShowImage(ImageData.SituationType.CorpseRevive, _secondUnit.transform, new Vector2(0f, 2f), 10f);
					state = EnumTutorialState.SecondManKillEvent;
				}
				break;

			// 두번쨰 사람이 죽은 상황
			case EnumTutorialState.SecondManKillEvent:

				if (!_secondUnit.IsDead)
				{
					state = EnumTutorialState.GrindEvent;
				}
				break;

			case EnumTutorialState.GrindEvent:
				// 머리 위에 그라인드 키 출력
				// 안 죽은애 머리 위에 뜨게
				if (_firstUnit != null & !_firstUnit.IsDead)
				{
					HideImage();
					ShowImage(ImageData.SituationType.Grind, _firstUnit.transform, new Vector2(0f, 2f), 0f);
					ShowImage(ImageData.SituationType.Grind, _secondUnit.transform, new Vector2(0f, 2f), 0f);
				}

				// 이후 그라인드 실행 부분에서 이벤트 지속
				StartEventGrind();

				// 그라인드 이벤트 완료
				if (_grindUnit.isFirstGrind)
				{
					HideImage();
				}
				// 유닛 강화 리워드 창 선택 완료
				if (UIRewardPopup.isRewardSelected)
				{
					StartEventTutorialEnd();
					state = EnumTutorialState.OpenBoxEvent;
				}
				break;
			case EnumTutorialState.OpenBoxEvent:
				
				StartOpenBoxEvent();
				
				break;
			case EnumTutorialState.ShowPortalEvent:
				HideImage();

				_portalController.gameObject.SetActive(true);
				
				state = EnumTutorialState.CompleteTutorial;
				break;

			case EnumTutorialState.CompleteTutorial:
				if (_portalController.isSuckInDone)
				{
					//포탈 타면 튜토리얼 끝
					SceneManagerEx.LoadScene("Title");
					ManagerRoot.Settings.ClearTutorial();
					ManagerRoot.Settings.SaveOptions();
				}
				break;

			default:
				break;
		}
	}

	void Reshow()
	{
		if( isShowGuideImage ) // todo null처리 개선 요망
		{
			HideImage();
			ShowImage( _situationType, _transform, _offset, _duration);
		}
	}

	public void ChangeZoomPos(Vector2 pos)
	{
		if (_cameraController == null)
		{
			Debug.LogWarning("CameraController is null");
			return;
		}
		_cameraController.ZoomTarget = Instantiate(new GameObject(), pos, Quaternion.identity).transform;
		_zoomTarget = _cameraController.ZoomTarget;
		StartCoroutine(ZoomOut());
		
	}
	
	IEnumerator ZoomOut()
	{
		yield return new WaitForSeconds(_zoomDuration);
		if (_cameraController == null)
		{
			Debug.LogWarning("CameraController is null");
			yield break;
		}
		_cameraController.ZoomTarget = _player.transform;
	}
	
	private void BoneSpearEvent()
	{
		if (!_firstUnit.IsDead)
		{
			if (ManagerRoot.Input.BoneSpearButton)
			{
				Time.timeScale = 1f;
				HideImage();
			}
		}
		else
		{
			_boneSpear.isCanCasting = true;
			ShowImage(ImageData.SituationType.CorpseRevive, _firstUnit.transform, new Vector2(0f, 2f), 10f);
			state = EnumTutorialState.TurnUndeadEvent;
		}
	}
	
	private void StartOpenBoxEvent()
	{
		if (_chest == null) return;
		if (!_chest.activeSelf)
		{
			_chest.SetActive(true);
		}
	}
	
	public void EndOpenBoxEvent()
	{
		state = EnumTutorialState.ShowPortalEvent;
	}
	
	public void ShowThrowBoneSpearTutorial(Vector2 pos)
	{
		_boneSpear.isCanCasting = false;

		ChangeZoomPos(pos);
		ShowText("!", _zoomTarget.transform, new Vector2(1f, 1f), Color.red, 1f);
		
		StartCoroutine(BoneSpearCoroutine());
	}

	IEnumerator BoneSpearCoroutine()
	{
		yield return new WaitForSeconds(_zoomDuration + 1f);
		Time.timeScale = 0f;
		// CanShowBoneSpear = true;
		_boneSpear.enabled = true;
		ShowImage(ImageData.SituationType.BoneSpearAttack, _player.transform, new Vector2(1.5f, 2f), 0f);
		_boneSpear.isCanCasting = true;
	}
	
	void ShowText(string text, Transform transform_, Vector2 offset, Color32 color, float duration = 1f)
	{
		Debug.Log("ShowText");
		_guideText = ManagerRoot.UI.ShowSceneUI<UIText>();

		Vector3 pos = transform_.position + (Vector3)offset;
		_guideText.SetInitialPosition(new Vector3(pos.x, pos.y, 0f));
		_guideText.SetText(text, color);
		_guideText.SetSize(2f);
		//Delete text
		Destroy(_guideText.gameObject, duration);
	}
	
	public void ShowImage(ImageData.SituationType situationType_, Transform transform_, Vector2 offset_, float duration_ = 1f)
	{
		_situationType = situationType_;
		_transform = transform_;
		_offset = offset_;
		_duration = duration_;

		isShowGuideImage = true;

		UIImage curImage = ManagerRoot.UI.ShowSceneUI<UIImage>();

		Vector3 pos = transform_.position + (Vector3)offset_;
		curImage.SetInitialPosition(new Vector3(pos.x, pos.y, 0f));

		if (ManagerRoot.Input.LastUsedInputType == DeviceChangeListener.ControlDeviceType.KeyboardAndMouse){
			_sprite = _imageData.ImagePairDictionary[situationType_].KeyboardSprite;
		}else{
			_sprite = _imageData.ImagePairDictionary[situationType_].GamepadSprite;
		}

		curImage.SetInnerImageWithImageData( _sprite );

		if (duration_ != 0f){
			StartCoroutine(ClearGuideImage(duration_));
		}

		_curImages.Add( curImage );
	}
	
	IEnumerator ClearGuideImage(float _duration)
	{
		yield return new WaitForSeconds(_duration);
		_situationType = default;
		_transform = null;
		_offset    = default;
		isShowGuideImage = false;
		
		HideImage();
	}


	public void HideImage()
	{
		foreach( UIImage curImage in _curImages)
		{
			if (curImage){
				Destroy( curImage.gameObject );
			}
		}
		_curImages.Clear();
	}


	public void FirstEncounterEvent() // 첫번째 병사 만나서 뼈창 쓰는 이벤트
	{
		if (_firstUnit.CurrentFaction == Faction.Undead)
		{
			HideImage(); // space바 숨김
			StartSecondManComeEvent();
			state = EnumTutorialState.SecondManComeEvent;
		}
	}

	// 첫번째 사람, Unit.TurnUndead()  한 후
	public void StartSecondManComeEvent()
	{
		HideImage(); // 턴언데드 가이드 이미지 숨김
		ManagerRoot.Unit.InstantiateUnit(UnitType.ArcherMan, new Vector2(-20f, 0f), Faction.Human, out _secondUnit);
	}

	// 두번째 사람, Unit. Die() 죽었을때
	public void StartEventTutorialEnd()
	{

		HideImage();

	}

	// 포탈 열린 후, 포탈 들어가기 전에
	public void StartEventGrind()
	{
		// ingGrindEvent = true;
		

		// Unit unit
	}

	public Unit GetNearestHuman(float detectRange = float.MaxValue)
	{
		List<Unit> enemyUnits = ManagerRoot.Unit.GetAllAliveHumanUnits();
		Unit nearestTarget = null;
		float nearestDistance = float.MaxValue;

		foreach (Unit enemyUnit in enemyUnits)
		{
			Vector3 toEnemy = enemyUnit.transform.position - _player.transform.position;
			if (toEnemy.magnitude > detectRange) continue;

			if (toEnemy.magnitude < nearestDistance)
			{
				nearestTarget = enemyUnit;
				nearestDistance = toEnemy.magnitude;
			}
		}
		return nearestTarget;
	}

}
