using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LOONACIA.Unity.Managers;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using System.Threading.Tasks;

public class PortalController : MonoBehaviour
{
	private Vector3 _originScale;
	private List<Unit> _undeadUnits = new List<Unit>();
	private bool _isWaitingSuckOut = false;
	//private bool _isPlayerNear = false;
	private bool _isAlreadySuckIn = false;
	public bool isSuckInDone = false;
	private UIText _guideText;

	public bool IsWaitingSuckOut => _isWaitingSuckOut;

	ImageData _imageData;
	Sprite _spriteKeyboard;
	Sprite _spriteGamepad;

	UIImage _curImage;

	DeviceChangeListener _deviceChangeListener => FindObjectOfType<DeviceChangeListener>();

	bool isTutorial = false;

	void Awake()
	{
		_originScale = transform.localScale;
		Reset();
	}
	void Start()
	{
		_imageData = ManagerRoot.Resource.Load<ImageData>("Data/UI/ImageData");
	}

	void Update()
	{
		//// if (_isPlayerNear && ManagerRoot.Input.PortalButton)
		// {
		//     if (_isAlreadySuckIn) return;
		//     // StartCoroutine(ManagerRoot.Unit.CheckAllSkeletonsInAllUnitsListAndMakeEnhancedSkeleton());
		//     // StartCoroutine(SuckIn());
		// }
	}

	void OnEnable()
	{
		Init();
		isTutorial = SceneManagerEx.CurrentScene.SceneType == SceneType.Tutorial;

		_deviceChangeListener.evtDeviceChanged += ReshowImage;
		if (GameManager.Instance.State == GameManager.GameState.Round)
		{
			//StartCoroutine(SuckIn());
			SuckIn();
		}
	}

	void OnDisable()
	{
		Reset();
		if (_deviceChangeListener != null)
		{
			_deviceChangeListener.evtDeviceChanged -= ReshowImage;
		}
	}

	void Reset()
	{
		//_isPlayerNear = false;
		_isWaitingSuckOut = false;

		HideText();
		HideImage();
	}

	void Init()
	{
		Reset();

		transform.localScale = Vector3.zero;
		transform.DOScaleX(_originScale.x, 0.5f)
			.OnStart(() => { transform.DOScaleY(_originScale.y, 1f); })
			.SetEase(Ease.OutQuad);
	}

	void Clear()
	{
		//_isPlayerNear = false;
		_isWaitingSuckOut = false;
		_isAlreadySuckIn = false;
		isSuckInDone = false;
		_undeadUnits.Clear();
	}

	public void ReverseTween()
	{
		transform.DOScaleY(0.25f, 0.25f)
			.OnComplete(() =>
			{
				transform.DOScaleX(0f, .1f);
				transform.DOScaleY(0f, .1f);
			})
			.SetEase(Ease.OutQuad);
	}

	//public async IEnumerator SuckIn()
	public async void SuckIn()
	{
		HideText();
		HideImage();

		_isAlreadySuckIn = true;
		Player player = GameManager.Instance.GetPlayer();

		player.CanAction = false;

		//Go forward sounds 6
		ManagerRoot.Sound.PlaySfx("Magical Chest opening 1", 1f);
		await Task.Delay(1000);
		// 해골 합치는거 기다려줘
		// yield return new WaitUntil(() => ManagerRoot.Unit.IsMergeEnd);
		// ManagerRoot.Unit.IsMergeEnd = false;

		//Player를 찾아서 SuckInCoroutine을 실행시킨다.
		if (player.TryGetComponent(out PortalSuckedInController playerSuckedInController))
		{
			Debug.Log("PortalController | SuckIn | playerSuckedInController start ");
			//StartCoroutine(playerSuckedInController.SuckInCoroutine());
			await playerSuckedInController.SuckInCoroutine();

			Debug.Log("PortalController | SuckIn | playerSuckedInController end ");
		}

		//Undead Unit들을 찾아서 SuckInCoroutine을 실행시킨다.
		_undeadUnits = ManagerRoot.Unit.GetAllAliveUndeadUnits();
		foreach (Unit unit in _undeadUnits)
		{
			if (unit.TryGetComponent(out PortalSuckedInController suckedInController))
			{
				Debug.Log("PortalController | SuckIn | suckedInController start");

				//StartCoroutine(suckedInController.SuckInCoroutine());
				await suckedInController.SuckInCoroutine();

				Debug.Log("PortalController | SuckIn | suckedInController end");
			}
			unit.IsStun = true;
		}

		Debug.Log("PortalController | SuckIn | Start Destoy All Corpse");

		//시체 제거
		ManagerRoot.Unit.DestroyAllCorpse(true);
        
		ReverseTween();
		ManagerRoot.Sound.PlaySfx("Throwing Knife (Thrown) 12", 1f);
		await Task.Delay(700); //yield return new WaitForSeconds(.7f);

		isSuckInDone = true;
		if (SceneManagerEx.CurrentScene.SceneType is not SceneType.Tutorial)
		{
			// ManagerRoot.Wave.WaveEnd();
			_isWaitingSuckOut = true;
		}
	}

	public IEnumerator SuckOut()
	{
		Debug.Log("PortalController | SuckOut |");

		Init();
		ManagerRoot.Sound.PlaySfx("Impact 2", .1f);
		//Player를 찾아서 TurnSuckedOutTrue를 실행시킨다.
		yield return new WaitForSeconds(1f);
		Player player = GameManager.Instance.GetPlayer();
		if (player.TryGetComponent(out PlayerMovement playerMovement))
		{
			playerMovement.PlayAnimation("NecromancerDie");
		}
		if (player.TryGetComponent(out PortalSuckedInController playerSuckedInController))
		{
			playerSuckedInController.TurnSuckedOutTrue();
		}

		yield return new WaitForSeconds(.3f);
		foreach (Unit unit in _undeadUnits)
		{
			if (unit.enabled == false) { continue; }
			if (unit.TryGetComponent(out PortalSuckedInController suckedInController))
			{
				suckedInController.TurnSuckedOutTrue();
			}
			unit.IsStun = true;
			unit.CurrentAnim.Play("Death");
		}
		ManagerRoot.Sound.PlaySfx("Staff Hitting (Flesh) 5", .6f);
		yield return new WaitForSeconds(2f);
		ReverseTween();
		player.CanAction = true;
		playerMovement.PlayAnimation("NecromancerJump");
		foreach (Unit unit in _undeadUnits)
		{
			unit.CurrentAnim.Play("Jump");
			unit.IsStun = false;
		}
		ManagerRoot.Sound.PlaySfx("Throwing Knife (Thrown) 4", 1f);
		yield return new WaitForSeconds(1f);
		gameObject.SetActive(false);
		_isWaitingSuckOut = false;
		_isAlreadySuckIn = false;
		_undeadUnits.Clear();
	}

	void ShowText(string text, Transform transform_, Color32 color)
	{
		_guideText = ManagerRoot.UI.ShowSceneUI<UIText>();

		Vector3 pos = transform_.position;
		_guideText.SetInitialPosition(new Vector3(pos.x, pos.y + 3f, 0f));
		_guideText.SetText(text, color);
	}

	void HideText()
	{
		if (_guideText != null)
		{
			Destroy(_guideText.gameObject);
			_guideText = null;
		}
	}
	void HideImage()
	{
		if (_curImage == null) { return; }
		Destroy(_curImage.gameObject);
	}
	void ShowImage()
	{
		Vector3 pos = transform.position + new Vector3(0, 3, 0);

		_curImage = ManagerRoot.UI.ShowSceneUI<UIImage>();
		_curImage.SetInitialPosition(pos);

		if (ManagerRoot.Input.LastUsedInputType == DeviceChangeListener.ControlDeviceType.KeyboardAndMouse)
		{
			_curImage.SetInnerImageWithImageData(_spriteKeyboard);
		}
		else
		{
			_curImage.SetInnerImageWithImageData(_spriteGamepad);
		}
	}
	void ReshowImage()
	{
		if (_curImage == null) { return; }

		HideImage();
		ShowImage();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.layer == Layers.Player)
		{
			//_isPlayerNear = true;

			if (!_isAlreadySuckIn)
			{
				ShowImage();
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject.layer == Layers.Player)
		{
			//_isPlayerNear = false;
			HideImage();
		}
	}
}