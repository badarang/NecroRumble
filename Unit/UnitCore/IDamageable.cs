using UnityEngine;

public interface IDamageable
{
    void TakeHeal(float healAmount, Transform healer, bool isHealEffect = false);
    void TakeDamage(AttackInfo attackInfo_, Color32 damageColor_ = default, Unit.SPECIALMODE specialMode_ = Unit.SPECIALMODE.NONE);
    void TakeCharm(float charmDuration_, AttackInfo attackInfo_);
    public void TakeKnockBack(Transform attackerTrans_, float power_ = 20f);
    public void TakeKnockDown(Transform attackerTrans_, float power_ = 20f, float duration_ = 2f);
    public void TakeKnockDownJump(Transform attackerTrans_, float power_ = 20f, float duration_ = 2f);
    public void TakeSlow(float slowPercent_, float slowDuration_);
}
