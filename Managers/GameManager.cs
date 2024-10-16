using System;
using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scene = UnityEngine.SceneManagement.Scene;
using TMPro;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	private int _curDifficulty;
	public int CurDifficulty
	{
		get => _curDifficulty;
		set => _curDifficulty = value;
		// if (GameUI != null) GameUI.SetDifficultyUI();
	}
	
	private int _clearedDifficulty;
	public int ClearedDifficulty
	{
		get => _clearedDifficulty;
		set => _clearedDifficulty = value;
	}
	
	private int _maxUndeadNum;
	public int MaxUndeadNum
	{
		get => _maxUndeadNum;
		set
		{
			_maxUndeadNum = value;
			if (GameUI != null) GameUI.SetUndeadNumUI();
		}
	}
	
	public int AttainableMaxUndeadNum { get; private set; }

	public int GetRemainUndeadNum()
	{
		return MaxUndeadNum - ManagerRoot.Unit.CountUndeadUnit();
	}

	private Player player = null;
	public Player GetPlayer()
	{
		if (player == null)
		{
			player = FindObjectOfType<Player>();
		}
		return player;
	}
	GameUIManager _gameUI;
	public GameUIManager GameUI => _gameUI;
	[ShowInInspector] public GameState State { get; private set; }
	public int EndRoundNum { get; set; }
	Coroutine _roundTimerCo;
	[SerializeField, ReadOnly] float _timeLeft;
	[SerializeField] bool _timeFlow = false;
	public bool TimeFlow
	{
		get => _timeFlow;
		set => _timeFlow = value;
	}

	public static Color32 UndeadBloodColor = new Color32(87, 38, 195, 255);
	public static Color32 HumanBloodColor = new Color32(184, 32, 32, 255);

	private Transform tmpReturnDestTransform;
	public PortalController Portal { get; private set; }

	public System.Action GameStateChanged;
	[HideInInspector] public SteamClientHandler SteamHandler;

	public bool IsPaused
	{
		get;
		private set;
	} = false;

	public bool IsOver
	{
		get;
		private set;
	} = false;
	public bool IsWin
	{
		get;
		private set;
	} = false;

	public enum GameState
	{
		Idle,
		Prepare,
		Round,
		Reward,
		GameOver,
		GameClear,
	}

	//public void preInit()
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

		CoroutineHandler.Init();
		GizmoHandler.Init();
		_gameUI = GameUIManager.Instance;
		
		if(FindAnyObjectByType<SteamClientHandler>() == null)
		{
			var go = new GameObject("@SteamClientHandler");
			go.transform.position = Vector3.zero;
			go.AddComponent<SteamClientHandler>();
			SteamHandler = go.GetComponent<SteamClientHandler>();
		}
		
		ClearedDifficulty = PlayerPrefs.GetInt("ClearedDifficulty", 0);
	}

	private void Start()
	{
		ManagerRoot managerRoot = ManagerRoot.Instance; // Manager Root Init을 위해 호출함
	}


	public void Init()
	{
		if (SceneManagerEx.CurrentScene.SceneType == SceneType.Tutorial)
		{				
			ManagerRoot.Unit.CurCharacter = 0;
		}
		if (ManagerRoot.Unit.CurCharacter == UnitManager.CharacterType.Necromancer)
		{
			_maxUndeadNum = 3;
			AttainableMaxUndeadNum = 999;
		}
		else
		{
			_maxUndeadNum = 2;
			AttainableMaxUndeadNum = Statics.CutePlayerMaxUndeadNumLimit;
			AddCutePlayerBasicUnitAdvantage();
			ManagerRoot.Event.onUnitSpawn += CutePlayerEliteAdvantage;
		}
		EndRoundNum = 4;

		BaseScene currentScene = SceneManagerEx.CurrentScene;
		if (SceneManagerEx.CurrentScene.SceneType == SceneType.Game)
		{
			Portal = (currentScene as GameScene).portalController;
			ChangeState(GameState.Prepare);
		}

		//Change player animator
		player = FindObjectOfType<Player>();
		if (player != null)
		{
			if (player.TryGetComponent(out PlayerMovement playerMovement))
			{
				playerMovement.SetAnimator(ManagerRoot.Unit.CurCharacter);
			}
		}

		ApplyDifficulty();
	}

	private void ApplyDifficulty()
	{
		//난이도 적용
		Debug.Log("<color=gray>GameManager | Init | Difficulty_1_OnUnitSpawn</color>");
		ManagerRoot.Event.onUnitSpawn += Difficulty_1_OnUnitSpawn; //난이도 1 효과 적용
		if (CurDifficulty <= 1) return;
		Debug.Log("<color=green>GameManager | Init | Difficulty_2_OnUnitSpawn</color>");
		ManagerRoot.Event.onUnitSpawn -= Difficulty_1_OnUnitSpawn; //난이도 2 효과 적용
		if (CurDifficulty <= 2) return;
		Debug.Log("<color=blue>GameManager | Init | Difficulty_3_OnUnitSpawn</color>");
		ManagerRoot.Event.onUnitSpawn += Difficulty_3_OnUnitSpawn; //난이도 3 효과 적용
		if (CurDifficulty <= 3) return;
		Debug.Log("<color=purple>GameManager | Init | Difficulty_4_OnUnitSpawn</color>");
		ManagerRoot.Event.onUnitSpawn += Difficulty_4_OnUnitSpawn; //난이도 4 효과 적용
		if (CurDifficulty <= 4) return;
		Debug.Log("<color=red>GameManager | Init | Difficulty_5_OnUnitSpawn</color>");
		ManagerRoot.Event.onUnitSpawn += Difficulty_5_OnUnitSpawn; //난이도 5 효과 적용
		if (CurDifficulty <= 5) return;
	}

	private void Difficulty_5_OnUnitSpawn(Unit newunit_)
	{
		//Debug.Log($"<color=yellow>GameManager | Difficulty_5_OnUnitSpawn | {newunit_.name} | {newunit_.instanceStats.GetModifierByName("Difficulty5_BuffMovespeed").modifierName}</color>");
		if (newunit_.CurrentFaction != Faction.Human) return; //인간에게만 적용
		StatModifier buffMoveSpeed = new StatModifier(StatType.MoveSpeed, "Difficulty5_BuffMovespeed", Statics.Difficulty_5_EnemyMoveSpeedModValue, StatModifierType.FinalPercentage, false);
		ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(newunit_, buffMoveSpeed);
		newunit_.Feedback.SetMoveSpeedEffect(true);
		Debug.Log($"GameManager | Difficulty_5_OnUnitSpawn | {newunit_.name} | {newunit_.instanceStats.GetModifierByName("Difficulty5_BuffMovespeed").modifierName}");
	}

	private void Difficulty_4_OnUnitSpawn(Unit newunit_)
	{
		if (newunit_.CurrentFaction != Faction.Human) return; //인간에게만 적용
		if (newunit_.IsElite) return; //엘리트면 취소
		if (ManagerRoot.Wave.CurrentTime < 60) return; //1분 이전에는 적용 안함 (초반에 적이 너무 강해지는 것을 방지하기 위함)
		StatModifier buffHp = new StatModifier(StatType.MaxHp, "Difficulty4_BuffHp", Statics.Difficulty_4_EnemyHpModValue, StatModifierType.FinalPercentage, false);
		ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(newunit_, buffHp);
	}

	private void Difficulty_3_OnUnitSpawn(Unit newunit_)
	{
		if (newunit_.CurrentFaction != Faction.Human) return; //인간에게만 적용
		if (newunit_.IsElite == false) return; //엘리트가 아니면 취소
		StatModifier buffHp = new StatModifier(StatType.MaxHp, "Difficulty3_BuffHp", Statics.Difficulty_3_EliteEnemyHpModValue, StatModifierType.FinalPercentage, false);
		ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(newunit_, buffHp);
	}

	private void Difficulty_1_OnUnitSpawn(Unit newunit_)
	{
		if (newunit_.CurrentFaction != Faction.Human) return; //인간에게만 적용
		StatModifier debuffAtk = new StatModifier(StatType.AttackDamage, "Difficulty1_DebuffAtk", Statics.Difficulty_1_EnemyAtkModValue, StatModifierType.FinalPercentage, false);
		ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(newunit_, debuffAtk);
	}

	private void AddCutePlayerBasicUnitAdvantage()
	{
		StatModifier hpMod = new StatModifier(StatType.MaxHp, "CutePlayerHpAdvantage", Statics.CutePlayerHpAdvantagePercentage,StatModifierType.FinalPercentage, true);
		StatModifier atkMod = new StatModifier(StatType.AttackDamage, "CutePlayerAtkAdvantage", Statics.CutePlayerAtkAdvantagePercentage,StatModifierType.FinalPercentage, true);
		
		//SwordMan
		ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.SwordMan, hpMod, false);
		ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.SwordMan, atkMod, false);
		
		//Archerman
		ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, hpMod, false);
		ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.ArcherMan, atkMod, false);
		
		//Assassin
		ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.Assassin, hpMod, false);
		ManagerRoot.UnitUpgrade.AddUnitUpgrade(UnitType.Assassin, atkMod, false);
	}

	private void CutePlayerEliteAdvantage(Unit newunit_)
	{
		if (newunit_.CurrentFaction == Faction.Human) return;
		if (newunit_.IsElite == false) return; //엘리트가 아니면 취소. (기본 유닛은 다른 함수에서 처리. 디스플레이상 문제 때문에)

		if (newunit_.instanceStats.GetModifierByName("CutePlayerHpAdvantage") == null)
		{
			StatModifier hpMod = new StatModifier(StatType.MaxHp, "CutePlayerHpAdvantage", Statics.CutePlayerHpAdvantagePercentage,StatModifierType.FinalPercentage, true);
			StatModifier atkMod = new StatModifier(StatType.AttackDamage, "CutePlayerAtkAdvantage", Statics.CutePlayerAtkAdvantagePercentage,StatModifierType.FinalPercentage, true);
			ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(newunit_, hpMod);
			ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(newunit_, atkMod);
		}
	}

	public void Clear()
	{
		IsOver = false;
		IsPaused = false;
		IsWin = false;
		TimeFlow = false;

		if (_roundTimerCo != null)
		{
			StopCoroutine(_roundTimerCo);
		}

		ChangeState(GameState.Idle);
	}

	public void PauseGame()
	{
		Debug.Log($"GameManager | PauseGame | {IsPaused}");
		if( SceneManagerEx.CurrentScene is TitleScene)
		{
			return;
		}

		if (!IsPaused)
		{
			if (GameUIManager.Instance.IsReward     )  { return; }
			if (GameUIManager.Instance.IsKeymapPopup)  { return; }
			if (GameUIManager.Instance.IsSettingsPopup){ return; }

			IsPaused = true;
			Time.timeScale = 0;
			GameUIManager.Instance.SetPauseScreen(true);
		}
		else if (IsPaused)
		{
			if (GameUIManager.Instance.IsReward     )  { return; }
			if (GameUIManager.Instance.IsKeymapPopup)  { return; }
			if (GameUIManager.Instance.IsSettingsPopup){ return; }

			IsPaused = false;
			if (!_gameUI.IsReward)
			{
				Time.timeScale = 1;
			}
			GameUIManager.Instance.SetPauseScreen(false);
		}
	}

	public void DefeatGame()
	{
		if( SceneManagerEx.CurrentScene is not GameScene)
		{
			return;
		}

		if (!IsOver)
		{
			IsOver = true;
			Time.timeScale = 0;
			GameUIManager.Instance.SetDefeatScreen(true);
			ManagerRoot.Sound.PlaySfx("Lost sound 13", 0.6f);
			
			SteamUserData.CheckUserStatsGameEnds(false);
		}
		else if (IsOver)
		{
			IsOver = false;
			Time.timeScale = 1;
			GameUIManager.Instance.SetDefeatScreen(false);
		}
	}
	public void WinGame()
	{
		if( SceneManagerEx.CurrentScene is not GameScene)
		{
			return;
		}

		if (!IsWin)
		{
			IsWin = true;
			Time.timeScale = 0;
			GameUIManager.Instance.SetWinScreen(true);
			_clearedDifficulty = CurDifficulty;
			PlayerPrefs.SetInt("ClearedDifficulty", CurDifficulty);
			SteamUserData.CheckUserStatsGameEnds(true);
		}
		else if (IsWin)
		{
			IsWin = false;
			Time.timeScale = 1;
			GameUIManager.Instance.SetWinScreen(false);
		}
	}

	public void RecallOrder()
	{
		if (Player.currentCommand == EnumPlayerCommand.Recall)
		{
			Player.currentCommand = EnumPlayerCommand.None;
			player.RemoveFlag();
		}
		else
		{
			Player.currentCommand = EnumPlayerCommand.Recall;
			tmpReturnDestTransform.position = player.transform.position;
			player.CreateFlag(tmpReturnDestTransform);
		}
	}

	public void IncreaseMaxUndeadNum(int num)
	{
		MaxUndeadNum += num;
	}

	public void ChangeState(GameState state_)
	{
		Debug.Log($"Game Manager | ChangeState : {State} to {state_}");
		State = state_;
		switch (state_)
		{
			case GameState.Prepare:
				OnPrePare();
				break;
			case GameState.Round:
				OnRound();
				break;
			case GameState.Reward:
				OnReward();
				break;
			case GameState.GameOver:
				OnGameOver();
				break;
			case GameState.GameClear:
				OnGameClear();
				break;
		}
		GameStateChanged?.Invoke();
	}



	// round 전 초기화해줘야 될 것들
	// round 전 셋팅해줘야 할 것들
	// 준비 끝나는대로 round로 넘겨주기
	private void OnPrePare()
	{
		tmpReturnDestTransform = new GameObject().transform;
		Player.currentCommand = EnumPlayerCommand.None;
		if (GetPlayer() != null) GetPlayer().RemoveFlag();

		UnitManager.UpdateUnitsInScene(); //시작할 때 Scene에 있던 유닛들 모두 리스트에 추가
		
		GameUIManager.Instance.SetUndeadNumUI();
		if (SceneManagerEx.CurrentScene.SceneType is SceneType.Tutorial)
		{
			//StartCoroutine(ShowKeymaps());
		}
		ChangeState(GameState.Round);
	}

	// round 중 체크해야되는 변수라던지 
	// round 중 발생할 이벤트라던지
	// round 시간 완료되거나 남은 적이 없으면 roundend로 넘겨주기
	private void OnRound()
	{
		GetPlayer().CurMp = GetPlayer().MaxMp;

		ManagerRoot.Skill.OnBattleStart();

		if (SceneManagerEx.CurrentScene.SceneType is not SceneType.Tutorial)
		{
			ManagerRoot.Wave.StartGameTimer();
			ManagerRoot.Wave.CreateWave();
		}

		// todo: Unit.SetCanMove(true)

	}

	// 웨이브 끝나고 지급하는 보상
	// 보상 UI 띄워줘야됨
	// 보상 선택 끝나면 prepare로 넘겨주기
	private void OnReward()
	{
		GetPlayer().CanAction = false;
		// todo: Unit.SetCanMove(false)

		ManagerRoot.Skill.OnBattleEnd();
	}

	private void OnGameOver()
	{
		DefeatGame();
	}
	private void OnGameClear()
	{
		StartCoroutine(WinGameDirection());
	}

	private IEnumerator WinGameDirection()
	{
		//승리 연출
		player.CanAction = false;
		player.isGameClearAnimationPlaying = true;
		ManagerRoot.Sound.PlaySfx("Impact Earthquake 1", 1f);
		ManagerRoot.Sound.PlaySfx("	Dragging Stone 7", 1f);
		CameraShake.Shake(2f, .2f);
		yield return new WaitForSecondsRealtime(2f);
		
		var damageEFfectGO = ManagerRoot.Resource.Load<GameObject>("Prefabs/Effects/CrawlingEffect");
		var damageEffect = Instantiate(damageEFfectGO, player.transform.position, Quaternion.identity);
		damageEffect.transform.GetChild(1).GetComponent<ShockWaveController>().CallShockWave();
		
		var _damageWave = ManagerRoot.Resource.Load<GameObject>("Prefabs/Skills/ResurrectDamageWave");
		var damageWave = Instantiate(_damageWave, player.transform.position, Quaternion.identity);
		damageWave.transform.localScale = Vector3.zero;
		damageWave.GetComponent<ResurrectDamageWave>().Init(player, 9999);
		
		while (damageWave.transform.localScale.x < 100f)
		{
			//대미지 웨이브 크기 키우기
			var localScale = damageWave.transform.localScale;
			var speed = Mathf.Lerp(10, 100, localScale.x / 100);
			localScale += Vector3.one * speed * Time.deltaTime;
			damageWave.transform.localScale = localScale;
			yield return null;
		}
		
		yield return new WaitForSecondsRealtime(1f);
		WinGame();
	}

	public void SetRoundTimer()
	{
		_timeLeft = MathF.Max(0, Statics.GameClearTime * 60 - ManagerRoot.Wave.CurrentTime);
		
		int minutes = Mathf.FloorToInt(_timeLeft / 60);
		int seconds = Mathf.FloorToInt(_timeLeft % 60);
		
		string formattedMinute = minutes.ToString();
		string formattedSecond = seconds.ToString();
		
		if (minutes < 10) formattedMinute = $"0{minutes}";
		if (seconds < 10) formattedSecond = $"0{seconds}";
		
		string formattedTime = formattedMinute + " : " + formattedSecond;
		
		_gameUI.SetTimeUI(formattedTime);
		_gameUI.GetTimeText().enabled = true;

		_roundTimerCo = StartCoroutine(RoundTimerCo());
	}

	IEnumerator RoundTimerCo()
	{
		TimeFlow = true;
		WaitForSeconds timerWfs = new WaitForSeconds(0.2f);

		while (ManagerRoot.Wave.CurrentTime > 0)
		{
			_timeLeft = MathF.Max(0, Statics.GameClearTime * 60 - ManagerRoot.Wave.CurrentTime);

			int minutes = Mathf.FloorToInt(_timeLeft / 60);
			int seconds = Mathf.FloorToInt(_timeLeft % 60);
		
			string formattedMinute = minutes.ToString();
			string formattedSecond = seconds.ToString();
		
			if (minutes < 10) formattedMinute = $"0{minutes}";
			if (seconds < 10) formattedSecond = $"0{seconds}";
			
			string formattedTime = formattedMinute + " : " + formattedSecond;
			
			_gameUI.SetTimeUI(formattedTime);
			//_gameUI.SetTimeUI(Mathf.FloorToInt(_timeLeft / 60) + " : " + Mathf.FloorToInt(_timeLeft % 60));

			yield return timerWfs;
		}

	}
	
	// private IEnumerator ShowKeymaps()
	// {
	// 	_gameUI.SetKeyMapInfos(true);
	// 	yield return new WaitForSeconds(30f);
	// 	_gameUI.SetKeyMapInfos(false);
	// }

	// public void CreatePortal()
	// {
	// 	Debug.Log("CreatePortal() ");
	// 	if (Portal != null)
	// 	{
	// 		Portal.gameObject.SetActive(true);
	// 		// CreateHealingPotionBasedOnRandom(Statics.HealingPotionDropPercent);
	// 	}
	// }
	public void CreateHealingPotionBasedOnRandom(float percent_, Vector3 position_)
	{
		// Vector3 randomPos = GetRandomPositionInQuarter(GameManager.Instance.GetPlayer().transform);
		Vector3 randomPos = GetRandomSpawnPosition(); //TODO: 이거 게임매니저 기준 위치로 설정되어있음ㅠ 어차피 유닛 죽었을때 나오지 않고 맵에 배치할거라 확률은 0으로 두고 쓰겠음.
		if (UnityEngine.Random.Range(0, 100) < percent_)
		{
			GameObject healingPotion = ManagerRoot.Resource.Instantiate("Items/HealingPotion");
			healingPotion.transform.position = randomPos;
		}
	}
	
	private Vector3 GetRandomSpawnPosition()
	{
		int xSign = Random.Range(0, 2) == 0 ? -1 : 1;
		int ySign = Random.Range(0, 2) == 0 ? -1 : 1;
		
		Vector3 randomOffset = new Vector2(Random.Range(40f, 45f) * xSign, Random.Range(25f, 30f) * ySign);
		return transform.position + randomOffset;
	}

	private void OnApplicationQuit() {
		if (!Application.isEditor) {
			System.Diagnostics.Process.GetCurrentProcess().Kill();
		}
	}
}
