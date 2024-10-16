using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public partial class Unit : MonoBehaviour, IDamageable, IAttacker
{
	#region Field Variables //----------------------------------------------

	[SerializeField] private Faction _currentFaction;
	private PairUnitData _humanData;
	private PairUnitData _undeadData;
	[SerializeField, ReadOnly] private int _currentHp;
	[SerializeField, ReadOnly] private int? _currentHatred;
	protected FeedbackController _feedback;
	public FeedbackController Feedback => _feedback;
	private Coroutine _attackRoutine;
	private Coroutine _deathCoroutine;
	private bool _isWalking;
	public bool CanRevive = true;
	protected Animator _humanAnimator;
	protected Animator _undeadAnimator;
	protected SpriteRenderer _humanRenderer;
	protected SpriteRenderer _undeadRenderer;
	protected Transform _bodyTransform;
	protected Transform _flipableTransform;
	protected WalkAnimRotator _walkAnimRotator;
	private UnitHpBarController _hpBarController;
	private GameObject _circleShadow;
	private GameObject _outerEffects;
	private Vector3 _currentScale;
	public Vector3 CurrentScale { get => _currentScale; }
	public GameObject _rangeCircle; //공격 범위 표시해주는 원 (카타리나)
	public GameObject _rangeCircle2; //공격 범위 표시해주는 원2 (자이언트 스켈레톤)
	private float originAnimatorSpeed;
	private UndeadForm _undeadForm;
	//TODO: 유닛 새로 만들때마다 start에서 설정해줄 필요 없이 자동으로 되도록 변경하기
	//private uint darkEssenceValue = 1; //유닛에 따른 어둠의 정수 가치 (궁수 = 1, 킹맨 = 5 등등..)
	public bool isReRevive = false;
	private uint _reReviveCount = 0;
	protected string reviveAudio = "Zombie Bite_04";
	protected float reviveVolume = 0.4f;
	private bool _isInvincible = false;
	private float _lastTrailDamagedTime = 0f;
	public int UnitType => (int)PairUnitData.UnitInfo.UnitType;

	#region Stun관련 ----------------------------------------
	public bool IsInvincible
	{
		get{
			return _isInvincible;
		}
		set{
			_isInvincible = value;
			if(value){
				// 무적 이펙트 켜주기
				_feedback?.ChangeMaterialIsInvincibleGlow(true);
			}
			else{
				// 무적 이펙트 꺼주기
				_feedback?.ChangeMaterialIsInvincibleGlow(false);
			}
		}
	}
	private bool _isCCImmunity = false;
	// stun states
	private int _isStun = 0;
	private int _isGenaralStun = 1 << 0;
	private int _isKnockDown = 1 << 1;
	private int _isReviving = 1 << 2;
	private int _isFearState = 1 << 3;
	private int _isGrindHooked = 1 << 4;
	#endregion

	[Header("Poison")]
	private bool _isPoison = false;
	private float _poisonDuration = 0f;
	private float _poisonTimer = 0f;
	private float _poisonTickInterval = 1f;
	private float _poisonDamagePercent = 0f;
	public bool IsPoison
	{
		get => _isPoison;
		set => _isPoison = value;
	}
	#region Upgrade Skill 관련 -----------------------------
	
	// Archer Unit
	[Header("ThirdHit")]
	private int _thirdHitCount = 0;
	private float _thirdHitDamageMultiplier = 0;
	private float _thirdHitDamageMultiplierElite = 0;
	public Coroutine ThirdHitCoroutine;
	
	public int ThirdHitCount
	{
		get => _thirdHitCount;
		set => _thirdHitCount = value;
	}
	
	public float ThirdHitDamageMultiplier
	{
		get => _thirdHitDamageMultiplier;
		set => _thirdHitDamageMultiplier = value;
	}
	
	public float ThirdHitDamageMultiplierElite
	{
		get => _thirdHitDamageMultiplierElite;
		set => _thirdHitDamageMultiplierElite = value;
	}

	// Assasin Unit
	// Fear
	public static float additionalFearDuration = 0;
	[HideInInspector] public float healingMultiplier = 1;

	#endregion
	public enum SPECIALMODE
	{
		NONE,
		NOEFFECT,
		NOEVENT,
	}

	public bool IsElite
	{
		get
		{
			if (UnitType == (int)global::UnitType.SwordMan || UnitType == (int)global::UnitType.ArcherMan ||UnitType == (int)global::UnitType.Assassin)
			{
				return false;
			}
			return true;
		}
	}

	private Coroutine _turnUndeadCoroutine;
	private Coroutine _fearCoroutine;

	[Header("Flip")]
	protected bool _isStopAutoFlip = false;
	private float _flipInertia = 0.5f;
	private bool isCanFlip = true;
	private float _flipInertiaTime = 0.25f;
		
	//유닛이 포함된 '무리'. null일 수 있다.

	#endregion

	//public static UnitType RecentUpgradedUnit => UnitUpgradeQueue.Count > 0 ? UnitUpgradeQueue.Peek() : global::UnitType.None;
	//public static Queue<UnitType> UnitUpgradeQueue = new();
	#region Properties //----------------------------------------------

	// IAttacker 구현
	public Transform Transform => transform;
	public GameObject GameObject => gameObject;
	public bool ActiveSelf
	{
		get
		{
			if (this != null && this.gameObject != null)
			{
				return this.gameObject.activeSelf;
			}
			else
			{
				return false;
			}
		}
	} 

	private PairUnitData _pairUnitData;
	//인간, 언데드 두 버전 모두 가지고 있는 데이터
	public PairUnitData PairUnitData
	{
		get
		{
			if (_pairUnitData == null)
				_pairUnitData = ManagerRoot.Resource.Load<PairUnitData>($"Data/Unit/{GetType().Name}Data");
			return _pairUnitData;
		}
	}
	
	
	
	//인간, 언데드 버전에 따른 유닛의 정보
	protected BaseUnitStats BaseStats => PairUnitData.GetStats(CurrentFaction);
	//public FactionUnitInfo Info = new FactionUnitInfo();
	public InstanceStats instanceStats;
	private bool _isDead;
	private Collider2D _corpseOverlapPreventingCollider;
	private Coroutine _decreaseAlphaCoroutine;


	//현재 세력. 이것만 바꿔주면 전부 자동으로 적용됨.
	public Faction CurrentFaction
	{
		get => _currentFaction;
		protected set
		{
			if (_currentFaction == value) return;

			_currentFaction = value;

			Init();
		}
	}

	public Faction OtherFaction
	{
		get
		{
			if (CurrentFaction == Faction.Undead)
			{
				return Faction.Human;
			}
			else
			{
				return Faction.Undead;
			}
		}
	}

	public int CurrentHp
	{
		get => _currentHp;
		protected set
		{
			_currentHp = Mathf.Clamp(value, 0, instanceStats.FinalMaxHp);
		}
	}

	public int? CurrentHatred
	{
		get => _currentHatred;
		protected set
		{
			if (instanceStats.FinalMaxHatred == null)
			{
				Debug.LogWarning("Trying to set CurrentHatred, but FinalMaxHatred == null. instanceStats must be updated.");
				return;
			}

			_currentHatred = value;

			if (_currentHatred == null) return;
			//증오 한계치 설정
			_currentHatred = Mathf.Clamp((int)_currentHatred, 0, (int)instanceStats.FinalMaxHatred);
		}
	}

	public UndeadForm UndeadForm { get => _undeadForm; set => _undeadForm = value; }

	[ShowInInspector]
	public uint Id { get; set; }

	public bool IsCommander { get; protected set; }
	
	[ShowInInspector]
	public bool IsDead
	{
		get => _isDead;
		protected set
		{
			_isDead = value;
			InitCircleShadow();
		}
	}

	public bool IsCCImmunity
	{
		get => _isCCImmunity;
		set => _isCCImmunity = value;
	}
	public bool IsStun 
	{ 
		get {
			return _isStun != 0;
		}
		set{
			SetStunFlag(_isGenaralStun, value);
		} 
	}
	public bool IsKnockDown
	{ 
		get {
			return CheckStunFlag(_isKnockDown);
		}
		set{
			SetStunFlag(_isKnockDown, value);
		} 
	}
	public bool IsReviving
	{ 
		get {
			return CheckStunFlag(_isReviving);
		}
		set{
			SetStunFlag(_isReviving, value);
		} 
	}

	public bool IsFearState
	{ 
		get {
			return CheckStunFlag(_isFearState);
		}
		set{
			SetStunFlag(_isFearState, value);
		} 
	}
		public bool IsGrindHooked
	{ 
		get {
			return CheckStunFlag(_isGrindHooked);
		}
		set{
			SetStunFlag(_isGrindHooked, value);
		} 
	}

	public bool IsAttacking { get; protected set; }
	public bool IsSpecialAttacking { get; protected set; }

	public bool IsWalking
	{
		get => _isWalking;
		protected set
		{
			_isWalking = value;
			CurrentAnim.SetBool(IsWalkingHash, value);
		}
	}

	public Animator CurrentAnim
	{
		get
		{
			if (_undeadAnimator == null) return _humanAnimator;
			else if (_humanAnimator == null) return _undeadAnimator;
			else return CurrentFaction == Faction.Undead ? _undeadAnimator : _humanAnimator;
		}
	}

	public UnitHpBarController HpBarController => _hpBarController;
	
	public UnitGroup Group { get; set; }

	#endregion

	#region Monobehavior Methods //----------------------------------------------

	private void Awake()
	{
		_humanAnimator = gameObject.FindChild("@HumanModel")?.GetComponent<Animator>();
		_undeadAnimator = gameObject.FindChild("@UndeadModel")?.GetComponent<Animator>();

		_humanRenderer = gameObject.FindChild("@HumanModel")?.GetComponent<SpriteRenderer>();
		_undeadRenderer = gameObject.FindChild("@UndeadModel")?.GetComponent<SpriteRenderer>();

		_bodyTransform = gameObject.FindChild("@Body").transform;
		_flipableTransform = gameObject.FindChild("@Flipable").transform;
		_walkAnimRotator = _flipableTransform.GetComponent<WalkAnimRotator>();
		_circleShadow = gameObject.FindChild("@CircleShadow").gameObject;
		
		_outerEffects = _bodyTransform.Find("OuterEffects").gameObject;
		_rangeCircle = _outerEffects.FindChild("RangeCircle").gameObject;
		_rangeCircle2 = _outerEffects.FindChild("RangeCircle2").gameObject;
		
		_corpseOverlapPreventingCollider = gameObject.FindChild("@CorpseOverlapPreventingCollider").GetComponent<Collider2D>();
		_feedback = GetComponent<FeedbackController>();
		_hpBarController = GetComponent<UnitHpBarController>();
		originAnimatorSpeed = CurrentAnim.speed;
		InitCircleShadow();
	}

	private void OnEnable()
	{
		Init();
	}

	public virtual void Start()
	{
		Init();
		InitAI();
	}

	public virtual void Update()
	{
		// Todo : 스테이지 고정되면 max값 바꿔서 멤버로 빼기
		// int maxPosY = 10000;
		// if(!IsDead){
		//     _humanRenderer.sortingOrder = maxPosY-(int)transform.position.y*100;
		//     _undeadRenderer.sortingOrder = maxPosY-(int)transform.position.y*100;
		// }

		// Todo : 카메라 밖에 적 오고있으면 표시? 기능 넣고 싶은데 고민중
		// Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
		// bool checkIsInCamera = viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0;
		// if (checkIsInCamera)
		// {
		//     // Your object is in the range of the camera, you can apply your behaviour
		//     Debug.Log("in camera");
		// }
		// else
		// {
		//     Debug.Log("out camera");

		// }

		UpdateFlip(); //TODO: 최적화 하게되면 이거 매 프레임마다 하는 것보다 좋은 방법 있을 듯?

		// if (TryGetComponent(out CheckTileInside checkTileInside))
		// {
		//     if (!checkTileInside.IsTileInside())
		//     {
		//         CurrentAnim.Play(JUMP);
		//     }
		// }

		// 스탯 보정치 시간 틱
		if (!IsReviving && !IsDead) instanceStats.TickModifierDurations();

		if (_isPoison)
		{
			_poisonDuration -= Time.deltaTime;
			if (_poisonDuration <= 0f)
			{
				_isPoison = false;
				_poisonDuration = 0f;
			}
			else
			{
				_poisonTimer -= Time.deltaTime;
				if (_poisonTimer <= 0f)
				{
					var poisonTextColor = new Color32(20, 100, 20, 255);
					var poisonDamage = instanceStats.FinalMaxHp * (_poisonDamagePercent / 100);
					
					//특수 유닛이면 데미지 반감.
					if (IsElite)
					{
						poisonDamage /= 4;
					}
					if (poisonDamage < 1) poisonDamage = 1;

					var atkInfo = new AttackInfo(this, poisonDamage, 0f);
					TakeDamage(atkInfo, poisonTextColor, SPECIALMODE.NOEFFECT);
					_poisonTimer = _poisonTickInterval;
				}
			}
		}

		UpdateCorpseOverlapPreventingCollider();
	}

	protected virtual void FixedUpdate() {
		if(IsEnableMove && !IsStun)
			_rigid.MovePosition((Vector2)transform.position + _velocity*Time.fixedDeltaTime);
		
	}

	#endregion

	#region Public Methods //----------------------------------------------

	public bool IsUnitDisable()
	{
		bool isMoveStopState = false; //GameManager.Instance.State != GameManager.GameState.Round; // Tutorial로 인해 해제

		return IsDead || IsStun || isMoveStopState;
	}

	public BaseUnitStats GetBaseStats()
	{
		return BaseStats;
	}

	public virtual void TryAttack(GameObject target_)
	{
		if (IsGrindHooked) return; //그라인드 갈고리에 맞아서 빨려들어가는 상황. 아무것도 못함.
		//FeedbackController Init
		if (target_.TryGetComponent(out Unit unit) && unit.IsDead) return;

		if (_attackRoutine != null) StopCoroutine(_attackRoutine);
		_attackRoutine = StartCoroutine(StartAttackAnimRoutine(target_));

		//증오(마나) 획득
		if (CurrentHatred is not null)
		{
			CurrentHatred += 10; //TODO: 증오 획득량 하드코딩 아닌 스태틱 변수로 빼기
		}
	}

	public virtual void TakeDamage(AttackInfo info_, Color32 color_ = default, SPECIALMODE specialMode_ = SPECIALMODE.NONE)
	{
		if (IsDead || IsInvincible || IsReviving) return; //죽은 유닛은 데미지를 받지 않음
		if (IsGrindHooked) return; //TODO: 오류수정 그라인드 갈고리에 맞아서 빨려들어가는 상황. 아무것도 못함.
		if (info_.attacker == null) { return; }
		if (info_.attacker.ActiveSelf == false) { return; }
		if (info_.attacker.Transform != null) // Transform 에 missing refer: 땅에 박힌 화살
		{
			if (_feedback == null)
			{
				Debug.Log("_feedback is null");
			}
			else
			{
				if (IsCCImmunity)
				{
					//피격 이펙트만 뜸
					_feedback.SetMaterialHitEffect();
				}
				else if (specialMode_ != SPECIALMODE.NOEFFECT)
				{
					_feedback.ApplyAttackerHit(_mass, info_.attacker.Transform.position, info_.knockBackPower);
				}
			}
		}
		
		var numberBalloonSize = 1f;
		int modifiedDamage = Mathf.FloorToInt(info_.damage * (100f - instanceStats.FinalDamageReduce) / 100f);
		if (modifiedDamage < 1) modifiedDamage = 1; //모든 공격이 최소 1의 데미지는 입힘.
		int finalDamage = modifiedDamage;
		if (specialMode_ != SPECIALMODE.NOEVENT)
		{
			ManagerRoot.Event.onIDamageableTakeHit?.Invoke(this, info_, modifiedDamage, ref finalDamage);
		}

		if (_thirdHitCount >= 3)
		{
			if (IsElite) //TODO: Elite 인 적에게 변수 부여 필요
			{
				finalDamage += (int) (CurrentHp * (_thirdHitDamageMultiplierElite / 100f));
			}
			else
			{
				finalDamage += (int) (CurrentHp * (_thirdHitDamageMultiplier / 100f));
			}
			_thirdHitCount = 0;
			_feedback.ControlThirdHitEffect(_thirdHitCount);
			numberBalloonSize = 2f;
		}
		
		CurrentHp -= finalDamage;
		if (CurrentHp <= 0) Die(info_);
		HpBarController.UpdateHpBar();

		if (CurrentFaction == Faction.Human)
		{
			var offsetX = Random.Range(-0.2f, 0.2f);
			var offSetY = Random.Range(-.2f, 0.2f);
			var offset = new Vector3(offsetX, offSetY, 0f);
			if (color_.Equals(default(Color32)))
			{
				switch ((int)(finalDamage / 50))
				{
					case 0:
						GameUIManager.Instance.CreateNumberBalloon(finalDamage.ToString(), transform.position + offset, new Color32(255, 255, 255, 255), numberBalloonSize);
						break;
					case 1:
						GameUIManager.Instance.CreateNumberBalloon(finalDamage.ToString(), transform.position + offset, new Color32(255, 60, 60, 255), numberBalloonSize * 1.2f);
						break;
					case 2:
						GameUIManager.Instance.CreateNumberBalloon(finalDamage.ToString(), transform.position + offset, new Color32(200, 100, 20, 255), numberBalloonSize * 1.4f);
						break;
					case 3:
						GameUIManager.Instance.CreateNumberBalloon(finalDamage.ToString(), transform.position + offset, new Color32(200, 200, 20, 255), numberBalloonSize * 1.6f);
						break;
					case 4:
						GameUIManager.Instance.CreateNumberBalloon(finalDamage.ToString(), transform.position + offset, new Color32(200, 20, 200, 255), numberBalloonSize * 1.8f);
						break;
					case >= 5 and < 20:
						GameUIManager.Instance.CreateNumberBalloon(finalDamage.ToString(), transform.position + offset, new Color32(150, 80, 255, 255), numberBalloonSize * 2.0f);
						break;
					default:
						GameUIManager.Instance.CreateNumberBalloon(finalDamage.ToString(), transform.position + offset, new Color32(255, 30, 115, 255), numberBalloonSize * 2.5f);
						break;
				}
			}
			else
			{
				GameUIManager.Instance.CreateNumberBalloon(finalDamage.ToString(), transform.position + offset + new Vector3(0f, .6f, 0f), color_, numberBalloonSize);
			}

			SteamUserData.CheckMaxDamage(finalDamage);
		}
		
		if (info_.attackingMedium is { } medium) PlayHitSound(medium.name);
		
		//무리에 속해있다면 무리내 모든 멤버들이 공격에 가담함.
		SetAggressive(true);
	}
	
	public virtual void TakeHeal(float healAmount, Transform healer = null, bool isHealEffect = true)
	{
		if (IsDead) return;
		if (IsGrindHooked) return; //그라인드 갈고리에 맞아서 빨려들어가는 상황. 아무것도 못함.
		
		healAmount *= healingMultiplier;
		//healAmount = instanceStats.FinalMaxHp - CurrentHp < healAmount ? instanceStats.FinalMaxHp - CurrentHp : healAmount; //최대 체력에 힐량 클램프
		
		CurrentHp += (int)healAmount;
		HpBarController.UpdateHpBar();
		GameUIManager.Instance.CreateNumberBalloon(((int)healAmount).ToString(), transform.position, new Color32(10, 230, 10, 255));
		if (isHealEffect)
		{
			GameObject levelUpPrefab = ManagerRoot.Resource.Load<GameObject>("Prefabs/Effects/VFXHealEffect");
			GameObject levelUpEffect = Instantiate(levelUpPrefab, transform.position + new Vector3(0f, 0.5f, 0f), Quaternion.identity);
			levelUpEffect.transform.SetParent(transform);
			Destroy(levelUpEffect, 2f);
		}
	}

	public virtual void TakeCharm(float charmDuration_, AttackInfo info_)
	{
		if (IsDead) return;
		if (IsCCImmunity) return;
		if (IsGrindHooked) return; //그라인드 갈고리에 맞아서 빨려들어가는 상황. 아무것도 못함.
		if(CurrentFaction == Faction.Undead) return;
		
		_feedback.ChangeMaterialCharmingGlow(true);
		if(CurrentFaction == Faction.Human)
			targetLayerMask = Layers.HumanUnit.ToMask();

		StartCoroutine(CharmRoutine(charmDuration_));
	}
	
	public virtual void TakeSlow(float slowPercent_, float slowDuration_)
	{
		if (IsDead || IsInvincible) return;
		if (IsCCImmunity) return;
		
		var slowValue = (-1) * Mathf.Abs(slowPercent_);
		
		_feedback.ApplyAttackerSlow(slowDuration_);
		
		//이미 슬로우 모디파이어가 남아있는게 있는지 확인
		var slowModifier = instanceStats.GetModifierByName("UnitSlow");
		if (slowModifier != null)
		{
			if (slowModifier.value >= slowValue && slowModifier.duration <= slowDuration_) //남아 있던 것보다 더 느리고 더 오래가면 (TODO: 더 고도화 필요. 현재는 상위 호환만 적용)
			{
				slowModifier.value = slowValue;
				slowModifier.duration = slowDuration_;
			}
		}
		else
		{
			StatModifier mod = new StatModifier(StatType.MoveSpeed, "UnitSlow", slowValue, StatModifierType.Percentage, false, slowDuration_);
			ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(this, mod);
		}
	}
	
	protected virtual IEnumerator CharmRoutine(float charmDuration_)
	{
		yield return new WaitForSeconds(charmDuration_);
		
		// 매혹 상태 끝
		// Todo : 매혹 당한 상태 이펙트 꺼주기
		_feedback.ChangeMaterialCharmingGlow(false);
		if(CurrentFaction == Faction.Human)
			targetLayerMask = Layers.UndeadUnit.ToMask() | Layers.Player.ToMask();
	}
	public void TakeKnockBack(Transform attackerTrans_, float power_ = 20f)
	{
		if (IsUnitDisable() || IsInvincible) return;
		if (IsGrindHooked) return; //그라인드 갈고리에 맞아서 빨려들어가는 상황. 아무것도 못함.
		if (IsCCImmunity) return;
		if (attackerTrans_ != null)
		{
			if (_feedback == null)
			{
				Debug.Log("_feedback == null");
			}
			else
			{
				_feedback.ApplyAttackerKnockback(_mass, attackerTrans_.position, power_);
				StartCoroutine(EndKnockDown());
			}
		}
	}
	public void TakeKnockDown(Transform attackerTrans_, float power_ = 20f, float duration_ = 1f)
	{
		if (IsUnitDisable() || IsInvincible) return;
		if (IsGrindHooked) return; //그라인드 갈고리에 맞아서 빨려들어가는 상황. 아무것도 못함.
		if (IsCCImmunity) return;
		if (attackerTrans_ != null)
		{
			if (_feedback == null)
			{
				Debug.Log("_feedback == null");
			}
			else
			{
				_feedback.ApplyAttackerKnockdown(_mass, attackerTrans_.position, power_, duration_);
				StartCoroutine(AIKnockDownRoutine(duration_));
			}
		}
	}
	
	public void TakeKnockDownJump(Transform attackerTrans_, float power_ = 20f, float duration_ = 1f)
	{
		if (IsUnitDisable() || IsInvincible) return;
		if (IsGrindHooked) return; //그라인드 갈고리에 맞아서 빨려들어가는 상황. 아무것도 못함.
		if (IsCCImmunity) return;
		if (attackerTrans_ != null)
		{
			if (_feedback == null)
			{
				Debug.Log("_feedback == null");
			}
			else
			{
				_feedback.ApplyAttackerKnockdown(_mass, attackerTrans_.position, power_, duration_);
				
				var dest_ = transform.localPosition + (transform.position - attackerTrans_.position).normalized * Random.Range(1f, 3f);
				var _jumpDuration = 1f;
				var _jumpHeight = power_ * 0.25f * (IsElite ? .5f : 1f);
				
				//그림자를 포함한 전체 오브젝트가 목적지로 이동함.
				transform.DOLocalMove(dest_, _jumpDuration)
					.SetEase(Ease.Linear);
				
				//그림자를 제외한 몸체 부분은 로컬 포지션상으로 제자리 점프함.
				_bodyTransform.DOLocalJump(Vector3.zero, _jumpHeight, 1, _jumpDuration)
					.SetEase(Ease.Linear)
					.AppendCallback(() =>
					{
						ManagerRoot.Sound.PlaySfx("Staff Hitting (Flesh) 5", .3f);
					});
				StartCoroutine(AIKnockDownRoutine(duration_));
			}
		}
	}
	
	public void TakeFear(float speed, Vector3 attackPos_, float duration_, AttackInfo info_ = null)
	{
		if (IsUnitDisable() || IsInvincible) return;
		if (IsGrindHooked) return; //그라인드 갈고리에 맞아서 빨려들어가는 상황. 아무것도 못함.
		if (IsCCImmunity) return;
		if (_fearCoroutine != null) StopCoroutine(_fearCoroutine);
		_fearCoroutine = StartCoroutine(FearRoutine(speed, attackPos_, duration_));
		
		ManagerRoot.Event.onUnitTakeFear?.Invoke(this, info_);
		_feedback.ApplyAttackerFear(duration_ + additionalFearDuration + 1f);
		
	}

	public void TakePoison(float duration, float dmgPercent)
	{
		if(IsInvincible) return;
		_isPoison = true;
		_poisonDuration = duration;
		_poisonDamagePercent = dmgPercent;
		_feedback.ApplyAttackerPoison(duration);
	}
	
	public IEnumerator SetThirdHitCoroutine(float duration_)
	{
		yield return new WaitForSeconds(duration_);
		_thirdHitCount = 0;
		_feedback.ControlThirdHitEffect(ThirdHitCount);
	}
	
	IEnumerator FearRoutine(float speed, Vector3 attackPos_, float duration_)
	{
		IsFearState = true;
		CurrentAnim.Play(RUNAWAY);
		
		float time = 0f;
		var direction = transform.position - attackPos_;
		direction = direction.normalized;
		
		
		while (time < duration_)
		{
			time += Time.deltaTime;
			transform.Translate(direction * Time.deltaTime);

			if (IsDead) break;
			
			yield return null;
		}
		
		IsFearState = false;
	}
	
	IEnumerator AIKnockDownRoutine(float duration_)
	{
		IsKnockDown = true;
		_collider.isTrigger = true;
		yield return new WaitForSeconds(duration_);
		if(!IsDead)
			_collider.isTrigger = false;
		StartCoroutine(EndKnockDown());
	}
	IEnumerator EndKnockDown()
	{
		yield return new WaitForSeconds(0.5f);
		IsKnockDown = false;
		BackToOriginalRotation();
	}

	public void TurnUndead()
	{
		if (CurrentFaction == Faction.Undead) return;
		if(_deathCoroutine != null) {StopCoroutine(_deathCoroutine);}
		CurrentFaction = Faction.Undead;
		Debug.Log($"Turn Undead : {name}");
		AIStop();
		ManagerRoot.Sound.PlaySfx(reviveAudio, reviveVolume);
		Feedback.ClearEffectCoroutines();
		Feedback.ChangeUnitColor(Color.white);
		//set alpha to 1
		if (_decreaseAlphaCoroutine != null) StopCoroutine(_decreaseAlphaCoroutine);
		_undeadRenderer.color.ChangedAlpha(1);
		
		if (_turnUndeadCoroutine == null)
			_turnUndeadCoroutine = StartCoroutine(PlayTurnUndeadAnimation());
		DestroyDeadWeapon();
		InitCircleShadow();
		HpBarController.UpdateHpBar();

		if (!IsElite)
		{
			GainExp(ExpType.Revive);
		}

		TurnUndeadEvent();

	}

	public void GainExp(ExpType expType_)
	{
		//최대 레벨에 도달하면 경험치 얻지 않음.
		if (UnitManager.UnitExpDic[(UnitType)UnitType].level >= UnitManager.UNIT_MAX_LEVEL) return;
		
		int deltaExp = 0;

		switch (expType_)
		{
			case ExpType.Revive:
				deltaExp = 1;
				break;
			case ExpType.Grind:
				deltaExp = Player.GrindExpUpValue;
				break;
		}

		
		//StartCreateExpPrefabs(GetTargetPos(), 0.3f, deltaExp); -> 과거 보라 구슬 획득 이펙트, 주석 지우면 다시 나옴
		UnitManager.UnitExpDic[(UnitType)UnitType].curExp += deltaExp;

		string unitAboveStr = $"Exp +{deltaExp}";

		GameUIManager.Instance.CreateNumberBalloon(unitAboveStr, transform.position, new Color32(200, 200, 200, 200));
		//ManagerRoot.UI.ShowSceneUI<UIUnitExpPanel>(); // 계속 떠있는 것으로 변경하면서 주석처리.
		GameUIManager.Instance.UpdateLvExpInfo((UnitType)UnitType, deltaLevel_: 0, deltaExp_: deltaExp); // level, exp
		
		bool canLevelUp = UnitManager.UnitExpDic[(UnitType)UnitType].maxExp <= UnitManager.UnitExpDic[(UnitType)UnitType].curExp;
		if (canLevelUp)
		{
			LevelUp((UnitType)UnitType);
		}
		
		GameUIManager.Instance.SetExpPanelBlink(UnitType, deltaExp);
	}

	public void LevelUp(UnitType unitType_)
	{
		//최대 레벨에 도달하면 레벨업하지 않음.
		if (UnitManager.UnitExpDic[unitType_].level >= UnitManager.UNIT_MAX_LEVEL) return;
		
		Debug.Log($"Unit | LevelUp | {unitType_}");

		UnitManager.UnitExpDic[unitType_].curExp -= UnitManager.UnitExpDic[unitType_].maxExp;
		UnitManager.UnitExpDic[unitType_].level++;
		
		if(UnitManager.UnitExpDic[unitType_].level >= UnitManager.UNIT_MAX_LEVEL)
		{
			var v = unitType_ switch
			{
				global::UnitType.SwordMan => SteamUserData.IsWarriorMaxLv = true,
				global::UnitType.ArcherMan => SteamUserData.IsArcherMaxLv = true,
				global::UnitType.Assassin => SteamUserData.IsAssassinMaxLv = true,
				_ => false
			};
			
			SteamUserData.CheckMasterAll();
		}

		UnitManager.UnitExpDic[unitType_].maxExp += UnitManager.MAX_EXP_INCREASE; // 다음 레벨업에 필요한 경험치 1 증가

		ManagerRoot.Sound.PlaySfx("Impact Horn 4", .4f);
		string unitAboveStr = ManagerRoot.I18N.getValue("^text_popup_levelUp");
		GameUIManager.Instance.CreateNumberBalloon(unitAboveStr, transform.position + new Vector3(0f, 1f, 0f), new Color32(255, 255, 0, 200), 1.5f);

		GameObject levelUpPrefab = ManagerRoot.Resource.Load<GameObject>("Prefabs/Effects/VFXLevelUpEffect");
		GameObject levelUpEffect = Instantiate(levelUpPrefab, transform.position + new Vector3(1f, 1f, 0f), Quaternion.identity);
		Destroy(levelUpEffect, 1f);

		//UI 창 생성
		GameUIManager.Instance.UpdateLvExpInfo(unitType_, deltaLevel_: 1, deltaExp_: 0); // level, exp

		//업그레이드 창 띄워야 할 큐에 추가
		//UnitUpgradeQueue.Enqueue(unitType_);


		ManagerRoot.Unit.GetSpecificUnitTypeAliveUnits(unitType_, Faction.Undead).ForEach(unit => unit.LevelUpEvent());

		//Delay 1 sec
		StartCoroutine(ShowRewardPopupByRankDispersionCo());
	}
	
	private Vector2 GetTargetPos()
	{
		Vector2 targetPos = Vector2.zero;
		float offset = 120f;
		Vector2 offsetVector = new Vector2(-40f, 40f);
			
		switch (UnitType)
		{
			case (int)global::UnitType.SwordMan:
				targetPos = GameUIManager.Instance.UnitExpPanelPosList[0] + Vector2.up * offset;
				break;
			case (int)global::UnitType.ArcherMan:
				targetPos = GameUIManager.Instance.UnitExpPanelPosList[1];
				break;
			case (int)global::UnitType.Assassin:
				targetPos = GameUIManager.Instance.UnitExpPanelPosList[2] + Vector2.down * offset;
				break;
			default:
				Debug.Log("UnIdentified UnitType");
				break;
		}

		
		if (targetPos != Vector2.zero)
		{
			targetPos += offsetVector;
			targetPos = Camera.main.ScreenToWorldPoint(targetPos);
		}

		return targetPos;
	}
	
	private void StartCreateExpPrefabs(Vector3 targetPos, float delay, int count)
	{
		StartCoroutine(CreateExpPrefabsCoroutine(targetPos, delay, count));
	}
	
	private IEnumerator CreateExpPrefabsCoroutine(Vector3 targetPos, float delay, int count)
	{
		for (int i = 0; i < count; i++)
		{
			yield return new WaitForSeconds(delay);
			CreateExpPrefab(targetPos);
		}
	}
	public void CreateExpPrefab(Vector2 targetPos)
	{
		GameObject expParticle = Instantiate(Resources.Load<GameObject>("Prefabs/Effects/BloodAbsorbtionEffect"), transform.position, Quaternion.identity);
		expParticle.GetComponent<BloodAbsorptionController>().targetPos = targetPos;
		expParticle.GetComponent<BloodAbsorptionController>().unitType = UnitType;
		//expParticle.GetComponent<BloodAbsorptionController>().JumpToPosition(targetPos);
		expParticle.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.2f, 1f, 1f);
		expParticle.GetComponent<TrailRenderer>().startColor = new Color(0.5f, 0.2f, 1f, 1f);
		expParticle.GetComponent<TrailRenderer>().endColor = new Color(0.5f, 0.2f, 1f, 1f);
	}
	
	IEnumerator ShowRewardPopupByRankDispersionCo()
	{
		yield return new WaitForSeconds(1f);

		//유닛 전용 강화인 경우
		int rank = UnitType switch
		{
			(int)global::UnitType.SwordMan  => 203,
			(int)global::UnitType.ArcherMan => 303,
			(int)global::UnitType.Assassin  => 403,
			_ => 0,
		};
		int dispersion = 0;
		
		//공용 강화인 경우
		if (UnitManager.UnitExpDic[(UnitType)UnitType].level % 2 != 1)
		{
			rank = 100;
			dispersion = 3;
		}

		GameUIManager.Instance.ShowRewardPopupByRankDispersion(rank, dispersion, (UnitType)UnitType);
	}

	public void LevelUpEvent()
	{
		GameObject levelUpPrefab = ManagerRoot.Resource.Load<GameObject>("Prefabs/Effects/VFXLevelUpAllEffect");
		GameObject levelUpEffect = Instantiate(levelUpPrefab, transform.position + new Vector3(0f, 0.5f, 0f), Quaternion.identity);
		levelUpEffect.transform.SetParent(transform);
		Destroy(levelUpEffect, 2f);
	}

	public virtual void TurnUndeadEvent() { }

	public void ReRevive()
	{
		isReRevive = true;
		_reReviveCount--;
		_feedback.ChangeMaterialGhostBlend(true);
		Debug.Log($"ReRevive : {name}");
		ManagerRoot.Sound.PlaySfx("Large Monster Growls 01");
		IsDead = false;
		StartCoroutine(PlayTurnUndeadAnimation(true));
	}
	public uint IncrementReReviveCount()
	{
		return ++_reReviveCount;
	}

	public void SetFlipableLocalScaleX(bool isLeft)
	{
		Vector3 tmp = _flipableTransform.localScale;
		tmp = isLeft ? new Vector3(Mathf.Abs(tmp.x) * -1, tmp.y, tmp.z) : new Vector3(Mathf.Abs(tmp.x), tmp.y, tmp.z);
		_flipableTransform.localScale = tmp;
	}

	public void PlayHitSound(string _attackMediumName)
	{
		switch (_attackMediumName)
		{
			case "Sword":
				ManagerRoot.Sound.PlaySfx("Dagger Stab (Flesh) 2", 0.4f);
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
				ManagerRoot.Sound.PlaySfx(arrowHitSoundList[Random.Range(0, arrowHitSoundList.Count)], 0.5f);
				break;
			case "Spear":
				ManagerRoot.Sound.PlaySfx("Dagger Stab (Flesh) 2", 0.35f);
				break;
			case "Assassin":
				ManagerRoot.Sound.PlaySfx("Spear Stab (Flesh) 3", 0.35f);
				break;
			case "Horse":
				ManagerRoot.Sound.PlaySfx("Heavy Sword Swing 1", 0.35f);
				break;
		}
	}

	public void PlayAttackSound() 
	{
		string attackSound = "";
		float volume = 0.4f;
		switch (UnitType)
		{
			case (int)global::UnitType.SpearMan:
				attackSound = "";
				break;
			case (int)global::UnitType.SwordMan:
				attackSound = "";
				break;
			case (int)global::UnitType.ArcherMan:
				attackSound = "";
				break;
			case (int)global::UnitType.Assassin:
				attackSound = "";
				break;
			case (int)global::UnitType.HorseMan:
				var horseAttackSounds = new string[]{"Heavy sword woosh 7", "Heavy sword woosh 10", "Heavy sword woosh 11"};
				if (_currentFaction == Faction.Human)
				{
					horseAttackSounds = new string[]{"Dagger Stab (Flesh) 2", "Dagger Stab (Flesh) 3", "Dagger Stab (Flesh) 4", "Dagger Stab (Flesh) 5"};
				}
				
				attackSound = horseAttackSounds[Random.Range(0, horseAttackSounds.Length)];
				volume = 1f;
				break;
			case (int)global::UnitType.Golem:
				var golemSounds = new string[]{"Earth Punch 1", "Earth Punch 2", "Earth Punch 3", "Earth Punch 5"};
				attackSound = golemSounds[Random.Range(0, golemSounds.Length)];
				volume = 1f;
				break;
			case (int)global::UnitType.TwoSwordAssassin:
				var twoSwordSounds = new string[]{"Dagger Stab (Flesh) 2", "Dagger Stab (Flesh) 3", "Dagger Stab (Flesh) 4", "Dagger Stab (Flesh) 5"};
				attackSound = twoSwordSounds[Random.Range(0, twoSwordSounds.Length)];
				volume = 1f;
				break;
			case (int)global::UnitType.FlightSword:
				var flightSwordSounds = new string[]{"Heavy sword woosh 7", "Heavy sword woosh 10", "Heavy sword woosh 11"};
				attackSound = flightSwordSounds[Random.Range(0, flightSwordSounds.Length)];
				volume = 1f;
				break;
			case (int)global::UnitType.PriestSuccubus:
				var whipSounds = new string[]{"Whipping Horse Larger 1", "Whipping Horse Larger 2",};
				attackSound = whipSounds[Random.Range(0, whipSounds.Length)];
				volume = 1f;
				break;
		}

		if (attackSound != "")
		{
			ManagerRoot.Sound.PlaySfx(attackSound, volume);
		}
	}

	#endregion

	public static void ClearStaticVariables(){
		additionalFearDuration = 0f;
	}
	public void InitFactionSet(Faction faction_)
	{
		this.CurrentFaction = faction_;
	}

	protected virtual void Init()
	{
		//죽음 상태 초기화
		IsDead = false;
		tag = CurrentFaction.ToString();

		//HumanModel과 UndeadModel 만 알파값 1로 변경
		_humanRenderer.color = _humanRenderer.color.ChangedAlpha(1);
		_undeadRenderer.color = _undeadRenderer.color.ChangedAlpha(1);

		//스탯 초기화
		if (instanceStats == null)
		{
			instanceStats = new InstanceStats(this);
		}
		else //인스턴스 스탯이 이미 존재한다면
		{
			//스탯 타입이 다르다면 (세력이 다르다면)
			if (instanceStats.BaseUnitStats.GetType() != BaseStats.GetType())
			{
				//인스턴스 스탯 초기화
				instanceStats = new InstanceStats(this);
			}
		}

		CurrentHp = instanceStats.FinalMaxHp; //체력 초기화

		if (CurrentFaction == Faction.Undead)
		{
			gameObject.layer = Layers.UndeadUnit;
			targetLayerMask = Layers.HumanUnit.ToMask();
			_maxTakeAggroSize = instanceStats.FinalMaxAggroNum ?? 1;
			if (instanceStats.FinalMaxAggroNum == null)
				Debug.LogWarning("Undead's MaxAggroNum returned null. instanceStats must be updated.");
			if (GameManager.Instance != null)
			{
				GameManager.Instance.GameUI.SetUndeadNumUI();
			}
			else
			{
				FindObjectOfType<GameManager>().GameUI.SetUndeadNumUI();
			}
		}
		else
		{
			gameObject.layer = Layers.HumanUnit;
			targetLayerMask = Layers.UndeadUnit.ToMask() | Layers.Player.ToMask();
			_maxTakeAggroSize = int.MaxValue;
		}

		//스프라이트 렌더러, 애니메이터 변경
		_humanAnimator.gameObject.SetActive(CurrentFaction == Faction.Human);
		_undeadAnimator.gameObject.SetActive(CurrentFaction == Faction.Undead);
		
		_currentScale = _humanAnimator.transform.localScale;

		//FeedbackController Init
		if (TryGetComponent(out FeedbackController feedback))
		{
			feedback.Init();
		}

		_collider = GetComponent<Collider2D>();
		_rigid = GetComponent<Rigidbody2D>();
		_collider.isTrigger = false;

		

	}

	protected void Die(AttackInfo attackInfo_ = default)
	{
		if (IsDead) return;
		
		IsDead = true;

		_collider.isTrigger = true;
		gameObject.layer = CurrentFaction == Faction.Undead
			? Layers.UndeadCorpse
			: Layers.HumanCorpse;

		_humanRenderer.sortingOrder = 0;
		_undeadRenderer.sortingOrder = 0;

		AttackTarget = null;

		if (HpBarController != null) HpBarController.TurnOffHpBar();
		
		//피드백 전체 제거
		if (TryGetComponent(out FeedbackController feedback))
		{
			feedback.ResetAllFeedbacks();
		}

		//피 이펙트 생성
		CreateBloodEffect();

		//죽음 애니메이션
		StartCoroutine(PlayDeathAnimation());

		//죽음 이벤트 실행
		ManagerRoot.Event.onUnitDeath?.Invoke(this, attackInfo_);

		BackToOriginalRotation();

		//시체 제거
		if (CurrentFaction == Faction.Undead)
		{
			if (_reReviveCount > 0)
			{ // Use by Skill Scarecrow. todo: 구조 개선 필요
				ReRevive();
				return;
			}
			StartCoroutine(DestroyCorpse());
			GameManager.Instance.GameUI.SetUndeadNumUI();
		}

		//시체 무기 생성
		if (CurrentFaction == Faction.Human)
		{
			CreateDeadWeapon();
			// 죽을때 포션
			GameManager.Instance.CreateHealingPotionBasedOnRandom(Statics.HealingPotionDropPercent, transform.position);

			//TODO: Vertical SLice 에서는 주석처리
			//CreateDarkEssence(darkEssenceValue);
			
			//죽는 타이머 시작
			if (!IsElite)
			{
				if (_deathCoroutine == null) _deathCoroutine = StartCoroutine(CorpseDecayCoroutine());
			}
			// 시체 색 어둡게 만들기
			if(IsElite)
				Feedback.ChangeUnitColor(new Color(0.77f, 0.77f, 0.77f));
			else
				Feedback.ChangeUnitColor(new Color(0.47f, 0.47f, 0.47f));
		}

		//TODO: UI처리?
	}

	IEnumerator CorpseDecayCoroutine()
	{
		yield return new WaitForSeconds(Statics.CorpseDestroyTime);
		if (CurrentFaction != Faction.Human) yield break; //최후의 순간에 유닛이 인간이 아니면 아무것도 안함.
		
		//리바이브 할 수 없는 상태로 만들고 사라지기 시작.
		//CanRevive = false;
		
		StartCoroutine(DestroyCorpse());
		//StartCoroutine(StartEffectAndAbsorb());
	}
	
	public void KillSelf()
	{
		if (IsDead) return;
		_currentHp = 0;
		Die();
	}

	public void DieWithNoEvent()
	{
		Debug.Log("Unit | DieWithNoEvent");
		IsDead = true;

		_collider.isTrigger = true;
		gameObject.layer = CurrentFaction == Faction.Undead
			? Layers.UndeadCorpse
			: Layers.HumanCorpse;

		_humanRenderer.sortingOrder = 0;
		_undeadRenderer.sortingOrder = 0;

		AttackTarget = null;

		//피드백 전체 제거
		if (TryGetComponent(out FeedbackController feedback))
		{
			feedback.ResetAllFeedbacks();
		}
		Debug.Log(HpBarController.name);
		if (HpBarController != null) HpBarController.TurnOffHpBar();
		//죽음 애니메이션
		StartCoroutine(PlayDeathAnimation());

		BackToOriginalRotation();

		//시체 제거
		if (CurrentFaction == Faction.Undead)
		{
			StartCoroutine(DestroyCorpse());
			GameManager.Instance.GameUI.SetUndeadNumUI();
		}

		//시체 무기 생성
		if (CurrentFaction == Faction.Human)
		{
			CreateDeadWeapon();
		}
	}

	protected virtual IEnumerator StartAttackAnimRoutine(GameObject target_)
	{
		IsAttacking = true;

		//_currentTarget = target_.GetComponent<Unit>();
		AttackingTarget = AttackTarget;
		bool prevIsWalking = IsWalking;
		IsWalking = false;
		//애니메이션 실행
		CurrentAnim.Play(ATTACK);
		//애니메이션 끝날때 까지 기다리기.
		yield return new WaitForSeconds(GetClipLength(ATTACK));
		AttackingTarget = null;
		IsWalking = prevIsWalking;
		IsAttacking = false;
	}

	protected void UpdateFlip()
	{

		if (IsDead || _isStopAutoFlip) return;
		// if (_facingTarget == null && IsWalking)
		if(!isCanFlip) return;
		StartCoroutine(StartFlipTimer());

		if (IsWalking)
		{
			// bool shouldFaceLeft = _direction.x < 0;
			// if (_direction.x == 0) shouldFaceLeft = _rigid.velocity.x < 0;
			if (math.abs(_velocity.x) <= _flipInertia) return;
			bool shouldFaceLeft = _velocity.x < 0;

			Flip(shouldFaceLeft);
		}
		else if (AttackingTarget != null)
		{
			bool shouldFaceLeft = transform.position.x > AttackingTarget.transform.position.x;
			Flip(shouldFaceLeft);
		}
	}

	private void UpdateCorpseOverlapPreventingCollider()
	{
		if (IsDead && CurrentFaction == Faction.Human)
		{
			_corpseOverlapPreventingCollider.enabled = true;
		}
		else
		{
			_corpseOverlapPreventingCollider.enabled = false;
		}
	}
	
	IEnumerator StartFlipTimer(){
		isCanFlip = false;
		yield return new WaitForSeconds(_flipInertiaTime);
		isCanFlip = true;
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if (CurrentFaction == Faction.Human)
		{
			if (other.gameObject.name == "PlayerDamageTrail")
			{
				//칠흑을 걷는 자 트레일에 닿으면 데미지를 입는다.
				if (Time.time - _lastTrailDamagedTime >= 1f)
				{
					var _damage = other.TryGetComponent(out TrailController trailController) ? trailController.Damage : 0;
					if (_damage > 0)
					{
						IAttacker attacker = GameManager.Instance.GetPlayer();
						AttackInfo atkInfo = new AttackInfo(attacker, _damage, attackingMedium: other.transform);
						TakeDamage(atkInfo);
					}

					_lastTrailDamagedTime = Time.time;
				}
			}
		}
	}

	public void SetCurrentHpWithoutFeedback(int amount_)
	{
		CurrentHp = amount_;
	}

	protected void Flip(bool shouldFaceLeft = false)
	{
		Vector3 tmp = _flipableTransform.localScale;
		tmp = shouldFaceLeft ? new Vector3(Mathf.Abs(tmp.x) * -1, tmp.y, tmp.z) : new Vector3(Mathf.Abs(tmp.x), tmp.y, tmp.z);
		_flipableTransform.localScale = tmp;
	}
	public float GetFlipableLocalScaleX()
	{
		return _flipableTransform.localScale.x;
	}

	public bool IsFacingRight()
	{
		return _flipableTransform.localScale.x > 0;
	}

	protected void InitCircleShadow()
	{
		var shouldShowShadow = !IsDead || IsReviving;
		_circleShadow.SetActive(shouldShowShadow);
		Material material = _circleShadow.TryGetComponent(out SpriteRenderer sprRenderer) ? sprRenderer.material : null;
		if (CurrentFaction == Faction.Undead)
		{
			material?.SetColor("_Color", Statics.undeadShadowColor);
		}
		else
		{
			material?.SetColor("_Color", Statics.humanShadowColor);
		}
	}

	public void CreateBloodEffect()
	{
		var i = Random.Range(4, 8);
		for (int j = 0; j < i; j++)
		{
			GameObject bloodEffect = ManagerRoot.Resource.Instantiate("Effects/BloodEffect");
			bloodEffect.transform.position = transform.position;
			if (bloodEffect.TryGetComponent(out BloodEffectController bloodEffectController))
			{
				bloodEffectController.SetColorBasedOnFaction(CurrentFaction);
			}
			else
			{

				Debug.Log("BloodEffectController == null");
			}
		}
	}

	public void CreateDeadWeapon()
	{
		GameObject deadWeapon = ManagerRoot.Resource.Instantiate("Unit/Weapons/DeadWeapon");
		if (deadWeapon.TryGetComponent(out DeadWeaponController deadWeaponController))
		{
			deadWeaponController.SetParentUnitType(UnitType);
			deadWeaponController.SetLocalScale(_feedback.GetModelContainer().Find("@HumanModel").transform.localScale.x);
		}

		deadWeapon.transform.position = transform.position;
		deadWeapon.transform.parent = _feedback.GetHumanModel().transform;
	}

	public void CreateDarkEssence(uint _value)
	{
		GameObject darkEssence = ManagerRoot.Resource.Instantiate("Items/DarkEssence");
		if (darkEssence == null) { return; }
		darkEssence.transform.position = transform.position + Vector3.up * 1f;
		if (darkEssence.TryGetComponent(out DarkEssence darkEssenceController))
		{
			darkEssenceController.SetValue(_value);
			//TODO: 가치에 따라 이미지 변경? 크기 조절?
		}
	}

	public void DestroyDeadWeapon()
	{
		Transform deadWeapon = _feedback.GetHumanModel().transform.Find("DeadWeapon");
		if (deadWeapon != null)
		{
			Destroy(deadWeapon.gameObject);
		}
	}

	public IEnumerator StartEffectAndAbsorb()
	{
		if (TryGetComponent(out FeedbackController feedback))
		{
			feedback.ChangeMaterialBasedOnIsSelected(true);
			yield return new WaitForSeconds(0.1f);
			feedback.ChangeMaterialGhostGlitch(true);
		}
		
		foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
		{
			spriteRenderer.DOFade(0f, 2f);
		}
		
		yield return new WaitForSeconds(1.5f);
		UnitManager.DestroyUnit(this);
	}

	public IEnumerator ShowRangeCircleRoutine(float duration_, float range_)
	{
		if (_rangeCircle == null) yield break;
		
		_rangeCircle.transform.localScale = new Vector3(range_ * 2, range_, 1f);
		_rangeCircle.SetActive(true);

		yield return new WaitForSeconds(duration_);
		
		_rangeCircle.SetActive(false);
	}
	public virtual void StartRecall(Vector2 offset_, RecallData data_, bool isFirstUnit_)
	{
		StartCoroutine(RecallRoutine(offset_, data_, isFirstUnit_));
	}

	IEnumerator DestroyCorpse()
	{
		Debug.Log("Unit | DestroyCorpse");
		yield return new WaitForSeconds(.65f);
		
		if (IsDead == false || IsReviving) yield break;
		
		if (_currentFaction == Faction.Human)
		{
			if (_decreaseAlphaCoroutine != null) StopCoroutine(_decreaseAlphaCoroutine);
			_decreaseAlphaCoroutine = StartCoroutine(DecreaseAlpha(0.15f));
		}
		else
		{
			if (_decreaseAlphaCoroutine != null) StopCoroutine(_decreaseAlphaCoroutine);
			_decreaseAlphaCoroutine = StartCoroutine(DecreaseAlpha());
		}
		
	}
    
    IEnumerator DecreaseAlpha(float decreaseSpeed_ = 1f)
    {
		SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

		while (true)
		{
			foreach (var spriteRenderer in spriteRenderers)
			{
				if (spriteRenderer == null) continue;
				Color newColor = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, spriteRenderer.color.a - decreaseSpeed_ * Time.deltaTime);
				spriteRenderer.color = newColor;
			}
			yield return null;
			//check index
			if (spriteRenderers.Length == 0) break;
			if (spriteRenderers[0] == null) break;
			if (spriteRenderers[0].color.a <= 0)
			{
				break;
			}
		}
        
		UnitManager.DestroyUnit(this);
		StopAllCoroutines();
		
		
		//Destroy(gameObject);
	}

	IEnumerator RecallRoutine(Vector2 offset_, RecallData data_, bool isFirstUnit)
	{
		//모든 유닛이 기계적으로 같은 타이밍에 점프하지 않도록 랜덤 딜레이를 줌. 단, 최소 하나의 유닛은 무조건 딜레이 0초.
		float randomDelay = isFirstUnit ? 0 : Random.Range(0f, data_.MaxRandomDelay);
		yield return new WaitForSeconds(randomDelay);
		
		transform.SetParent(_player.transform);

		DOTween.Kill(transform);
		
		//그림자를 포함한 전체 오브젝트가 목적지로 이동함.
		transform.DOLocalMove(offset_, data_.Jumpduration)
			.SetEase(Ease.Linear)
			.OnComplete(() => transform.SetParent(null));
		
		List<string> teleportSoundList = new List<string>()
		{
			"Throwing Knife (Thrown) 4",
			"Throwing Knife (Thrown) 5",
			"Throwing Knife (Thrown) 6",
			"Throwing Knife (Thrown) 7",
			"Throwing Knife (Thrown) 12",
		};
		
		ManagerRoot.Sound.PlaySfx(teleportSoundList[Random.Range(0, teleportSoundList.Count)], 0.5f);
		
		//그림자를 제외한 몸체 부분은 로컬 포지션상으로 제자리 점프함.
		_bodyTransform.DOLocalJump(Vector3.zero, data_.JumpHeight, 1, data_.Jumpduration)
			.SetEase(Ease.Linear)
			.AppendCallback(() =>
			{
				ManagerRoot.Sound.PlaySfx("Staff Hitting (Flesh) 5", .3f);
			})
			.OnComplete(() =>
			{
				//도착 후 바운스 하는 애니메이션
				_bodyTransform.DOLocalJump(Vector3.zero, data_.BounceHeight, 1, data_.BounceDuration)
					.SetEase(Ease.Linear);
			});

		//Flip 설정
		bool shouldFaceLeft = _player.transform.position.x + offset_.x < transform.position.x;
		_isStopAutoFlip = true; //회전 애니메이션 끝나고 false로 돌려줌.
		Flip(shouldFaceLeft);

		//몸체의 회전 애니메이션
		int direction = shouldFaceLeft ? 1 : -1;
		_bodyTransform.DOLocalRotate(new(0, 0, 90 * direction), data_.Jumpduration)
			.OnComplete(() => _bodyTransform.DOLocalRotate(Vector3.zero, data_.RotateRollbackDuration) //바운스
				.OnComplete(() => _isStopAutoFlip = false));
	}

	public IEnumerator InvincibleRoutine(float duration_){
		IsInvincible = true;
		yield return new WaitForSeconds(duration_);
		IsInvincible = false;
	}
	public Transform GetAttackTransformByUnitType()
	{
		Transform attackTransform = new GameObject().transform;
		switch (UnitType)
		{
			case (int)global::UnitType.SpearMan:
				attackTransform.name = "Spear";
				break;
			case (int)global::UnitType.SwordMan:
				attackTransform.name = "Sword";
				break;
			case (int)global::UnitType.ArcherMan:
				attackTransform.name = "Archer";
				break;
			case (int)global::UnitType.Assassin:
				attackTransform.name = "Assassin";
				break;
			case (int)global::UnitType.HorseMan:
				attackTransform.name = "Horse";
				break;
			default:
				attackTransform.name = "Default";
				break;
		}

		return attackTransform;
	}

	public void TakeDamageCircleRange(float range, float damage)
	{
		List<Unit> units = ManagerRoot.Unit.GetUnitsInCircle(transform.position, range, CurrentFaction == Faction.Undead ? Faction.Human : Faction.Undead);
		foreach (Unit unit in units)
		{
			AttackInfo attackInfo = new AttackInfo(this, damage, attackingMedium: GetAttackTransformByUnitType());
			ManagerRoot.Event.onUnitAttack?.Invoke(unit, attackInfo);
			unit.TakeDamage(attackInfo);
		}
	}

	#region Animation Event Methods //----------------------------------------------
	//직접적으로 애니메이션 이벤트에서 실행되는건 아니고 UnitAnimationEvents 스크립트에서 한 다리 건너서 호출해야 함.
	//즉, 실제 애니메이션 이벤트는 UnitAnimationEvents 스크립트에 만들어야 선택지에 뜸.
	public virtual void AnimEvent_HitMoment()
	{
		if (AttackingTarget != null)
		{
			if (AttackingTarget.TryGetComponent<IDamageable>(out IDamageable damageable))
			{
				AttackInfo atkInfo = new AttackInfo(this, instanceStats.FinalAttackDamage, attackingMedium: GetAttackTransformByUnitType());
				PlayAttackSound();
				ManagerRoot.Event.onUnitAttack?.Invoke(this, atkInfo);
				damageable.TakeDamage(atkInfo);
			}
		}
	}
	public virtual void AnimEvent_GuardMoment()
	{
		if (AttackingTarget != null)
		{
			if (AttackingTarget.TryGetComponent<IDamageable>(out IDamageable damageable))
			{
				damageable.TakeKnockDown(transform);
			}
		}
	}
	public virtual void AnimEvent_HealMoment()
	{
		if (AttackingTarget != null)
		{
			if (AttackingTarget.TryGetComponent<IDamageable>(out IDamageable damageable))
			{
				damageable.TakeHeal(instanceStats.FinalAttackDamage, transform);
				//damageable.TakeDamage(-Info.AttackDamage, transform);
			}
		}
	}

	#endregion

	private void OnDrawGizmos()
	{
		Gizmos.color = CurrentFaction == Faction.Undead ? Color.magenta : Color.cyan;
		Gizmos.DrawWireCube(transform.position + Vector3.up * .4f, new Vector3(1.1f, 1.1f, 1));
	}
	private void OnDrawGizmosSelected()
	{
		DrawAIGizmos();
	}

	public void CorpseSelected()
	{
		if (TryGetComponent(out FeedbackController feedback))
		{
			feedback.ChangeMaterialBasedOnIsSelected(true);
		}
	}

	public void CorpseUnselected()
	{
		if (TryGetComponent(out FeedbackController feedback))
		{
			feedback.ChangeMaterialBasedOnIsSelected(false);
		}
	}
	
	#region util //----------------------------------------------
	private bool CheckStunFlag(int flag_){
		// check stun flag, flag로 인해 스턴중이면 true
		return (_isStun & flag_) != 0;
	}
	private void SetStunFlag(int flag_, bool isStun_){
		// true일때 stun flag 1로 토글
		if(isStun_){
			_isStun |= flag_;
		} 
		// false일때 stun flag 0으로 토글
		else{ 
			_isStun &= ~flag_;
		}
		
		if(_isStun != 0){ // 스턴 상태일때
			_behaviorTree.enabled = false;
			IsAttacking = false;
			IsSpecialAttacking = false;
			IsEnableMove = false;
			IsEnableAttack = false;
		}
		else { // 스턴 상태 아닐때
			_behaviorTree.enabled = true;
			IsEnableMove = true;
			IsEnableAttack = true;
		}
	}
	
	public static List<Collider2D> GetUnitListEcllipse(Vector3 centerPoint, float width_, float height_, LayerMask targetLayerMask_)
	{
		var targetUnits = Physics2D.OverlapAreaAll(
			new Vector2(centerPoint.x - width_, centerPoint.y - height_),
			new Vector2(centerPoint.x + width_, centerPoint.y + height_),
			targetLayerMask_
		);
		List<Collider2D> unitList = new List<Collider2D>();
		foreach(Collider2D unitCollider in targetUnits){
			if(unitCollider == null) break;
			unitList.Add(unitCollider);
		}
		return unitList;
	}
	#endregion

}

public enum Faction { Undead, Human }

public enum UndeadForm { Normal, Zombie, Skeleton, Specter, }

public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
{
	public AnimationClipOverrides(int capacity) : base(capacity) { }
	public AnimationClip this[string name]
	{
		get { return this.Find(x => x.Key.name.Equals(name)).Value; }
		set
		{
			int index = this.FindIndex(x => x.Key.name.Equals(name));
			if (index != -1)
				this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
		}
	}
}

//[Serializable]
public class InstanceStats
{
	//생성자
	public InstanceStats(Unit rootUnit_)
	{
		RootUnit = rootUnit_;
		BaseUnitStats = rootUnit_.GetBaseStats();
	}

	public Unit RootUnit { get; }
	public BaseUnitStats BaseUnitStats { get; }

	private List<StatModifier> _maxHpModifiers = new();
	private List<StatModifier> _maxHatredModifiers = new();
	private List<StatModifier> _moveSpeedModifiers = new();
	private List<StatModifier> _attackRangeModifiers = new();
	private List<StatModifier> _attackPerSecModifiers = new();
	private List<StatModifier> _attackDamageModifiers = new();
	private List<StatModifier> _maxAggroNumModifiers = new();
	private List<StatModifier> _damageReduceModifiers = new();
	private List<StatModifier> _healthDrainModifiers = new();
	
	private bool _isAnyPositiveModDurationRemains;

	//[ShowInInspector,ReadOnly]
	public int FinalMaxHp => Mathf.RoundToInt(GetModifiedStat(BaseUnitStats.BaseMaxHp, _maxHpModifiers));
	public float FinalMoveSpeed => GetModifiedStat(BaseUnitStats.BaseMoveSpeed, _moveSpeedModifiers);
	public float FinalAttackRange => GetModifiedStat(BaseUnitStats.BaseAttackRange, _attackRangeModifiers);
	public float FinalAttackPerSec
	{
		get
		{
			var atkSpd = GetModifiedStat(BaseUnitStats.BaseAttackPerSec, _attackPerSecModifiers);
			RootUnit.CurrentAnim.SetFloat("atkAnimSpd", atkSpd + .2f); //공격 애니메이션 배속은 공속 + 0.2(오프셋)
			return atkSpd;
		}
	}

	public int FinalAttackDamage => Mathf.RoundToInt(GetModifiedStat(BaseUnitStats.BaseAttackDamage, _attackDamageModifiers));
	public int FinalDamageReduce => Mathf.RoundToInt(GetModifiedStat(0, _damageReduceModifiers));
	public int FinalHealthDrain => Mathf.RoundToInt(GetModifiedStat(0, _healthDrainModifiers));
	//언데드 only
	public int? FinalMaxHatred
	{
		get
		{
			if (BaseUnitStats is UndeadUnitStats undeadUnitStats)
			{
				return Mathf.RoundToInt(GetModifiedStat(undeadUnitStats.BaseMaxHatred, _maxHatredModifiers));
			}
			else
			{
				return null;
			}
		}
	}

	public int? FinalMaxAggroNum
	{
		get
		{
			if (BaseUnitStats is UndeadUnitStats undeadUnitStats)
			{
				return Mathf.RoundToInt(GetModifiedStat(undeadUnitStats.BaseMaxAggroNum, _maxAggroNumModifiers));
			}
			else
			{
				return null;
			}
		}
	}
	
	public StatModifier GetModifierByName(string name_)
	{
		var allModifiers = new List<StatModifier>();
		allModifiers.AddRange(_maxHpModifiers);
		allModifiers.AddRange(_maxHatredModifiers);
		allModifiers.AddRange(_moveSpeedModifiers);
		allModifiers.AddRange(_attackRangeModifiers);
		allModifiers.AddRange(_attackPerSecModifiers);
		allModifiers.AddRange(_attackDamageModifiers);
		allModifiers.AddRange(_maxAggroNumModifiers);
		allModifiers.AddRange(_damageReduceModifiers);
		allModifiers.AddRange(_healthDrainModifiers);

		return allModifiers.Find(x => x.modifierName == name_);
	}

	//TODO: 리스트 하나로 합치기
	public void AddMaxHpModifier(StatModifier modifier_) => AddModifier(_maxHpModifiers, modifier_);
	public void AddMaxHatredModifier(StatModifier modifier_) => AddModifier(_maxHatredModifiers, modifier_);
	public void AddMoveSpeedModifier(StatModifier modifier_) => AddModifier(_moveSpeedModifiers, modifier_);
	public void AddAttackRangeModifier(StatModifier modifier_) => AddModifier(_attackRangeModifiers, modifier_);
	public void AddAttackPerSecModifier(StatModifier modifier_) => AddModifier(_attackPerSecModifiers, modifier_);
	public void AddAttackDamageModifier(StatModifier modifier_) => AddModifier(_attackDamageModifiers, modifier_);
	public void AddMaxAggroNumModifier(StatModifier modifier_) => AddModifier(_maxAggroNumModifiers, modifier_);
	public void AddDamageReduceModifier(StatModifier modifier_) => AddModifier(_damageReduceModifiers, modifier_);
	public void AddHealthDrainModifier(StatModifier modifier_) => AddModifier(_healthDrainModifiers, modifier_);

	public void TickModifierDurations()
	{
		var allModifiers = new List<StatModifier>();
		allModifiers.AddRange(_maxHpModifiers);
		allModifiers.AddRange(_maxHatredModifiers);
		allModifiers.AddRange(_moveSpeedModifiers);
		allModifiers.AddRange(_attackRangeModifiers);
		allModifiers.AddRange(_attackPerSecModifiers);
		allModifiers.AddRange(_attackDamageModifiers);
		allModifiers.AddRange(_maxAggroNumModifiers);
		allModifiers.AddRange(_damageReduceModifiers);

		bool previousState = _isAnyPositiveModDurationRemains;
		_isAnyPositiveModDurationRemains = false;
		for (int i = 0; i < allModifiers.Count; i++)
		{
			var mod = allModifiers[i];
			if (mod.duration <= 0)
			{
				mod.value = 0;
				continue;
			}
			else if (mod.duration < Mathf.Infinity && mod.value > 0)
			{
				_isAnyPositiveModDurationRemains = true;
			}
			mod.duration -= Time.deltaTime;
		}
		
		if (_isAnyPositiveModDurationRemains != previousState)
		{
			OnDurationModifierStateChanged();
		}
	}
	
	private void OnDurationModifierStateChanged()
	{
		if (RootUnit.TryGetComponent(out FeedbackController feedback))
		{
			feedback.GetEnforcedEffect().SetActive(_isAnyPositiveModDurationRemains);
		}
	}
	
	private float GetModifiedStat(float baseValue_, List<StatModifier> modifiers_)
	{
		float finalValue = baseValue_;
		float sumPercentAdd = 0;
		var finalModifiers = new List<StatModifier>();
		for (int i = 0; i < modifiers_.Count; i++)
		{
			StatModifier mod = modifiers_[i];
			switch (mod.type)
			{
				case StatModifierType.BaseAddition:
					finalValue += mod.value;
					break;
				case StatModifierType.Percentage:
					sumPercentAdd += mod.value;
					break;
				case StatModifierType.FinalPercentage:
					finalModifiers.Add(mod);
					break;
			}
		}
		finalValue *= (100 + sumPercentAdd) / 100f;

		foreach (var mod in finalModifiers)
		{
			finalValue *= (1 + (mod.value / 100f));
		}
		
		return finalValue;
	}

	private void AddModifier(List<StatModifier> modifiers_, StatModifier modifier_)
	{
		//HP를 보정하는 경우 이전 최대체력 기억해두기
		float originalResult = 0;
		if (modifier_.statType == StatType.MaxHp)
		{
			originalResult = FinalMaxHp;
		}

		//실제 보정치 적용
		var clonedModifier = new StatModifier(modifier_.statType, modifier_.modifierName, modifier_.value, modifier_.type, false, modifier_.duration);
		modifiers_.Add(clonedModifier);

		//HP를 보정하는 경우 현재 체력도 추가
		if (modifier_.statType == StatType.MaxHp)
		{
			var change = FinalMaxHp - originalResult;
			if (change < 0) change = 0;
			var curHp = RootUnit.CurrentHp + Mathf.RoundToInt(change);
			if (curHp > FinalMaxHp) curHp = FinalMaxHp;
			RootUnit.SetCurrentHpWithoutFeedback(curHp);
			RootUnit.HpBarController.UpdateHpBar();
		}
	}
}

[Serializable]
public class StatModifier
{
	public StatModifier(StatType statType, string modifierName, float value, StatModifierType type, bool shouldBeDisplayed, float duration = Mathf.Infinity)
	{
		this.statType = statType;
		this.modifierName = modifierName;
		this.value = value;
		this.type = type;
		this.shouldBeDisplayed = shouldBeDisplayed;
		this.duration = duration;
	}

	public StatType statType;
	[HideInInspector]
	public string modifierName;
	public float value;
	public StatModifierType type;
	public float duration;
	public bool shouldBeDisplayed;
}

public enum StatModifierType
{
	BaseAddition,
	Percentage,
	FinalPercentage
}

public enum StatType
{
	MaxHp,
	MaxHatred,
	MoveSpeed,
	AttackRange,
	AttackPerSec,
	AttackDamage,
	MaxAggroNum,
	DamageReduce,
	HealthDrain,
}

public class UnitGroup
{
	public List<Unit> MemeberUnits { get; } = new();
}