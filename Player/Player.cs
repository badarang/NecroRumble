
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityCharacterController;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour, IDamageable, IAttacker
{
	//디버그
	[SerializeField, HideInInspector] private bool _isGodMode;
	public bool IsGodMode
	{
		get => _isGodMode;
		set => _isGodMode = value;
	}

	//기본
	private PlayerData _data;
	private PlayerMovement _playerMove;
	private FeedbackController _feedback;
	private bool _canAction;
	public bool IsKnockDown { get; set; }
	public SkillBase[] skills;
	private CircleCollider2D _groundCheckCol;

	// IAttacker 구현
	public Transform Transform => transform;
	public GameObject GameObject => gameObject;
	public bool ActiveSelf => gameObject.activeSelf;

	//스탯
	[SerializeField, ReadOnly, ProgressBar(0, "MaxHp")]
	private int _curHp;
	private int _maxHp;
	private uint _darkEssence;

	[SerializeField, ReadOnly, ProgressBar(0, "MaxMp")]
	private int _curMp = 10; // temp set
	private int _maxMp = 10;     // temp set

	//네크로맨스
	private Light2D _necroLight;
	List<Unit> _corpseUnderfoots = new();
	public static int GrindExpUpValue { get; set; }
	
	//리서렉션 (두번째 목숨)
	private NecroResurrect resurrect;

	public static EnumPlayerCommand currentCommand = EnumPlayerCommand.None;

	//기타 이펙트
	private GameObject _flagPrefab;
	public GameObject flag;

	private bool isPlayedPlayerDeathAnimation;

	public bool isGameClearAnimationPlaying;

	private float playerInvincibleTimer;
	//public List<UnitType, int> _unitCount = new List<UnitType, int>();


	#region Properties
	public int CurHp
	{
		get => _curHp;
		private set
		{
			_curHp = value;
			_curHp = Mathf.Clamp(_curHp, 0, MaxHp);
		}
	}
	public int MaxHp
	{
		get => _maxHp;
		set => _maxHp = value;
	}

	public uint DarkEssence
	{
		get => _darkEssence;
		private set => _darkEssence = value;
	}

	public int CurMp
	{
		get => _curMp;
		set
		{
			_curMp = value;
			_curMp = Mathf.Clamp(_curMp, 0, MaxMp);
		}
	}

	public int MaxMp
	{
		get => _maxMp;
		private set => _maxMp = value;
	}

	public bool CanAction
	{
		get => _canAction;
		set => _canAction = value;
	}

	public PlayerData Data => _data;
	//public static float CutePlayerHealRatio { get; set; } //네크냥 체력 퍼센트 힐
	public static int CutePlayerFixedHealAmount { get; private set; } //네크냥 체력 고정 힐 
	public static int CutePlayerBonusHeal { get; set; } //네크냥 체력 고정 힐 증가량

	#endregion

	private void Awake()
	{
		_data = Resources.Load<PlayerData>("Data/PlayerData");
		_feedback = GetComponent<FeedbackController>();
		_necroLight = transform.Find("SpotLight").GetComponent<Light2D>();
		_flagPrefab = Resources.Load<GameObject>("Prefabs/Effects/Flag");
		resurrect = GetComponentInChildren<NecroResurrect>();
		_groundCheckCol = transform.Find("@DetectArea").GetComponent<CircleCollider2D>();

		_curHp = MaxHp = _data.MaxHP;
		CurMp = MaxMp = _data.MaxMP;
		_playerMove = GetComponent<PlayerMovement>();
		_canAction = true;

		CutePlayerBonusHeal = 0;
		skills = this.gameObject.GetComponentsInChildren<SkillBase>();
	}

	private void Start()
	{
		//FeedbackController Init
		if (TryGetComponent(out FeedbackController feedback))
		{
			feedback.Init();
		}

		//Character Init
		CharacterInit();
		
		HpBarUpdate();
		
		// Init Shadow
		ShadowInit();

		// StartCoroutine(RegenMp());
	}

	private void Update()
	{
		if (playerInvincibleTimer <= 0f)
		{
			playerInvincibleTimer = 0f;
		}
		else
		{
			playerInvincibleTimer -= Time.deltaTime;
		}
	}

	private void ShadowInit()
	{
		var shadowObject = transform.Find("@CircleShadow");
		if (shadowObject == null) return;
		//그림자 켜기
		shadowObject.gameObject.SetActive(true);
		
		//그림자 색 변경
		Material material = shadowObject.TryGetComponent(out SpriteRenderer sprRenderer) ? sprRenderer.material : null;
		if (material == null) return;
		material.SetColor("_Color", Statics.undeadShadowColor);

	}

	private void CharacterInit()
	{
		if (ManagerRoot.Unit.CurCharacter == UnitManager.CharacterType.Necromancer) // 할아버지
		{
			GrindExpUpValue = 2;
		}
		else // 미소녀
		{
			MaxHp = Statics.CutePlayerMaxHp;
			CurHp = MaxHp;
			CutePlayerFixedHealAmount = Statics.CutePlayerFixedHealAmount;
			GrindExpUpValue = 2;
		}
	}
	
	public void GainDarkEssence(uint value_)
	{
		DarkEssence += value_;
		GameManager.Instance.GameUI.SetDarkEssenceUI(DarkEssence);
	}

	public void CostDarkEssence(uint value_)
	{
		DarkEssence -= value_;
	}

	public void TakeDamage(AttackInfo info_, Color32 damageColor_ = default, Unit.SPECIALMODE specialMode_ = Unit.SPECIALMODE.NONE)
	{		
		//갓모드 무적
#if UNITY_EDITOR
		if (IsGodMode) return;
#endif
		//부활중이면 무적
		if (resurrect.IsResurrecting) return;
		
		//게임 클리어시 무적
		if (isGameClearAnimationPlaying) return;

		if (playerInvincibleTimer > 0f) return;
		
		int damage = (int)info_.damage;
		
		if (!SteamUserData.IsDamaged) SteamUserData.IsDamaged = true;

		//플레이어 데미지 보정
		Unit attackerUnit = info_.attacker as Unit;
		if (attackerUnit != null)
		{
			damage = Mathf.Min((int)attackerUnit.PairUnitData.GetStats(Faction.Human).BaseAttackDamage, damage);
			damage = (int)(damage * (1 - Statics.PlayerDamageReduceRatio));
		}
		damage = Mathf.Min(damage, 50);

		if (damage == 0) return;
		
		ManagerRoot.Event.onIDamageableTakeHit?.Invoke(this, info_, damage, ref damage);
		_curHp -= damage;
		SetPlayerInvincibleTime(Statics.PlayerInvincibleTime);
		HpBarUpdate();
		GameUIManager.Instance.SetHpBarDoScale();
		PlayHitSound(info_.attackingMedium.name);
		
		if (_curHp <= MaxHp * 0.5f)
		{
			PlayerTakeDamageEffect.TakeDamageEffect(1 - _curHp / (float)MaxHp);
		}
		
		if (_feedback == null)
		{
			Debug.Log("_feedback is null");
		}
		else if (info_.attacker.Transform != null)
		{
			_feedback.ApplyAttackerHit(_playerMove.GetMass(), info_.attacker.Transform.position);
		}
		GameUIManager.Instance.CreateNumberBalloon(damage.ToString(), transform.position, new Color32(136, 36, 124, 255));

		if (_curHp <= 0)
		{
			if (resurrect.CanResurrect && ManagerRoot.Unit.CurCharacter == UnitManager.CharacterType.Necromancer)
			{
				resurrect.StartResurrect();
			}
			else
			{
				if (isPlayedPlayerDeathAnimation == false) //연출
				{
					isPlayedPlayerDeathAnimation = true;
					_canAction = false;
					StartCoroutine(PlayPlayerDeathCO());
				}
			}
		}
	}

	private void SetPlayerInvincibleTime(float playerInvincibleTime)
	{
		playerInvincibleTimer = playerInvincibleTime;
		_feedback.ApplyInvincibleEffect(playerInvincibleTime);
	}

	private IEnumerator PlayPlayerDeathCO()
	{
		Time.timeScale = 0.1f;
		ManagerRoot.Sound.PlaySfx("Lost sound 13 Larger", 1f);
		_playerMove.GetAnimator().SetFloat("DieAnimSpd", 10f);
		_playerMove.PlayAnimation("NecromancerDie");
		yield return new WaitForSecondsRealtime(3f);
		//죽는 소리 재생
		GameManager.Instance.ChangeState(GameManager.GameState.GameOver);
	}
	
	public void TakeCharm(float charmDuration_, AttackInfo attackInfo_) { }

	public void TakeSlow(float slowPercent_, float slowDuration_)
	{
		_feedback.ApplyAttackerSlow(slowDuration_);
	}

	public void TakeHeal(float healAmount, Transform healer = null, bool isHealEffect = false)
	{
		_curHp += (int)healAmount;
		HpBarUpdate();
		GameUIManager.Instance.CreateNumberBalloon(healAmount.ToString(), transform.position, new Color32(10, 230, 10, 255));
		if (isHealEffect)
		{
			GameObject levelUpPrefab = ManagerRoot.Resource.Load<GameObject>("Prefabs/Effects/VFXHealEffect");
			GameObject levelUpEffect = Instantiate(levelUpPrefab, transform.position + new Vector3(0f, 0.5f, 0f), Quaternion.identity);
			levelUpEffect.transform.SetParent(transform);
			Destroy(levelUpEffect, 2f);
		}
	}

	public void TakeKnockBack(Transform attackerTrans_, float power_ = 20f)
	{
		if (attackerTrans_ != null)
		{
			if (_feedback == null)
			{
				Debug.Log("_feedback is null");
			}
			else
			{
				_feedback.ApplyAttackerKnockback(_playerMove.GetMass(), attackerTrans_.position, power_);
			}
		}
	}

	public void TakeKnockDown(Transform attackerTrans_, float power_ = 20f, float duration_ = 2f)
	{
		if (attackerTrans_ != null)
		{
			if (_feedback == null)
			{
				Debug.Log("_feedback is null");
			}
			else
			{
				IsKnockDown = true;
				StartCoroutine(KnockDownRoutine(duration_));
				_feedback.ApplyAttackerKnockdown(_playerMove.GetMass(), attackerTrans_.position, power_, duration_);
			}
		}
	}

	IEnumerator KnockDownRoutine(float duration_)
	{
		yield return new WaitForSeconds(duration_);
		IsKnockDown = false;
	}
	
	public void TakeKnockDownJump(Transform attackerTrans_, float power_ = 20f, float duration_ = 2f)
	{
		if (attackerTrans_ != null)
		{
			if (_feedback == null)
			{
				Debug.Log("_feedback is null");
			}
			else
			{
				_feedback.ApplyAttackerKnockdown(_playerMove.GetMass(), attackerTrans_.position, power_, duration_);
			}
		}
	}

	private void HpBarUpdate()
	{
		_curHp = Mathf.Clamp(_curHp, 0, MaxHp);
		GameUIManager.Instance.SetHpUI(_curHp, _maxHp);
	}

	// private void MpBarUpdate()
	// {
	// 	_curMp = Mathf.Clamp(_curMp, 0, MaxMp);
	// 	GameUIManager.Instance.SetMpUI(_curMp, MaxMp);
	// }

	// public void ReplenishMp(float value_)
	// {
	// 	CurMp += (int)value_;
	// 	MpBarUpdate();
	// }

	public void IncreaseMaxHp(int amount_)
	{
		MaxHp += amount_;
		CurHp += amount_;
		HpBarUpdate();
	}

	// public void IncreaseMaxMp(int maxMp_)
	// {
	// 	MaxMp += maxMp_;
	// 	MpBarUpdate();
	// }

// 	public void CostMp(float cost_)
// 	{
// 		//갓모드시 마나 안닳음
// #if UNITY_EDITOR
// 		if (IsGodMode) return;
// #endif

// 		CurMp -= (int)cost_;
// 		MpBarUpdate();
// 	}

	public void IncreaseNecroLightIntensity(float intensity)
	{
		if (intensity == 0f)
		{
			_necroLight.gameObject.SetActive(false);
			return;
		}

		if (_necroLight.gameObject.activeSelf == false)
		{
			_necroLight.gameObject.SetActive(true);
			_necroLight.intensity = 0f;
		}
		_necroLight.intensity += intensity;
		_necroLight.intensity = Mathf.Clamp(_necroLight.intensity, 0f, 3f);
	}

	public Vector2 GetMoveDirection()
	{
		return _playerMove.GetMoveDirection();
	}
	
	private void OnTriggerEnter2D(Collider2D other)
	{
		// if (other.gameObject.layer == Layers.HumanCorpse)
		// {
		//     if( !other.TryGetComponent(out Unit corpse) ){ return; }
		//     if( !_corpseUnderfoots.Contains(corpse) )
		//     {
		//         _corpseUnderfoots.Add(corpse);

		//         ReselectLastCorpseUnderfoot();
		//     }
		// }
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		// if (other.gameObject.layer == Layers.HumanCorpse)
		// {
		//     if( !other.TryGetComponent(out Unit corpse) ){ return; }
		//     if(  _corpseUnderfoots.Contains(corpse) )
		//     {
		//         corpse.CorpseUnselected();
		//         _corpseUnderfoots.Remove(corpse);
		//         ReselectLastCorpseUnderfoot();
		//     }
		// }
	}

	void ReselectLastCorpseUnderfoot()
	{
		if (_corpseUnderfoots.Count == 0) { return; }

		foreach (Unit corpse in _corpseUnderfoots)
		{
			corpse.CorpseUnselected();
		}
		GetLastCorpseUnderfoot().CorpseSelected();
	}

	public Unit GetLastCorpseUnderfoot()
	{
		if (_corpseUnderfoots == null || _corpseUnderfoots.Count == 0)
		{
			return null;
		}
		return _corpseUnderfoots[_corpseUnderfoots.Count - 1];
	}
	public List<Unit> GetListCorpseUnderfoot()
	{
		return _corpseUnderfoots;
	}

	public void CostCoprseUnderfoot(Unit corpseUnderfoot_)
	{
		corpseUnderfoot_.CorpseUnselected();
		if (_corpseUnderfoots.Contains(corpseUnderfoot_))
		{
			_corpseUnderfoots.Remove(corpseUnderfoot_);
			ReselectLastCorpseUnderfoot();
		}
	}

	public void CreateFlag(Transform destTransform_)
	{
		if (flag != null) { return; }
		// flag = ManagerRoot.Resource.Instantiate(_flagPrefab);
		// flag.transform.position = destTransform_.position;
		flag = Instantiate(_flagPrefab, destTransform_.position, Quaternion.identity);
		ManagerRoot.Sound.PlaySfx("Pop sound 1", 0.5f);
	}

	public void RemoveFlag()
	{
		if (flag == null) { return; }
		// ManagerRoot.Resource.Release(flag);
		flag.SetActive(false);
		Destroy(flag);
		flag = null;
	}

	public void PlayHitSound(string _attackMediumName)
	{
		switch (_attackMediumName)
		{
			case "Sword":
				ManagerRoot.Sound.PlaySfx("Dagger Stab (Flesh) 2", 0.3f);
				break;
			case "TestArrow":
				//TODO: 따로 빼기
				List<String> arrowHitSoundList = new List<string>()
				{
					"Arrow Impact flesh (human) 1",
					"Arrow Impact flesh (human) 4",
					"Arrow Impact flesh (human) 6",
					"Arrow Impact flesh (human) 7",
					"Arrow Impact flesh (human) 9",
					"Arrow Impact flesh (human) 10",
				};
				ManagerRoot.Sound.PlaySfx(arrowHitSoundList[Random.Range(0, arrowHitSoundList.Count)], 0.4f);
				break;
			case "Spear":
				ManagerRoot.Sound.PlaySfx("Dagger Stab (Flesh) 2", 0.25f);
				break;
			case "Assassin":
				ManagerRoot.Sound.PlaySfx("Spear Stab (Flesh) 3", 0.3f);
				break;
			case "Horse":
				ManagerRoot.Sound.PlaySfx("Heavy Sword Swing 1", 0.3f);
				break;
		}
	}

	[PropertyOrder(-1)]
	[HideIf("IsGodMode")]
	[Button("GOD MODE (off)", ButtonSizes.Large), GUIColor("white")]
	private void SetGodModeOn() => _isGodMode = true;

	[PropertyOrder(-1)]
	[ShowIf("IsGodMode")]
	[Button("GOD MODE (on)", ButtonSizes.Large)]
	[GUIColor("GetGodModeButtonColor")]
	private void SetGodModeOff() => _isGodMode = false;

	private static Color GetGodModeButtonColor()
	{
		const float INTERVAL = .15f;
		Color result = Color.white;

#if UNITY_EDITOR

		Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();

		float time = (float)EditorApplication.timeSinceStartup;
		time %= INTERVAL * 2;

		if (time > INTERVAL) result = Color.cyan;
		else result = Color.magenta;

#endif

		return result;
	}

	[Button("Fear Test", ButtonSizes.Large)]
	private void FearTest()
	{
		var allHumans = ManagerRoot.Unit.GetAllAliveHumanUnits();
		foreach (var human in allHumans)
		{
			human.TakeFear(1f, transform.position, 2f);
		}
	}

	public Collider2D[] GetGrounds()
	{
		//groundCheckCol에 닿아있는 Ground layer 콜라이더들의 배열을 반환
		return Physics2D.OverlapCircleAll(_groundCheckCol.transform.position, _groundCheckCol.radius, Layers.GridGround.ToMask());
	}

}