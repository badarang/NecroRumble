using System;
using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class CrossBowManUnit : Unit
{
    
    GameObject projectile;
    public override void Start()
    {
        base.Start();
        projectile = Resources.Load<GameObject>("Prefabs/Unit/Weapons/TestArrow");
        
        _isRangedDealer = true;
    }
    public override void AnimEvent_HitMoment()
    {
        if( AttackTarget != null){            
            GameObject projectileGO = ManagerRoot.Resource.Instantiate(projectile);
            projectileGO.transform.position = transform.position;
            ParabolaProjectile parabolaProjectile = projectileGO.GetComponent<ParabolaProjectile>();
            parabolaProjectile.Init(this, 1.5f, 50f, instanceStats.FinalAttackDamage, AttackTarget.transform);
            parabolaProjectile.FireProjectile(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }
    
}
