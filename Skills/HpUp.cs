using System;
using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class HpUp : SkillBase
{
    HpUpData _data;
    Player _player => GameManager.Instance.GetPlayer();

    void Start()
    {

    }

    private void ThornShield(IDamageable damaged_, AttackInfo attackInfo_, int modifiedDamage_, ref int finalDamage_)
    {
        const float DAMAGE_REFLECT_RATE = 5f;
        //피해 입은 유닛이 플레이어라면 공격자에게 피해를 입힌다.
        if (damaged_ is Player)
        {
            if (attackInfo_.attacker as Unit is not { CurrentFaction: Faction.Human }) return;
            
            var atk = new AttackInfo((IAttacker)damaged_, modifiedDamage_ * DAMAGE_REFLECT_RATE);
            (attackInfo_.attacker as IDamageable)?.TakeDamage(atk);
        }
    }
    
    void Excute()
    {
        Debug.Log("HpUp Excuted");

        _data = LoadData<HpUpData>();
        Id    = _data.Id;
        Name  = _data.name;

        _player.IncreaseMaxHp(_data.increaseMaxHp);
    }

    public override void OnBattleStart()
    {

    }
    public override void OnBattleEnd()
    {
    }
    public override void OnSkillUpgrade()
    {

    }

    public override void OnSkillAttained()
    {
        if( ManagerRoot.Event == null ){ return; }

        ManagerRoot.Event.onIDamageableTakeHit += ThornShield;
        Excute();
    }

    public void OnDisable()
    {
        if( ManagerRoot.Instance == null ){ return; }
        ManagerRoot.Event.onIDamageableTakeHit -= ThornShield;
    }
}
