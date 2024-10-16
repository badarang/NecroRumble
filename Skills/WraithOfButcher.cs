using System;
using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public class WraithOfButcher : SkillBase
{
    private WraithOfButcherData _data;
    private float _maxCooldown;
    private float _curretnCooldown;
    private bool _isDuringBattle;

    public WraithOfButcherData Data => _data;

    private void Awake()
    {
        _data = LoadData<WraithOfButcherData>();
        Id   = _data.Id;
        Name = _data.Name;

        #if UNITY_EDITOR
        //DEBUG
        _isDuringBattle = true;
        #endif
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        TrySpawnButcher();
    }

    public override void OnBattleStart()
    {
        //쿨타임 카운트다운 시작
        _isDuringBattle = true;
    }

    public override void OnBattleEnd()
    {
        //쿨타임 초기화 및 카운트다운 정지
        _isDuringBattle = false;
        _curretnCooldown = _maxCooldown;
    }

    public override void OnSkillUpgrade()
    {
        switch (SkillLevel)
        {
            case 2:
                //TODO: 스킬 레벨 2 효과 구현
                break;
            case 3:
                //TODO: 스킬 레벨 3 효과 구현
                break;
            case 4:
                //TODO: 스킬 레벨 4 효과 구현
                break;
            case 5:
                //TODO: 스킬 레벨 5 효과 구현
                break;
        }
        Init();
    }

    private void Init()
    {
        _maxCooldown = _data.GetCurSkillData(SkillLevel).Cooldown;
        _curretnCooldown = _maxCooldown;
    }
    
    private void CreateButcherUnit()
    {
        //생성 위치 산정
        int angle = Random.Range(0, 360);
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        float randomness = Random.Range(-_data.SpawnDistanceRandomness, _data.SpawnDistanceRandomness);
        float distance = _data.SpawnDistance + randomness;
        Vector2 spawnPosition = transform.position + rotation * Vector2.right;
        
        ManagerRoot.Unit.CreateUnit(UnitType.ButcherZombie, spawnPosition, Faction.Undead, undeadForm_: UndeadForm.Zombie);
    }

    private void TrySpawnButcher()
    {
        if (!_isDuringBattle) return;
        
        //Tick Cooldown
        if (_curretnCooldown > 0) _curretnCooldown -= Time.deltaTime;
        
        if (_curretnCooldown > 0) return;
        
        //Spawn Butcher
        for (int i = 0; i < _data.SpawnCount; i++)
        {
            CreateButcherUnit();
        }
        
        //Reset Cooldown
        _curretnCooldown = _maxCooldown;
    }

    public override void OnSkillAttained()
    {
    }
}
