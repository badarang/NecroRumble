using System.Collections;
using System.Xml.Serialization;
using BehaviorDesigner.Runtime.Tasks.Unity.Timeline;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class AssassinUnit : Unit
{
	// GhostStep
	private Coroutine ghostStepLoopCo; 
	private Coroutine ghostStepCo;
	bool isGhostStep = false;


	// HiddenStep
	public bool canHiddenBlade = false;
	public int hiddenBladeCount = 1;
	public override void Start() {
		base.Start();
		reviveAudio = "Ghost Attack_01_Edit Lower";
		reviveVolume = .1f;
	}

    protected override void Init(){
        base.Init();
        if(CurrentFaction == Faction.Undead){
            UIWinPopup.IsFamilyImage[(int)UIWinPopup.Images.GhostImage] = true;

			SteamUserData.IsAssassinRevived = true;
			_addAttackWeightBehaviorUndead = new AssassinAttackWeightBehavior_U();
        }
	}

	protected override void FixedUpdate() {
		base.FixedUpdate();
		if(isGhostStep && _velocity != Vector2.zero){
			_rigid.MovePosition((Vector2)transform.position + _velocity*Time.fixedDeltaTime);
			Flip(_velocity.x < 0);
		}
	}
	
	public void SetHiddenBlade(int hiddenBladeCount_){
		canHiddenBlade = true;
		hiddenBladeCount = hiddenBladeCount_;
	}
	protected override IEnumerator StartAttackAnimRoutine(GameObject target_)
	{
		IsAttacking = true;

		//_currentTarget = target_.GetComponent<Unit>();
		AttackingTarget = AttackTarget;
		bool prevIsWalking = IsWalking;
		IsWalking = false;
		//애니메이션 실행
		if(canHiddenBlade){
			// hidden blade
			CurrentAnim.speed = hiddenBladeCount + 1;
			CurrentAnim.Play(ATTACK);
			yield return new WaitForSeconds(GetClipLength(ATTACK)/CurrentAnim.speed + 0.05f); 
			// 참고 : 뒤에 나오는 CurrentAnim.Play(ATTACK)가 실행 안되는 문제가 있어서 위의 wait에서 딜레이를 0.05정도 주었더니 버그 고쳐짐
			for(int i = 0; i < hiddenBladeCount; ++i){
				if(AttackingTarget.TryGetComponent(out Unit unit_) && unit_.IsDead) break;
				//애니메이션 실행
				CurrentAnim.Play(ATTACK);
				//애니메이션 끝날때 까지 기다리기.
				yield return new WaitForSeconds(GetClipLength(ATTACK)/CurrentAnim.speed+ 0.05f);
			}
			canHiddenBlade = false;
			CurrentAnim.speed = 1;
		}
		else{
			CurrentAnim.Play(ATTACK);
			yield return new WaitForSeconds(GetClipLength(ATTACK));

		}
		AttackingTarget = null;
		IsWalking = prevIsWalking;
		IsAttacking = false;
	}

	public void StartGhostStepCoolTimeRoutine(float cooTime_, float duration_, float velocityMultiplier_, float fearRange_, float fearDuration_, float fearCoolTime_)
	{
		if (ghostStepLoopCo != null) StopCoroutine(ghostStepLoopCo);
		ghostStepLoopCo = StartCoroutine(GhostStepLoopCooldownCo(cooTime_, duration_, velocityMultiplier_, fearRange_, fearDuration_, fearCoolTime_));
	}

	private IEnumerator GhostStepLoopCooldownCo( float coolTime_, float duration_, float velocityMultiplier_, float fearRange_, float fearDuration_, float fearCoolTime_){
		while(true)
		{
			yield return new WaitForSeconds(coolTime_);
			
			//이 악령 유닛이 죽은 상태일 경우 루틴 종료
			if(this == null || IsDead)
			{
				break;
			}
			
			//매 인터벌마다 인지 범위 내에 적 유닛이 있는지 체크
			const float CHECK_INTERVAL = 0.15f;
			float checkTimer = 0;
			while (true)
			{
				checkTimer += Time.deltaTime;

				if (checkTimer > CHECK_INTERVAL)
				{
					checkTimer = 0;
					//인지 범위 내에 적 유닛이 있다면 고스트스텝 루틴 실행후 루프에서 빠져나오기
					if (GetFarthestTarget() != null)
					{
						if (ghostStepCo != null) StopCoroutine(ghostStepCo);
						ghostStepCo = StartCoroutine(GhostStepRoutine(duration_, velocityMultiplier_, fearRange_, fearDuration_, fearCoolTime_));
						yield return new WaitForSeconds(duration_);
						break;
					}
				}

				yield return null;
			}
		}
	}
	
	private IEnumerator GhostStepRoutine( float duration_, float velocityMultiplier_, float fearRange_, float fearDuration_, float fearCoolTime_)
	{
		IsStun = true;
		// _collider.enabled = false; // 공격 안받음
		_collider.isTrigger = true; // 공격 받음
		isGhostStep = true;
		_undeadRenderer.color = Color.blue;
		float timer = 0;
		float fearTimer = 0;
		Unit target = null;

		while (timer < duration_ && !IsDead)
		{
			timer += Time.deltaTime;
			var farthestTarget = GetFarthestTarget();
			bool isTargetDead = target is not {IsDead: false};
			if (isTargetDead) 
			{
				target = GetFarthestTarget();
			}
			
			//타겟이 있는 경우 이동
			if (target != null)
			{
				var distance = Vector2.Distance(target.transform.position, transform.position);
				if (distance > .5f)
				{
					_velocity = velocityMultiplier_ * instanceStats.FinalMoveSpeed * (target.transform.position - transform.position).normalized;
				}
				else
				{
					target = null;
				}
			}
			else //타겟이 없는 경우 대기
			{
				//플레이어와 일정 거리 내에 들어올 때까지 플레이어에게로 이동.
				var distance = Vector2.Distance(_player.transform.position, transform.position);
				if (distance > 1.5f)
				{
					_velocity = velocityMultiplier_ * instanceStats.FinalMoveSpeed * (_player.transform.position - transform.position).normalized;
				}
				else
				{
					_velocity = Vector2.zero;
				}
			}
			
			//타겟 유무와 관계없이 공포 범위 효과 적용
			fearTimer += Time.deltaTime;
			if (fearTimer >= fearCoolTime_)
			{
				fearTimer = 0;
				var units = ManagerRoot.Unit.GetUnitsInCircle(transform.position, fearRange_, Faction.Human);
				foreach (var unit in units)
				{
					if (unit is not {IsDead: false}) continue;
					if (unit.IsFearState) continue;
					
					var fearAtkInfo = new AttackInfo(this, 0);
					unit.TakeFear(1f, transform.position, fearDuration_, fearAtkInfo);
				} 
			}

			yield return null;
		}
		
		//고스트 스텝 종료 처리
		_undeadRenderer.color = Color.white;
		isGhostStep = false;
		_collider.isTrigger = false;
		// _collider.enabled = true;
		IsStun = false;
	}

	
	Unit GetFarthestTarget()
	{
		var targetList = ManagerRoot.Unit.GetUnitsInCircle(transform.position, Statics.NormalDetectRadius, Faction.Human);
		Unit farthestTarget = null;
		float dist = 0;
		
		//공포 걸린 유닛은 무시하고 선별
		foreach(Unit target in targetList)
		{
			if(target is not {IsDead: false}) continue; //살아 있지 않거나 null이면 패스.
			if (target.IsFearState) continue; //이미 공포에 걸린 적은 타겟으로 삼지 않음.
			float tmpDist = Vector3.Distance(target.transform.position, transform.position);
			if(tmpDist > dist)
			{
				dist = tmpDist;
				farthestTarget = target;
			}
		}
		
		//공포 걸리지 않은 유닛은 없다면 공포 걸린 유닛 중 가장 먼 유닛 가져오기
		if (farthestTarget == null)
		{
			foreach(Unit target in targetList)
			{
				if(target is not {IsDead: false}) continue; //살아 있지 않거나 null이면 패스.
				float tmpDist = Vector3.Distance(target.transform.position, transform.position);
				if(tmpDist > dist)
				{
					dist = tmpDist;
					farthestTarget = target;
				}
			}
		}
		
		return farthestTarget;
	}
}
