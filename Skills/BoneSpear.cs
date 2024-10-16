 using System.Collections.Generic;
using UnityEngine;
using LOONACIA.Unity.Managers;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;

public class BoneSpear : SkillBase
{
	private Camera _cam => Camera.main;
	private GameObject _lineObject;
	private LineRenderer _lineRenderer;
	private Player _player;
	private Vector3 offset = new Vector3(0, .5f, 0);
	InputManager _input => ManagerRoot.Input;


	Vector2 _lineStartPos;
	Vector2 _lineEndPos;
	Vector2 _lineDirec;

	Vector2 _attackDirec;

	bool _isCasting;

	public bool isCanCasting = true;

	BoneSpearData _data;

	float failSoundDelay = 0f;
	private double failDelay;

	void Start()
	{
		_data = LoadData<BoneSpearData>();

		Id = _data.Id;
		Name = _data.Name;

		_player = GameManager.Instance.GetPlayer();


		_lineObject = transform.Find("AttackRangeLine").gameObject;
		_lineRenderer = _lineObject.GetComponent<LineRenderer>();

		_lineRenderer.startColor = _data.lineCol;
		_lineRenderer.endColor   = _data.lineCol;
	}

	void Update()
	{
		TickCooldown();

		if (_input.BoneSpearButton)
		{
			//if UI is open, return
			if (GameUIManager.Instance.IsReward || _player.CanAction == false)
			{
				_isCasting = false;
				_lineObject.SetActive(false);
				return;
			}

			if (IsCooldown)
			{
				if (failDelay <= 0f)
				{
					failDelay = .2f;
					PlayFailSound();
				}
				else
				{
					failDelay -= Time.deltaTime;
				}
				GameUIManager.Instance.SetSkillCoolDownText();
				GameUIManager.Instance.SetSpearDoScale();
				return;
			}

			// if (_player.CurMp < _data.requiredMP)
			// {
			// 	//Shake Mana Bar
			// 	GameUIManager.Instance.SetMpShakeUI(15f, .5f);
			// 	PlayFailSound();
			// 	return;
			// }
			if (_player.CanAction == false) { PlayFailSound(); return; }

			_isCasting = true;
		}

		if (_isCasting)
		{
			_lineRenderer.startWidth = .5f;
			_lineRenderer.endWidth   = .5f;
			DrawAttackRange();
		}

		if (_isCasting && !_input.BoneSpearButton) // 뼈창 발사
		{
			_isCasting = false;
			_lineObject.SetActive(false);

			if (_lineDirec == Vector2.zero) return;

			if (!isCanCasting) // 뼈창 못 쓰는 상황
			{
				//TODO: 화면 쉐이킹, 플레이어가 뼈창을 쏠 수 없다는 피드백
				return;
			}

			Vector2 newPos = (Vector2)(transform.position + offset);
			CreateBoneSpear(newPos, _attackDirec.normalized);

			UseBonSpearUI();
			
			// _player.CostMp(_data.requiredMP);
		}

		if (failSoundDelay > 0)
		{
			failSoundDelay -= Time.deltaTime;
		}
	}

	private void TickCooldown()
	{
		if (IsCooldown)
		{
			CurrentCooldown -= Time.deltaTime;
			
			if(CurrentCooldown < 0)
			{
				IsCooldown = false;
			}
		}
	}
	
	private void UseBonSpearUI()
	{
		MaxCooldown = _data.maxCooltime;
		CurrentCooldown = MaxCooldown;
		
#if UNITY_EDITOR
		if (_player.IsGodMode) CurrentCooldown = 0;
#endif

		GameUIManager.Instance.SetSpearCooldownUI();
	}

	private void CreateBoneSpear(Vector2 position, Vector2 direction)
	{
		GameObject boneSpear;
		if (ManagerRoot.Unit.CurCharacter == UnitManager.CharacterType.CutePlayer)
		{
			boneSpear = ManagerRoot.Resource.Instantiate(_data.cuteBoneSpearPrefab, position, Quaternion.identity);
		}
		else
		{
            boneSpear = ManagerRoot.Resource.Instantiate(_data.boneSpearPrefab, position, Quaternion.identity);
		}
		
		if (boneSpear.TryGetComponent(out BoneSpearProjectile boneSpearProjectile))
		{
			ManagerRoot.Sound.PlaySfx("Flame Whoosh 4");
			var damage = _data.damage;			
#if UNITY_EDITOR
			if (_player.IsGodMode) damage = 999;
#endif
			boneSpearProjectile.Init(_player, _data.speed, _data.destroyRange, damage);
            boneSpearProjectile.SetDirection(direction);
            boneSpearProjectile.SetRotation(direction, true, true);

		}
	}

	void PlayFailSound()
	{
		// 지우는게 나은거 같음
		if (failSoundDelay > 0) return;
		ManagerRoot.Sound.PlaySfx("Error Sound 4", 0.6f);
		failSoundDelay = 0.15f;
	}


	void DrawAttackRange()
	{
		if (_player.CanAction == false)
		{
			_lineObject.SetActive(false);
			_isCasting = false;
			return;
		}
		_lineObject.SetActive(true);

		Vector2 newPos = (Vector2)(transform.position + offset);

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
				Vector2 _aimDir = GetNearestEnemyDirection();
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
		
		if (!isCanCasting)
		{
			_lineRenderer.startColor = Color.red;
			_lineRenderer.endColor   = Color.red;
			return;
		}

		_lineRenderer.startColor = _data.lineCol;
		_lineRenderer.endColor = _data.lineCol;
	}

	public Vector2 GetTargetDir(float detectDeg, Vector2 _dir)
	{
		List<Unit> enemyUnits = ManagerRoot.Unit.GetAllAliveHumanUnits();

		Unit nearestTarget = null;
		//float nearestDistance = float.MaxValue;
		float minDetectDeg = float.MaxValue;

		foreach (Unit enemyUnit in enemyUnits)
		{
			Vector3 toEnemy = enemyUnit.transform.position - transform.position;
			if (toEnemy.magnitude > _data.destroyRange) continue;

			float angleBetweenTwoVector = CalculateAngle(toEnemy, _dir);

			if (angleBetweenTwoVector < detectDeg)
			{
				// 이미 target이 있는 경우, 거리 비교
				if (nearestTarget != null)
				{
					if (angleBetweenTwoVector < minDetectDeg)
					{
						nearestTarget = enemyUnit;
						minDetectDeg = angleBetweenTwoVector;
					}
				}
				else
				{
					// target이 없는 경우
					nearestTarget = enemyUnit;
					minDetectDeg = angleBetweenTwoVector;
				}
			}
		}

		if (nearestTarget == null) return Vector2.zero;
		return nearestTarget.transform.position - transform.position;
	}

	public Unit GetNearestHuman()
	{
		List<Unit> enemyUnits = ManagerRoot.Unit.GetAllAliveHumanUnits();
		Unit nearestTarget = null;
		float nearestDistance = float.MaxValue;

		foreach (Unit enemyUnit in enemyUnits)
		{
			Vector3 toEnemy = enemyUnit.transform.position - transform.position;
			if (toEnemy.magnitude > _data.destroyRange) continue;
			if (toEnemy.magnitude < nearestDistance)
			{
				nearestTarget = enemyUnit;
				nearestDistance = toEnemy.magnitude;
			}
		}
		return nearestTarget;
	}

	public float CalculateAngle(Vector2 vector1, Vector2 vector2)
	{
		float dotProduct = Vector2.Dot(vector1.normalized, vector2.normalized);
		float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

		return angle;
	}
	public float CalculateAngle(Vector2 vector2_)
	{
		float dotProduct = Vector2.Dot(_lineDirec.normalized, vector2_.normalized);
		float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

		return angle;
	}

	public Vector2 GetNearestEnemyDirection()
	{
		float minDist = float.MaxValue;
		Vector2 nearestDir = Vector2.zero;
		foreach (var enemy in ManagerRoot.Unit.GetAllAliveHumanUnits())
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

	public Vector2 GetAimAssistDirection(Vector2 _dir)
	{
		Vector2 targetPosition = GetTargetDir(Statics.AimAssistDegree, _dir);
		if (targetPosition == Vector2.zero)
		{
			return _dir;
		}
		return targetPosition;
	}

	public override void OnBattleStart() { }
	public override void OnBattleEnd() { }
	public override void OnSkillUpgrade() { }
	public override void OnSkillAttained()
	{
	}
}
