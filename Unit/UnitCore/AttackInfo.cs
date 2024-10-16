using UnityEngine;


public class AttackInfo
{
    public IAttacker attacker;
    public float damage;
    public float knockBackPower;
    /// <summary>
    /// 공격의 매개체. 공격자가 궁수라면, 매개체는 화살에 해당함.
    /// </summary>
    public Transform attackingMedium;

    public AttackInfo(IAttacker attacker, float damage, float knockBackPower = 0, Transform attackingMedium = null)
    {
        this.attacker = attacker;
        this.damage = damage;
        this.knockBackPower = knockBackPower;
        this.attackingMedium = attackingMedium;
    }
}