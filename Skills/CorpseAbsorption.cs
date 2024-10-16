using UnityEngine;
using System.Collections;
using LOONACIA.Unity.Managers;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class CorpseAbsorption : SkillBase
{
    Player _player;
    CorpseAbsorptionData _data;
    InputManager _input;

    [ReadOnly, SerializeField]
    bool _isPerfoming = false;
    [ShowInInspector]Queue<Unit> _waitQToAbsorb = new Queue<Unit>();

    void Start()
    {
        _data = LoadData<CorpseAbsorptionData>();
        _player = GameManager.Instance.GetPlayer();
        _input = ManagerRoot.Input;
        Id   = _data.Id;
        Name = _data.Name;

        StartCoroutine( DeQ() );
    }

    void Update()
    {
        // if      ( _input.CorpseAbsorptionButton){ _isPerfoming = true;  }
        // else if (!_input.CorpseAbsorptionButton){ _isPerfoming = false; }


        if(_isPerfoming)
        {
            Unit underfoot = _player.GetLastCorpseUnderfoot();
            if(underfoot == null){ return; }
            if(_player.CanAction == false){ return; }

            if( !_waitQToAbsorb.Contains(underfoot)){
                _waitQToAbsorb.Enqueue(underfoot);
                underfoot.GetComponent<Rigidbody2D>().simulated = false;
                _player.CostCoprseUnderfoot(underfoot);
            }
        }
    }

// <<<<<<< HEAD
//     IEnumerator KillByCorpseAbsorption(Unit underfoot_)
//     {
//         if (underfoot_.TryGetComponent(out FeedbackController feedback)){ 
//             feedback.ChangeMaterialBasedOnIsSelected(true);
//             yield return new WaitForSeconds(0.1f);
//             feedback.ChangeMaterialGhostGlitch(true);
//         }
//         yield return new WaitForSeconds(_data.dieDelay);
//         ManagerRoot.Unit.DestroyCorpse(underfoot_);
//         _player.ReplenishMp( _data.replenishMp);
// =======


    IEnumerator DeQ()
    {
        while(true)
        {
            while( _waitQToAbsorb.Count > 0)
            {
                Unit underfoot_ = _waitQToAbsorb.Dequeue();

                StartCoroutine(StartEffectAndAbsorb(underfoot_));

                yield return new WaitForSeconds(_data.nextAbsorbDelay); // 0.1초
            }

            yield return new WaitForSeconds(0.2f); // 과부하 방지
        }
    }

    IEnumerator StartEffectAndAbsorb(Unit underfoot_)
    {
        if (underfoot_.TryGetComponent(out FeedbackController feedback)){ 
            feedback.ChangeMaterialBasedOnIsSelected(true);
            yield return new WaitForSeconds(0.1f);
            feedback.ChangeMaterialGhostGlitch(true);
        } 

        yield return new WaitForSeconds(_data.absorbEffectDelay); // 2초

        Debug.Log($"Corpse Absorb : {underfoot_.name}");

        ManagerRoot.Unit.DestroyCorpse(underfoot_);
        // _player.ReplenishMp( _data.replenishMp);
    }


    public override void OnBattleStart() {}
    public override void OnBattleEnd() {}
    public override void OnSkillUpgrade(){}
    public override void OnSkillAttained()
    {
    }
}
