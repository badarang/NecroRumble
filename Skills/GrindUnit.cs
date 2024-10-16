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

public class GrindUnit : SkillBase
{
	private Camera _cam => Camera.main;
	private Player _player;
	private PlayerMovement _playerMove;

	private RangeSelector _rangeSelector;

	private TutorialManager _tutorial;
	[SerializeField] private float _removeRange;
    
	public bool isFirstGrind = false;
	bool _canGrindGameState;

	GrindUnitData _data;

	private List<String> _soundList = new List<string>()
	{
		"Body Flesh 1",
		"Body Flesh 2",
		"Body Flesh 10",
	};

	private GameObject _lineObject;
	private LineRenderer _lineRenderer;
	private Vector3 offset = new Vector3(0, .5f, 0);
	InputManager _input => ManagerRoot.Input;
	
	Vector2 _lineStartPos;
	Vector2 _lineEndPos;
	Vector2 _lineDirec;

	Vector2 _attackDirec;
	
	bool _isCasting;
	float failSoundDelay = 0f;
	
	public float RemoveRange => _removeRange;
	void Start()
	{
		_data = LoadData<GrindUnitData>();
		_player = (GameManager.Instance == null) ? FindObjectOfType<Player>() : GameManager.Instance.GetPlayer();
		_playerMove = GetComponent<PlayerMovement>();
		
		_lineObject = transform.Find("AttackRangeLine").gameObject;
		_lineRenderer = _lineObject.GetComponent<LineRenderer>();

		_lineRenderer.startColor = _data.lineCol;
		_lineRenderer.endColor   = _data.lineCol;
		

		// SetRangeSelector(_data._rangeSelectorType);
		//_rangeSelector = new EclipseRangeSelector(Layers.UndeadUnit.ToMask(), _removeRange);
		//_rangeSelector = new 
		Init();
	}

	void OnEnable()
	{
		Init();
	}

	void Init()
	{
		if (GameManager.Instance == null) return;

		// ManagerRoot.Input.OnGrindPressed -= ExcuteGrind;
		// ManagerRoot.Input.OnGrindPressed += ExcuteGrind;

		GameManager.Instance.GameStateChanged -= OnGameStateChanged;
		GameManager.Instance.GameStateChanged += OnGameStateChanged;
	}

	void Update()
	{
		// if (CurrentCooldown <= 0 && _canGrindGameState && _player.CanAction)
		// {
		// 	_rangeSelector.RangeSelect();
		// }
		TickCooldown();
		
		if (_input.GrindButton)
		{
			if(SceneManagerEx.CurrentScene.SceneType == SceneType.Tutorial)
			{
				if(_tutorial == null)
				{
					_tutorial = FindAnyObjectByType<TutorialManager>();
				}

				if (_tutorial.state != TutorialManager.EnumTutorialState.GrindEvent) return;
			}
						
			if (GameUIManager.Instance.IsReward || _player.CanAction == false)
			{
				_isCasting = false;
				_lineObject.SetActive(false);
				return;
			}

			if (IsCooldown)
			{
				PlayFailSound();
				GameUIManager.Instance.SetSkillCoolDownText();
				GameUIManager.Instance.SetGrindDoScale();
				return;
			}
			if (_player.CanAction == false) { PlayFailSound(); return; }
			
			_isCasting = true;
			
			if (_isCasting)
			{
				_lineRenderer.startWidth = 1f;
				_lineRenderer.endWidth   = 1f;
				DrawAttackRange();
			}
		}
		
		if (_isCasting && !_input.GrindButton) // 그라인드 갈고리 발사
		{
			_isCasting = false;
			_lineObject.SetActive(false);

			if (_lineDirec == Vector2.zero) return;

			Vector2 newPos = (Vector2)(transform.position + offset);
			CreateGrindHook(newPos, _attackDirec.normalized);

			MaxCooldown = _data.GetCurSkillData(SkillLevel).Cooldown;
			CurrentCooldown = MaxCooldown;

			GameUIManager.Instance.SetGrindCooldownUI();
			//_player.CostMp(_data.requiredMP);
		}
		
		if (failSoundDelay > 0) failSoundDelay -= Time.deltaTime;
	}
	
	private void CreateGrindHook(Vector2 position, Vector2 direction)
	{
		GameObject boneSpear = Instantiate(_data.grindHookPrefab, position, Quaternion.identity);
		if (boneSpear.TryGetComponent(out GrindHookProjectile grindHookProjectile))
		{
			ManagerRoot.Sound.PlaySfx("Flame Whoosh 4");
			grindHookProjectile.Init(_player, _data.speed, _data.destroyRange, 1);
		
			grindHookProjectile.SetDirection(direction);
			grindHookProjectile.SetRotation(direction, true);
		
		}
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

	private void DrawAttackRange()
	{
		if (_player.CanAction == false)
		{
			_lineObject.SetActive(false);
			_isCasting = false;
			return;
		}
		_lineObject.SetActive(true);

		Vector2 newPos = (Vector2)(transform.position + offset);

		//Unit _targetOutlineUndead = null;
		
		if (ManagerRoot.Input.LastUsedInputType == DeviceChangeListener.ControlDeviceType.Gamepad) //TODO: 컨트롤러를 꼽고 있는 상태에서 키보드로 조작하면 자동조준 됨. 이건 현재상황에서 어떻게 바꿀지 모르겠음. 
		{
			if (ManagerRoot.Input.AimButton) // 조이스틱 누르고 있는 상태
			{
				float _directionX = _input.AimHorizontal;
				float _directionY = _input.AimVertical;
				_lineDirec = new Vector2(_directionX, _directionY).normalized;
				_lineDirec = GetAimAssistDirection(_lineDirec);
			}
			else //RT만 누른 상태
			{
				//Nearst Aim assist
				Vector2 _aimDir = GetNearestUndeadDirection();
				if (_aimDir != Vector2.zero)
				{
					_lineDirec = _aimDir;
				}
			}
		}
		else // KeyboardAndMouse
		{
			_lineDirec = (Vector2)_cam.ScreenToWorldPoint(Input.mousePosition) - newPos;
		}


		_lineStartPos = newPos;
		_lineEndPos = newPos + _lineDirec.normalized * _data.destroyRange;

		_lineRenderer.SetPosition(0, _lineStartPos);
		_lineRenderer.SetPosition(1, _lineEndPos);

		_attackDirec = _lineDirec;

		_lineRenderer.startColor = _data.lineCol;
		_lineRenderer.endColor   = _data.lineCol;
	}
	void OnGameStateChanged()
	{
		_canGrindGameState = GameManager.Instance.State == GameManager.GameState.Round;
	}
	
	public void ExcuteGrind(Unit _unit)
	{
		//List<Unit> selectedUnits = new List<Unit>(_rangeSelector.SelectedQ);
		SetPlayerCanMove(_data._canMove);

		isFirstGrind = true;
		RemoveUnit(_unit);
		ResetValue();
		ManagerRoot.Event.onUnitGrind?.Invoke(_unit);

	}
	
	public Vector2 GetNearestUndeadDirection()
	{
		float minDist = float.MaxValue;
		Vector2 nearestDir = Vector2.zero;
		foreach (var enemy in ManagerRoot.Unit.GetAllAliveUndeadUnits())
		{
			float dist = Vector2.Distance(enemy.transform.position, transform.position);
			if (dist < minDist)
			{
				minDist = dist;
				nearestDir = (Vector2)enemy.transform.position - (Vector2)transform.position;
			}
		}
		return nearestDir;
	}

	public Unit GetNearestUndeadUnit()
	{
		float minDist = float.MaxValue;
		Unit nearestUnit = null;
		foreach (var enemy in ManagerRoot.Unit.GetAllAliveUndeadUnits())
		{
			float dist = Vector2.Distance(enemy.transform.position, transform.position);
			if (dist < minDist)
			{
				minDist = dist;
				nearestUnit = enemy;
			}
		}
		return nearestUnit;
	}
	
	public Vector2 GetAimAssistDirection(Vector2 _dir)
	{
		Vector2 targetPosition = GetTargetDir(Statics.AimAssistDegree, _dir);
		if (targetPosition == Vector2.zero)
		{
			return _dir;
		}
		return targetPosition;
	}
	
	public Vector2 GetTargetDir(float detectDeg, Vector2 _dir)
	{
		List<Unit> undeadUnits = ManagerRoot.Unit.GetAllAliveUndeadUnits();

		Unit nearestTarget = null;
		//float nearestDistance = float.MaxValue;
		float minDetectDeg = float.MaxValue;

		foreach (Unit undeadUnit in undeadUnits)
		{
			Vector3 toEnemy = undeadUnit.transform.position - transform.position;
			if (toEnemy.magnitude > _data.destroyRange) continue;

			float angleBetweenTwoVector = CalculateAngle(toEnemy, _dir);

			if (angleBetweenTwoVector < detectDeg)
			{
				// 이미 target이 있는 경우, 거리 비교
				if (nearestTarget != null)
				{
					if (angleBetweenTwoVector < minDetectDeg)
					{
						nearestTarget = undeadUnit;
						minDetectDeg = angleBetweenTwoVector;
					}
				}
				else
				{
					// target이 없는 경우
					nearestTarget = undeadUnit;
					minDetectDeg = angleBetweenTwoVector;
				}
			}
		}

		if (nearestTarget == null) return Vector2.zero;
		return nearestTarget.transform.position - transform.position;
	}
	
	public Unit GetTargetUndead()
	{
		List<Unit> undeadUnits = ManagerRoot.Unit.GetAllAliveUndeadUnits();

		Unit nearestTarget = null;
		//float nearestDistance = float.MaxValue;
		//float minDetectDeg = float.MaxValue;
		float minDist = float.MaxValue;

		foreach (Unit undeadUnit in undeadUnits)
		{
			Vector3 toEnemy = undeadUnit.transform.position - transform.position;
			if (toEnemy.magnitude > _data.destroyRange) continue;
			float dist = Vector2.Distance(undeadUnit.transform.position, transform.position);
			
			
			if (LineRendererOverlapsUnit(_lineRenderer.GetPosition(0), _lineRenderer.GetPosition(1)))
			{
				// 이미 target이 있는 경우, 거리 비교
				if (nearestTarget != null)
				{
					if (dist < minDist)
					{
						nearestTarget = undeadUnit;
						minDist = dist;
					}
				}
				else
				{
					// target이 없는 경우
					nearestTarget = undeadUnit;
					minDist = dist;
				}
			}
		}

		return nearestTarget;
	}
	
	private bool LineRendererOverlapsUnit(Vector2 start, Vector2 end)
	{
		int layerMask = Layers.UndeadUnit;
		RaycastHit2D hit = Physics2D.Linecast(start, end, layerMask);
		return hit.collider != null;
	}
	
	public float CalculateAngle(Vector2 vector1, Vector2 vector2)
	{
		float dotProduct = Vector2.Dot(vector1.normalized, vector2.normalized);
		float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

		return angle;
	}
	
	private void RemoveUnit(Unit _unit)
	{
		ManagerRoot.Sound.PlaySfx(_soundList[UnityEngine.Random.Range(0, _soundList.Count)], 0.6f);
		
		if (_unit == null)
		{
			Debug.LogError("!!!!!!!!!!!!!!!그라인드 중 Unit Null 임!!!!!!!!!!!!!!!!!!!!");
			return;
		}
		if (_unit.IsElite == false)
		{
			_unit.GainExp(ExpType.Grind);
		}
		_unit.DieWithNoEvent();
	}

	void ResetValue() // 가변값 초기화
	{
		SetPlayerCanMove(true);

		//CurrentCooldown = _data.GetCurSkillData(SkillLevel).Cooldown;

	}

	public void SetPlayerCanMove(bool canMove_)
	{
		_playerMove.CanMove = canMove_;
	}

	void PlayFailSound()
	{
		// 지우는게 나은거 같음
		if (failSoundDelay > 0) return;
		ManagerRoot.Sound.PlaySfx("Error Sound 4", volume_: 0.6f);
		failSoundDelay = 0.15f;
	}

	// void SetRangeSelector(RangeSelectorType type_)
	// {
	//     switch(type_)
	//     {
	//         case RangeSelectorType.CircleRangeSelector:
	//             _rangeSelector = new CircleRangeSelector(Layers.HumanCorpse.ToMask());
	//             break;
	//         case RangeSelectorType.EclipseRangeSelector:
	//             _rangeSelector = new EclipseRangeSelector2(Layers.HumanCorpse.ToMask(), _removeRange);
	//             break;
	//         case RangeSelectorType.ChargingRangeSelector:
	//             _rangeSelector = new ChargingRangeSelector(Layers.HumanCorpse.ToMask());
	//             break;
	//         case RangeSelectorType.FrontSquareRangeSelector:
	//             _rangeSelector = new FrontSquareRangeSelector(Layers.HumanCorpse.ToMask());
	//             break;
	//         case RangeSelectorType.FrontCircleRangeSelector:
	//             _rangeSelector = new FrontCircleRangeSelector(Layers.HumanCorpse.ToMask());
	//             break;
	//         case RangeSelectorType.PointerCircleRangeSelector:
	//             _rangeSelector = new PointerCircleRangeSelector(Layers.HumanCorpse.ToMask());
	//             break;
	//         case RangeSelectorType.UnderfootRangeSelector:
	//             _rangeSelector = new UnderfootRangeSelector();
	//             break;
	//     }
	// }



	public override void OnBattleStart() { }
	public override void OnBattleEnd() { }
	public override void OnSkillUpgrade() { }
	public override void OnSkillAttained()
	{
	}
}