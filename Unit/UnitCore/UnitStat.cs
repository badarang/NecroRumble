using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStat
{
    public float hp;
    public float damage;
    public float moveSpeed;
    public float attackRange;
    
    public UnitStat(float hp_, float damage_, float moveSpeed_, float attackRange_)
    {
        hp = hp_;
        damage = damage_;
        moveSpeed = moveSpeed_;
        attackRange = attackRange_;
    }
}
