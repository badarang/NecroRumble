using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class SoulDash : SkillBase
{
    private SoulDashData _data;
    private List<Unit> _damagedEnemies = new();
    private Coroutine _lv1SoulDashCoroutine;
    private Player _player;
    private Recall _recall;
    
    //Lv2
    private List<Unit> _lv2FastAssassins = new();
    private const string LV2_SPEED_MODIFIER_NAME = "SoulDashLv2_MoveSpeed";

    private void Init()
    {
        _data = LoadData<SoulDashData>();
        _player = (GameManager.Instance == null) ? FindObjectOfType<Player>() : GameManager.Instance.GetPlayer();
        _recall = _player.GetComponentInChildren<Recall>();
        Id = _data.Id;
        Name = _data.Name;
    }
    
    public override void OnSkillAttained()
    {
        Init();
        
        ManagerRoot.Event.onRecallStarted += StartLv1SoulDash;
        ManagerRoot.Event.onRecallLanded += EndLv1SoulDash;
    }


    public override void OnSkillUpgrade()
    {
        if (SkillLevel == 2)
        {
            ManagerRoot.Event.onRecallLanded += Lv2MakeAssassinFast;
            ManagerRoot.Event.onIDamageableTakeHit += Lv2AssassinLoseFast;

            //복귀 쿨타임 감소
            _recall.cooldownAddition += -1 * Mathf.Abs(_data.Lv2RecallCooldownReduceSecond);
        }
    }

    private void Lv2AssassinLoseFast(IDamageable damaged_, AttackInfo attackinfo_, int modifieddamage_, ref int finaldamage_)
    {
        //공격자가 빨라진 어쌔신이면 속도를 원래대로 돌린다.
        if (attackinfo_.attacker is Unit attackerUnit && _lv2FastAssassins.Contains(attackerUnit))
        {
            ManagerRoot.UnitUpgrade.TweakSingleUnitModValueByName(attackerUnit, LV2_SPEED_MODIFIER_NAME, 0);
            _lv2FastAssassins.Remove(attackerUnit);
        }
    }

    private void Lv2MakeAssassinFast()
    {
        //모든 어쌔신 유닛들 가져오기.
        var allAssassins = ManagerRoot.Unit.GetAllAliveUndeadUnits().FindAll(unit => unit.UnitType == (int) UnitType.Assassin);
        
        //어쌔신들의 이동속도를 빠르게 한다.
        StatModifier fastMod = new StatModifier(StatType.MoveSpeed, LV2_SPEED_MODIFIER_NAME, _data.Lv2MoveSpeedIncreasePercent, StatModifierType.Percentage, false);
        foreach (var assassin in allAssassins)
        {
            if (assassin.instanceStats.GetModifierByName(LV2_SPEED_MODIFIER_NAME) == null)
            {
                ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(assassin, fastMod);
            }
            else
            {
                ManagerRoot.UnitUpgrade.TweakSingleUnitModValueByName(assassin, LV2_SPEED_MODIFIER_NAME, _data.Lv2MoveSpeedIncreasePercent);
            }
            
            if (_lv2FastAssassins.Contains(assassin) == false) _lv2FastAssassins.Add(assassin);
        }
    }

    public override void OnBattleStart(){}
    public override void OnBattleEnd(){}

    private void StartLv1SoulDash()
    {
        //모든 어쌔신 언데드 유닛들 가져오기.
        var allAssassins = ManagerRoot.Unit.GetAllAliveUndeadUnits().FindAll(unit => unit.UnitType == (int) UnitType.Assassin);
        
        //대시 반복 코루틴 시작.
        if (_lv1SoulDashCoroutine != null) StopCoroutine(_lv1SoulDashCoroutine);
        _lv1SoulDashCoroutine = StartCoroutine(Lv1SoulDashCoroutine(allAssassins));
        
        //TODO: 이펙트 시작
    }
    
    private void EndLv1SoulDash()
    {
        if (_lv1SoulDashCoroutine != null) StopCoroutine(_lv1SoulDashCoroutine);
        _damagedEnemies.Clear();
        
        //TODO: 이펙트 끝
    }
    
    private IEnumerator Lv1SoulDashCoroutine(List<Unit> allAssassins_)
    {
        while (true)
        {
            foreach (var assassin in allAssassins_)
            {
                //어쌔신마다 주변 원범위 내에 있는 적들을 가져온다.
                var pos = assassin.transform.position;
                int humanLayerMask = Layers.HumanUnit.ToMask();
                var collidedHumans = Physics2D.OverlapCircleAll(pos, _data.Lv1FearRadius, humanLayerMask)
                    .Select(x => x.GetComponent<Unit>())
                    .Where(x => x != null)
                    .ToList();
                
                //인간 유닛들에게 대미지와 공포
                foreach (var human in collidedHumans)
                {
                    //이미 공격한 적이면 무시.
                    if (_damagedEnemies.Contains(human)) continue;
                    _damagedEnemies.Add(human);
                    
                    //대미지
                    var atkInfo = new AttackInfo(assassin, assassin.instanceStats.FinalAttackDamage);
                    human.TakeDamage(atkInfo);
                    
                    //공포
                    var fearAtkInfo = new AttackInfo(assassin, 0);
                    human.TakeFear(1f, _player.transform.position, _data.Lv1FearDuration, fearAtkInfo);
                }
            }
            yield return null;
        }
    }
}
