using System;
using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using UnityEngine;
using Random = UnityEngine.Random;

public class PriestSuccubusUnit : Unit
{
    GameObject healProjectile;
    GameObject charmProjectile;

    private GameObject healingFan;
    private GameObject charmingFan;
    
    private Vector3 charmingOffset = new Vector3(.5f, 0.25f, 0f);
    private Vector3 healingOffset = new Vector3(1f, 0.5f, 0f);
    
    public float CharmCoolTime = 10f; // 매혹 쿨타임
    public float CharmDuration = 5f; // 매혹 지속시간
    
    
    
    // Todo : 스킬 사운드 변경?? 혹은 제거. 현재는 활과 같은 사운드 사용중
    private List<string> _attackAudioList = new List<string>()
    {
        "Throwing Knife (Thrown) 1",
        "Throwing Knife (Thrown) 2",
        "Throwing Knife (Thrown) 8",
        "Throwing Knife (Thrown) 9",
        "Throwing Knife (Thrown) 12",
    };
    public override void Start()
    {
        base.Start();
        reviveAudio = "Debuff Downgrade 37";
        _isRangedDealer = true;

        healProjectile = Resources.Load<GameObject>("Prefabs/Unit/Weapons/HealProjectile");
        charmProjectile = Resources.Load<GameObject>("Prefabs/Unit/Weapons/CharmProjectile");
        healingFan = Resources.Load<GameObject>("Prefabs/Unit/Weapons/HealingFan");
        charmingFan = Resources.Load<GameObject>("Prefabs/Unit/Weapons/CharmingFan");

        // _addMoveWeightBehaviorUndead = new ArcherManAddMoveWeightBehavior_U();
        // reviveAudio = "Zombie Hit_02";
        _addAttackWeightBehaviorHuman = new PriestAddAttackWeightBehavior_H();
        _addMoveWeightBehaviorHuman = new PriestAddMoveWeightBehavior_H();

        
    }
    protected override void Init()
    {
        base.Init();
        if(CurrentFaction == Faction.Undead){
            IsSpecialAttackable = true;
            UIWinPopup.IsFamilyImage[(int)UIWinPopup.Images.SuccubusImage] = true;

			SteamUserData.IsSuccubusRevived = true;
		}
        else{
            targetLayerMask = Layers.HumanUnit.ToMask();
        }
    }
    public override void AnimEvent_HealMoment()
    {
        // Todo : 현재 힐/매혹 프로젝타일을 활과 같은거 쓰고있는데 스킬에 맞게 변경 필요
        bool canAttack = AttackTarget != null && (AttackTarget.layer == Layers.Player || !AttackTarget.GetComponent<Unit>().IsDead) && !IsStun;
        if(canAttack ){       
            if(CurrentFaction == Faction.Undead){
                GameObject charmingFanGO = Instantiate(charmingFan);
                var offset = charmingOffset;
                if (!IsFacingRight())
                {
                    offset.x = Mathf.Abs(offset.x) * (-1);
                }
                charmingFanGO.transform.position = transform.position + offset;
                FanSkillController fanSkillController = charmingFanGO.GetComponent<FanSkillController>();
                fanSkillController.IsCharmMode = true;
                fanSkillController.ParticleDelay = .05f;
                Vector3 dir = AttackTarget.transform.position - transform.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                charmingFanGO.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                fanSkillController.Init(10f, 15f, 0.5f);
                
                
                // Todo : 매혹 프로젝타일 변경 혹은 이펙트 추가. Assets/_Scripts/Unit/UnitCore/CharmProjectile.cs
                // GameObject projectileGO = ManagerRoot.Resource.Instantiate(charmProjectile);
                // projectileGO.transform.position = transform.position;
                // ParabolaProjectile parabolaProjectile = projectileGO.GetComponent<ParabolaProjectile>();
                // parabolaProjectile.Init(this,1.5f, 50f, instanceStats.FinalAttackDamage, AttackTarget.transform);
                // parabolaProjectile.FireProjectile(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition));
                var charmSounds = new List<string> {"Fire Punch Larger 3", "Fire Punch Larger 4", "Fire Punch Larger 5"};
                ManagerRoot.Sound.PlaySfx(charmSounds[Random.Range(0, charmSounds.Count)], 1f);
            }     
            else{
                GameObject healingFanGO = Instantiate(healingFan);
                var offset = healingOffset;
                if (!IsFacingRight())
                {
                    offset.x = Mathf.Abs(offset.x) * (-1);
                }
                // Debug.Log(healingOffset);
                healingFanGO.transform.position = transform.position + offset;
                FanSkillController fanSkillController = healingFanGO.GetComponent<FanSkillController>();
                fanSkillController.IsCharmMode = false;
                fanSkillController.ParticleDelay = .02f;
                Vector3 dir = AttackTarget.transform.position - transform.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                healingFanGO.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                fanSkillController.Init(10f, 15f, .5f);
                
                
                // Todo : 힐 프로젝타일 변경 혹은 이펙트 추가. Assets/_Scripts/Unit/UnitCore/HealProjectile.cs
                // GameObject projectileGO = ManagerRoot.Resource.Instantiate(healProjectile);
                // projectileGO.transform.position = transform.position;
                // ParabolaProjectile parabolaProjectile = projectileGO.GetComponent<ParabolaProjectile>();
                // parabolaProjectile.Init(this,1.5f, 50f, instanceStats.FinalAttackDamage, AttackTarget.transform);
                // parabolaProjectile.FireProjectile(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition));
                ManagerRoot.Sound.PlaySfx("Notification sound 14", 1f);
                ManagerRoot.Sound.PlaySfx("Powerup upgrade 1", 1f);
                ManagerRoot.Sound.PlaySfx("Powerup upgrade 18", 1f);
            }
        }
    }


	public override void TakeCharm(float charmDuration_, AttackInfo info_)
	{
        base.TakeCharm(CharmDuration, info_);

		targetLayerMask = Layers.UndeadUnit.ToMask() | Layers.Player.ToMask();
	}

	protected override IEnumerator CharmRoutine(float charmDuration_)
	{
		yield return new WaitForSeconds(charmDuration_);
		
		targetLayerMask = Layers.HumanUnit.ToMask();

	}
    
    public override void AIDetectAround(Collider2D[] detectEnemies_){
        if(IsSpecialAttacking){
            return;
        }
        else{
            base.AIDetectAround(detectEnemies_);
        }
    }
    
    public override EnumBTState AIAttackCheck(){  
        if(IsSpecialAttacking){
            return EnumBTState.Running;
        }   
        return base.AIAttackCheck();    
    }
    public override bool AISpecialAttack(){
        if(IsSpecialAttacking) return true;
        if(IsSpecialAttackable){
            AIStop();
            IsSpecialAttackable = false;

            StartCoroutine(StartSpecialAttackRoutine());
            return true;
        }
        return false;
    }

    protected IEnumerator SpecialAtkCooltimeRoutine(){
        yield return new WaitForSeconds(CharmCoolTime);
        IsSpecialAttackable = true;
    }
    
    protected IEnumerator StartSpecialAttackRoutine()
    {
        IsSpecialAttacking = true;
        
        // 애니메이션 실행
        // 매혹
        if(CurrentFaction == Faction.Human){
            CurrentAnim.Play(ATTACK);

            yield return new WaitForSeconds(GetClipLength(ATTACK));
        }
        else{
            CurrentAnim.Play(EXTRA_1);

            yield return new WaitForSeconds(GetClipLength(EXTRA_1));
        }
        
        IsSpecialAttacking = false;
        AIStop();
        StartCoroutine(SpecialAtkCooltimeRoutine());
        
        // AttackingTarget = null;
        IsSpecialAttacking = false;
    }
    
    protected override void AIIdle_H(){
        if(IsCampUnit && !IsAggressive)
        {
            AIGoToDestination(CampPosition, 0.5f);
        }
        else
        {
            Unit human = GetNearestHuman();
            if(human == null){
                if(_moveWeightList.Count > 0)
                    AIMove();
                else 
                    AIStop();
            }
            else{
                AIGoToDestination(human.transform, 4f);
            }
        }
        // AIStop();
    }

    protected override void AIGoToDestination(Transform transform_, float _arriveDistance){
        float dist = Vector3.Distance(transform_.position, transform.position);
        if(dist >= _arriveDistance){
            MoveDestination = transform_;
            AddDestinationWeight(_arriveDistance);
            // AddWeight(AIPathUpdate(), (dist -_limitPlayerDistance) / dist * _targetMaxWeight);
            AIMove();
        }
        else{
            if(_moveWeightList.Count > 0)
                AIMove();
            else 
                AIStop();
        }
    }

	public Unit GetNearestHuman()
	{
		List<Unit> humanUnits = ManagerRoot.Unit.GetAllAliveHumanUnits();
		Unit nearestTarget = null;
		float nearestDistance = float.MaxValue;

		foreach (Unit humanUnit in humanUnits)
		{
            if(humanUnit == this) continue;
			Vector3 dist = humanUnit.transform.position - transform.position;
			if (dist.magnitude < nearestDistance)
			{
				nearestTarget = humanUnit;
				nearestDistance = dist.magnitude;
			}
		}
		return nearestTarget;
	}
}
