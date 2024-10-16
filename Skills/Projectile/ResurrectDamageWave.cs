using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResurrectDamageWave : MonoBehaviour
{
    private Player _player;
    private int _damage;
    
    public void Init(Player player_, int damage_)
    {
        _player = player_;
        _damage = damage_;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Unit>() is {} unit)
        {
            if (unit.CurrentFaction == Faction.Human)
            {
                var atkInfo = new AttackInfo(_player, _damage, 10f, transform);
                unit.TakeDamage(atkInfo);
            }
        }
    }
}
