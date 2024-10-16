using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class MoveSpeedUp : SkillBase
{
    MoveSpeedUpData _data;
    Player _player => GameManager.Instance.GetPlayer();
    PlayerMovement _playerMove => _player.GetComponent<PlayerMovement>();

    private Coroutine _loopCheckRoutine;
    void Start()
    {

    }

    IEnumerator CreateTrailLoop()
    {
        while (true)
        {
            if (_playerMove.IsWalking())
            {
                var trail = ManagerRoot.Instantiate(_data.trailPrefab, _player.transform.position, Quaternion.identity);
                trail.name = "PlayerDamageTrail";
                trail.GetComponent<TrailController>().Init(_data.trailDamage);
                Destroy(trail, 3f);
            }
            yield return new WaitForSeconds(_data.trailCreateDelay);
        }
    }
    
    void Excute()
    {
        Debug.Log("MoveSpeedUp Excuted");

        _data = LoadData<MoveSpeedUpData>();
        Id    = _data.Id;
        Name  = _data.name;

        //이속 증가 없앰
        // _playerMove.IncreaseMaxSpeed(_data.increaseMaxSpeed);
        // _playerMove.IncreaseMaxAccel(_data.increaseMaxAcceleration);
        // _playerMove.IncreaseMaxDeccel(_data.increaseMaxSpeed);
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
        Excute();
        //반복 체크 루틴 시작
        if (_loopCheckRoutine != null) StopCoroutine(_loopCheckRoutine);
        _loopCheckRoutine = StartCoroutine(CreateTrailLoop());
    }
    
    public void OnDisable()
    {
    }
}
