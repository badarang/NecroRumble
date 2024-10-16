using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LOONACIA.Unity.Managers;
using MoreMountains.Tools;
using Unity.VisualScripting;
using UnityEngine;

public class ArcherManUnit : Unit
{
    //private float _kitingRange = 2f; // just to prevent warning log by cyrano
    //private bool _isBack = false; // just to prevent warning log by cyrano
    GameObject projectile;
    public GameObject secondAttackTarget;
    public GameObject thirdAttackTarget;
	
    [Header("RapidShot")]
    private float _rapidShotCount = 0;
    public float RapidShotCount
    {
        get => _rapidShotCount;
        set => _rapidShotCount = value;
    }

    public float ProjectileSpeedMultiplier => Mathf.Lerp(1, instanceStats.FinalAttackPerSec, 0.5f);
    
    [Header("BlackArrow")]
    private Coroutine _blackArrowCoroutine;
    private float _blackArrowShotDelay = 0f;
    public int BlackArrowLevel = 0;
    public float BlackArrowShotDelay = 0f;
    public float BlackArrowDamageMultiplier = 0f;
    
    [Header("HeavyAttack")]
    public float HeavyAttackSlowPercent = 0f;
    public float HeavyAttackSlowDuration = 0f;
    
    private List<string> _attackAudioList = new List<string>()
    {
        // "Releasing string (Bow) 3", 
        // "Releasing string (Bow) 7", 
        // "Releasing string (Bow) 9",
        // "Releasing string (Bow) 10",
        // "Releasing string (Bow) 15",
        "Throwing Knife (Thrown) 1",
        "Throwing Knife (Thrown) 2",
        "Throwing Knife (Thrown) 8",
        "Throwing Knife (Thrown) 9",
        "Throwing Knife (Thrown) 12",
    };
    public override void Start()
    {
        base.Start();
        projectile = Resources.Load<GameObject>("Prefabs/Unit/Weapons/TestArrow");
        _addMoveWeightBehaviorUndead = new ArcherManAddMoveWeightBehavior_U();
        reviveAudio = "Zombie Hit_02";
        _isRangedDealer = true;
    }
    protected override void Init(){
        base.Init();
        if(CurrentFaction == Faction.Undead){
            UIWinPopup.IsFamilyImage[(int)UIWinPopup.Images.ArcherImage] = true;

			SteamUserData.IsArcherRevived = true;
		}
	}

    public override void Update()
    {
        base.Update();
        if (_blackArrowShotDelay > 0)
        {
            _blackArrowShotDelay -= Time.deltaTime;
        }
    }
    public override void AnimEvent_HitMoment()
    {
        bool canAttack = AttackTarget != null && (AttackTarget.layer == Layers.Player || !AttackTarget.GetComponent<Unit>().IsDead) && !IsStun;
        if(canAttack){
            AttackInfo atkInfo = new AttackInfo(this, instanceStats.FinalAttackDamage, attackingMedium: transform);
            ManagerRoot.Event.onUnitAttack?.Invoke(this, atkInfo);
            GameObject projectileGO = ManagerRoot.Resource.Instantiate(projectile);
            projectileGO.transform.position = transform.position;
            ParabolaProjectile parabolaProjectile = projectileGO.GetComponent<ParabolaProjectile>();
            parabolaProjectile.Init(this,1.5f * ProjectileSpeedMultiplier, 50f, instanceStats.FinalAttackDamage, AttackTarget.transform);
            
            //블랙애로우 판정
            bool isBlackArrow = ManagerRoot.Skill.PlayerSkill
                .Any(x => x.GetType() == typeof(BlackArrow));
            if (isBlackArrow && _blackArrowShotDelay <= 0 && CurrentFaction == Faction.Undead) 
            {
                if (_blackArrowCoroutine != null) StopCoroutine(_blackArrowCoroutine);
                _blackArrowCoroutine = StartCoroutine(BlackArrowCoroutine());
                
                if (BlackArrowDamageMultiplier == 3) parabolaProjectile.transform.localScale *= 1.5f;
                else if (BlackArrowDamageMultiplier == 5) parabolaProjectile.transform.localScale *= 2f;

                parabolaProjectile.GetComponent<SpriteRenderer>().color = Color.black;
                
                atkInfo.damage *= BlackArrowDamageMultiplier;
                parabolaProjectile.MultiplyDamage(BlackArrowDamageMultiplier);
            }
            
            //헤비어택 판정 (슬로우)
            if (HeavyAttackSlowPercent > 0)
            {
                parabolaProjectile.SlowModifier(HeavyAttackSlowPercent, HeavyAttackSlowDuration);
            }
            parabolaProjectile.FireProjectile(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition));
            ManagerRoot.Sound.PlaySfx(_attackAudioList[UnityEngine.Random.Range(0, _attackAudioList.Count)], 0.4f);
            
        }
    }
    
    
    IEnumerator BlackArrowCoroutine() //MultiShot에도 BlackArrow가 적용되게끔 하기 위해 코루틴으로 구현
    {
        yield return new WaitForSeconds(0.1f);
        _blackArrowShotDelay = BlackArrowShotDelay;
    }

    public void SetSecondAttackTarget()
    {
        Unit target = null;
        List<Unit> units = ManagerRoot.Unit.GetAllAliveHumanUnits();
        units.MMShuffle();
        //check attackable unit
        foreach (Unit unit in units)
        {
            if (unit.gameObject == AttackTarget) continue;
            if (unit.instanceStats.FinalAttackRange + Statics.MultishotAdditionalRange < Vector2.Distance(transform.position, unit.transform.position)) continue;
            target = unit;
            break;
        }

        if (target == null)
        {
            secondAttackTarget = null;
            return;
        }
        secondAttackTarget = target.gameObject;
    }
    
    public void SetThirdAttackTarget()
    {
        Unit target = null;
        List<Unit> units = ManagerRoot.Unit.GetAllAliveHumanUnits();
        units.MMShuffle();
        //check attackable unit
        foreach (Unit unit in units)
        {
            if (unit.gameObject == AttackTarget || unit.gameObject == secondAttackTarget) continue;
            if (unit.instanceStats.FinalAttackRange + Statics.MultishotAdditionalRange < Vector2.Distance(transform.position, unit.transform.position)) continue;
            target = unit;
            break;
        }

        if (target == null)
        {
            thirdAttackTarget = null;
            return;
        }
        thirdAttackTarget = target.gameObject;
    }
    
    
    public void FireExtraProjectile(GameObject target_)
    {
        GameObject projectileGO = ManagerRoot.Resource.Instantiate(projectile);
        projectileGO.transform.position = transform.position;
        ParabolaProjectile parabolaProjectile = projectileGO.GetComponent<ParabolaProjectile>();
        parabolaProjectile.Init(this,1.5f * ProjectileSpeedMultiplier, 50f, instanceStats.FinalAttackDamage, target_.transform);
        bool isBlackArrow = ManagerRoot.Skill.PlayerSkill
            .Any(x => x.GetType() == typeof(BlackArrow));
        if (isBlackArrow && _blackArrowShotDelay <= 0)
        {
            if (BlackArrowDamageMultiplier == 3) parabolaProjectile.transform.localScale *= 1.5f;
            else if (BlackArrowDamageMultiplier == 5) parabolaProjectile.transform.localScale *= 2f;
            parabolaProjectile.GetComponent<SpriteRenderer>().color = Color.black;
            parabolaProjectile.MultiplyDamage(BlackArrowDamageMultiplier);
        }
        if (HeavyAttackSlowPercent > 0)
        {
            parabolaProjectile.SlowModifier(HeavyAttackSlowPercent, HeavyAttackSlowDuration);
        }
        parabolaProjectile.FireProjectile(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition));
        ManagerRoot.Sound.PlaySfx(_attackAudioList[UnityEngine.Random.Range(0, _attackAudioList.Count)], 0.2f);
    }
}
