using LOONACIA.Unity.Managers;
using UnityEngine;

public class BloodFang : SkillBase
{
    BloodFangData _data;

    void Start()
    {
        _data = LoadData<BloodFangData>();
        Id    = _data.Id;
        Name  = _data.name;
        
        //StatModifier mod = new StatModifier(StatType.AttackDamage, 1)
    }

    private void OnUnitDeath(Unit deadUnit_, AttackInfo attackInfo_)
    {
        if (deadUnit_   == null) return;
        if (attackInfo_ == null) return;
        if (attackInfo_.attacker == null) return;
        
        if (deadUnit_.gameObject.layer            == Layers.UndeadUnit) return; // 적군 유닛이 죽었을 때만 실행
        if (attackInfo_.attacker.GameObject.layer != Layers.UndeadUnit ) return; // 공격자는 아군이여야 함
        
        if (attackInfo_.attacker.GameObject.TryGetComponent(out Unit undeadUnit))
        {
            GameObject BloodAbsorptionEffect = Instantiate(_data._effectPrefab, deadUnit_.transform.position, Quaternion.identity);
            if (BloodAbsorptionEffect.TryGetComponent(out BloodAbsorptionController bloodAbsorptionController))
            {
                bloodAbsorptionController.target = undeadUnit;
                bloodAbsorptionController.JumpToRandomHeight();
            }
            
            if( undeadUnit.instanceStats == null ){ Debug.LogWarning("instanceStats == null "); return;}
            var recoveryRatio = undeadUnit.IsElite ? _data.eliteHpRecoveryRate : _data.hpRecoveryRate;
            float healAmount = undeadUnit.instanceStats.FinalMaxHp * recoveryRatio;
            
            if (_data.useFixedHealAmount) healAmount = _data.fixedHealAmount; //고정치 방식
            undeadUnit.TakeHeal(healAmount);
        }
    }



    public override void OnBattleStart()
    {
        
    }
    public override void OnBattleEnd()
    {
        
    }
    public override void OnSkillUpgrade()
    {
        if (SkillLevel == 1)
        {
            //이것저것 해
        }
        else if (SkillLevel == 2)
        {
            //두번째 업그레이드때 뭐 할지
        }
    }
    public override void OnSkillAttained()
    {
        ManagerRoot.Event.onUnitDeath += OnUnitDeath;
    }
    public void OnDisable()
    {
        if( ManagerRoot.IsGameQuit ){ return; }
        ManagerRoot.Event.onUnitDeath -= OnUnitDeath;
    }
}
