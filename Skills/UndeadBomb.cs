using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class UndeadBomb : SkillBase
{
    UndeadBombData _data;

    private LayerMask _maskDamaged = Layers.HumanUnit.ToMask(); // 적 유닛에게 데미지

    void Start()
    {
        _data = LoadData<UndeadBombData>();
        Id    = _data.Id;
        Name  = _data.name;
    }

    private void DeathBomb(Unit deadUnit_, AttackInfo attackInfo_)
    {
        if (attackInfo_ == null) return;
        if (deadUnit_   == null) return;
        if (deadUnit_.CurrentFaction == Faction.Human){ return; } // 아군 유닛이 죽었을 때만 실행

        Collider2D[] enemyUnits = GetCorpseListEllipse(deadUnit_.transform.position, _data._range, _data._range * .5f);
        
        GameObject explosionEffect = Instantiate(_data._effectPrefab, deadUnit_.transform.position, Quaternion.identity);
        Destroy(explosionEffect, 1f);
        
        if (explosionEffect.FindChild("RangeCircle").TryGetComponent(out ExplosionController explosionController))
        {
            explosionController.Explode(_data._range);
        }
        
        foreach(var enemyUnit in enemyUnits)
        {
            if (enemyUnit.TryGetComponent(out Unit unit))
            {
                unit.TakeDamage(new AttackInfo(attackInfo_.attacker, _data._damage, _data._knockBackPower));
            }
        }

    }


    Collider2D[] GetCorpseListEllipse(Vector3 centerPoint, float _width, float _height)
    {
        var humanUnits = Physics2D.OverlapAreaAll(
            new Vector2(centerPoint.x - _width, centerPoint.y - _height),
            new Vector2(centerPoint.x + _width, centerPoint.y + _height),
            _maskDamaged
        );
        return humanUnits;
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
        ManagerRoot.Event.onUnitDeath += DeathBomb;
    }
    
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onUnitDeath -= DeathBomb;
    }
}
