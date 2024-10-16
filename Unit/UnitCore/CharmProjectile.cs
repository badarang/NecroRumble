using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharmProjectile : ParabolaProjectile
{
    public override void TryTakeDamage(){
        if (_target.TryGetComponent(out IDamageable damageable))
        {
            AttackInfo attackInfo = new (_owner, _damage, attackingMedium: transform);
            damageable.TakeCharm(_damage, attackInfo);
        }
    }
    
    
}