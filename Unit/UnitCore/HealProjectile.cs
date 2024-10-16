using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealProjectile : ParabolaProjectile
{
    public override void TryTakeDamage(){
        if (_target.TryGetComponent(out IDamageable damageable))
        {
            AttackInfo attackInfo = new (_owner, _damage, attackingMedium: transform);
            damageable.TakeHeal(_damage, attackInfo.attacker.GameObject.transform);
        }
    }
    
    
}