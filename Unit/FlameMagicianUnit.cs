using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tactical.Tasks;
using DG.Tweening;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using Unity.VisualScripting;
using UnityEngine;

public class FlameMagicianUnit : Unit
{
    public float SpecialAttackCoolTime = 5f;
    public float SpecialAttackCastingSpeed = 1f;
    const float CastingAnimTime = 2.4f;
    public float SpecialAttackDuration = 5f;
    // public float FireBallDamage = 10f;
    public float HumanFireBallSpeed = 1;
    public float UndeadFireBallSpeed = 1;
    private float _fireBallSpeed;

    GameObject _projectileHuman;
    GameObject _projectileUndead;
    GameObject _fireMagicCircle;
    Animator _fireMagicCircleAnim;
    GameObject _fireMagicCircleHuman;
    Animator _fireMagicCircleHumanAnim;
    GameObject _fireMagicCircleUndead;
    Animator _fireMagicCircleUndeadAnim;
    List<GameObject> _targetList;

    bool isTakeHit = false;

    public override void Start()
    {
        base.Start();
        reviveAudio = "Lava burning person 2 Larger";
        _projectileHuman = Resources.Load<GameObject>("Prefabs/Unit/Weapons/FireBallHuman");
        _projectileUndead = Resources.Load<GameObject>("Prefabs/Unit/Weapons/FireBallUndead");
        _addMoveWeightBehaviorUndead = new ArcherManAddMoveWeightBehavior_U();
        _isRangedDealer = true;
        
		_fireMagicCircleHuman = gameObject.FindChild("@FireMagicCircleHuman").gameObject;
		_fireMagicCircleHumanAnim = _fireMagicCircleHuman.GetComponent<Animator>();
		_fireMagicCircleUndead = gameObject.FindChild("@FireMagicCircleUndead").gameObject;
		_fireMagicCircleUndeadAnim = _fireMagicCircleUndead.GetComponent<Animator>();
    }

    protected override void Init()
    {
        base.Init();
        IsSpecialAttacking = false;
        StartCoroutine(SpecialAtkCooltimeRoutine());
        _targetList = new List<GameObject>();
        if (CurrentFaction == Faction.Human)
        {
            _fireMagicCircle = _fireMagicCircleHuman;
            _fireMagicCircleAnim = _fireMagicCircleHumanAnim;
            _fireBallSpeed = HumanFireBallSpeed;
        }
        else
        {
            UIWinPopup.IsFamilyImage[(int)UIWinPopup.Images.LichImage] = true;
			SteamUserData.IsLichRevived = true;

			_fireMagicCircleHuman.SetActive(false);
            _fireMagicCircle = _fireMagicCircleUndead;
            _fireMagicCircleAnim = _fireMagicCircleUndeadAnim;
            _fireBallSpeed = UndeadFireBallSpeed;
            SpecialAttackCastingSpeed *= 1.5f;
        }
        
        
    }

    public override void Update()
    {
        base.Update();
    }
    
	public override void TakeDamage(AttackInfo info_, Color32 color_ = default, SPECIALMODE specialMode_ = SPECIALMODE.NONE)
	{
        isTakeHit = true;
        base.TakeDamage(info_, color_, specialMode_ );
    }
    public override void AnimEvent_HitMoment()
    {
        bool canAttack = AttackTarget != null && (AttackTarget.layer == Layers.Player || !AttackTarget.GetComponent<Unit>().IsDead) && !IsStun;
        if(canAttack){
            AttackInfo atkInfo = new AttackInfo(this, instanceStats.FinalAttackDamage, attackingMedium: transform);
            ShootFire(AttackTarget, atkInfo);
            ManagerRoot.Event.onUnitAttack?.Invoke(this, atkInfo);
        }
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
        IsSpecialAttackable = false;
        yield return new WaitForSeconds(SpecialAttackCoolTime);
        IsSpecialAttackable = true;
    }
    
    protected IEnumerator StartSpecialAttackRoutine()
    {
        IsSpecialAttacking = true;
        isTakeHit = false;

        CurrentAnim.Play(EXTRA_1);
        if(_fireMagicCircle == null){
            if(CurrentFaction == Faction.Human){
                _fireMagicCircle = _fireMagicCircleHuman;
                _fireMagicCircleAnim = _fireMagicCircleHumanAnim;
            }
            else{
                _fireMagicCircle = _fireMagicCircleUndead;
                _fireMagicCircleAnim = _fireMagicCircleUndeadAnim;

            }
        }
        _fireMagicCircle.SetActive(true);
        _fireMagicCircleAnim.speed = SpecialAttackCastingSpeed;

        float timer = 0;
        float waitTime = CastingAnimTime / SpecialAttackCastingSpeed;
        while(true){
            if(IsUnitDisable()) break;
            if(isTakeHit) break;

            if(timer >= waitTime) break;
            
            yield return null;
            timer += Time.deltaTime;
        }

        // 메테오~ 쿠과과광
        timer = 0;
        float fireTimer = 0;
        while(true){
            if(isTakeHit || IsUnitDisable()){
                _fireMagicCircle.SetActive(false);
                break;
            }
            if(timer >= SpecialAttackDuration) {
                _fireMagicCircle.SetActive(false);
                CurrentAnim.Play(EXTRA_2);
                yield return new WaitForSeconds(GetClipLength(EXTRA_2));
                break;
            }
            if(fireTimer >= 1/_fireBallSpeed){
                fireTimer = 0;
                GameObject target = GetSpecialAttackTarget();
                AttackInfo atkInfo = new AttackInfo(this, instanceStats.FinalAttackDamage, attackingMedium: transform);
                ShootFire(target, atkInfo);
                if(CurrentFaction == Faction.Undead){
                    target = GetSpecialAttackTarget();
                    atkInfo = new AttackInfo(this, instanceStats.FinalAttackDamage, attackingMedium: transform);
                    ShootFire(target, atkInfo);
                    target = GetSpecialAttackTarget();
                    atkInfo = new AttackInfo(this, instanceStats.FinalAttackDamage, attackingMedium: transform);
                    ShootFire(target, atkInfo);
                }
            }
            yield return null;
            fireTimer += Time.deltaTime;
            timer += Time.deltaTime;
        }

        // End Skill
        if(!IsDead)
            CurrentAnim.Play(IDLE);
        _fireMagicCircle.SetActive(false);
        

        IsSpecialAttacking = false;       
        StartCoroutine(SpecialAtkCooltimeRoutine());
        
    }

    GameObject GetSpecialAttackTarget(){
        GameObject target = null;
        if(_targetList != null){
            for(int i = 0 ; i < _targetList.Count; ++i){
                target = _targetList[0];
                _targetList.RemoveAt(0);
                if(target == null || (target.TryGetComponent(out Unit unit) && unit.IsDead)){
                    continue;
                }
                break;
            }
        }
        if(target == null){
            Collider2D[] detectEnemies_ = new Collider2D[100];
            Physics2D.OverlapCircleNonAlloc(transform.position, instanceStats.FinalAttackRange*2, detectEnemies_, targetLayerMask);
            _targetList = new List<GameObject>();
            foreach(var detect in detectEnemies_){
                if(detect == null) break;
                if(detect.TryGetComponent(out Unit unit) && unit.IsDead) continue;
                _targetList.Add(detect.gameObject);
            }
            if(_targetList.Count == 0) return null;
            target = _targetList[0];
            _targetList.RemoveAt(0);
        }
        return target;
    }

    void ShootFire(GameObject target, AttackInfo attackInfo){
        if(target == null) return;
        GameObject projectileGO;
        if (CurrentFaction == Faction.Human)
        {
            projectileGO = ManagerRoot.Resource.Instantiate(_projectileHuman);
        }
        else
        {
            projectileGO = ManagerRoot.Resource.Instantiate(_projectileUndead);
        }
        projectileGO.transform.position = transform.position;
        FireProjectile fireProjectile = projectileGO.GetComponent<FireProjectile>();
        Vector2 targetDirection = target.transform.position - transform.position;
        Vector3 offset = _flipableTransform.localScale.x < 0 ?  new Vector3(-1f, 1.5f, 0f): new Vector3(1f, 1.5f, 0f);
        fireProjectile.transform.position = transform.position + offset;
        fireProjectile.Init(transform.position + offset, target.transform, targetLayerMask, attackInfo);
        var fireSounds = new List<string> {"Blow Torch Stop Larger 1", "Blow Torch Stop Larger 2", "Blow Torch Stop Larger 3", "Blow Torch Stop Larger 4"};
        ManagerRoot.Sound.PlaySfx(fireSounds[Random.Range(0, fireSounds.Count)], 1f);
    }
}
