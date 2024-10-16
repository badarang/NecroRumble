using System;
using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class NecroResurrect : SkillBase
{
    private NecroResurrectData _data;
    private Player _player;
    private Animator _anim;
    private GameObject _damageWave;
    private GameObject _damageEffect;
    private bool _canResurrect = true;
    private bool _isResurrecting;
    
    public bool CanResurrect
    {
        get => _canResurrect;
        private set => _canResurrect = value;
    }
    
    public bool IsResurrecting
    {
        get => _isResurrecting;
        private set => _isResurrecting = value;
    }
    
    public override void OnSkillAttained(){}
    public override void OnSkillUpgrade(){}
    public override void OnBattleStart(){}
    public override void OnBattleEnd(){}

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _data = ManagerRoot.Resource.Load<NecroResurrectData>("Data/Skill/NecroResurrectData");
        _player = FindObjectOfType<Player>();
        _anim = _player.GetComponentInChildren<Animator>();
        Id = _data.Id;
        Name = _data.Name;
        _damageWave = ManagerRoot.Resource.Load<GameObject>("Prefabs/Skills/ResurrectDamageWave");
        _damageEffect = ManagerRoot.Resource.Load<GameObject>("Prefabs/Effects/CrawlingEffect");
    }
    
    public void StartResurrect()
    {
        StartCoroutine(PlayResurrectSequence());
    }
    
    private IEnumerator PlayResurrectSequence()
    {
        if (!CanResurrect) yield break;
        
        CanResurrect = false;
        IsResurrecting = true;
        _player.CanAction = false;
        
        //죽음 애니메이션 재생
        _anim.SetFloat("DieAnimSpd", 1);
        ManagerRoot.Sound.PlaySfx("Debuff Downgrade 21 Larger", 1f);
        //_anim.SetTrigger("Die");
        _anim.Play("NecromancerDie");
        //죽음 시간만큼 대기
        yield return new WaitForSeconds(_data.DeathDuration);
        
        //부활 애니메이션 재생
        _anim.SetFloat("DieAnimSpd", -1);
        _anim.Play("NecromancerDie", -1, 1f);
        //_anim.SetTrigger("Die");
        ManagerRoot.Sound.PlaySfx("Debuff Downgrade 22 Larger", 1f);
        ManagerRoot.Sound.PlaySfx("Debuff Downgrade 21", 1f);
        
        //체력 회복
        _player.TakeHeal(_player.MaxHp * _data.ResurrectHpRatio);
        
        //대미지 웨이브 생성
        StartCoroutine(DamageWaveCoroutine());
        ManagerRoot.Sound.PlaySfx("Explosion Raw 2", 1f);

        //부활 애니메이션 시간만큼 대기 (일단 하드코딩.)
        yield return new WaitForSeconds(_data.ResurrectAnimationDuration);
        
        //이동, 공격등 가능.
        _player.CanAction = true;
        _anim.Play("NecromancerIdle");
        _anim.SetFloat("DieAnimSpd", 1);
        
        //무적 시간
        yield return new WaitForSeconds(_data.InvincibleDuration);
        IsResurrecting = false;
    }

    private IEnumerator DamageWaveCoroutine()
    {
        if (_damageWave == null) yield break;
        
        //TODO: 이 코루틴에서 전체적인 이펙트 관리. 워낙 복잡할 것 같아서 정확한 위치를 특정하긴 어려움.
        var damageEffect = Instantiate(_damageEffect, _player.transform.position, Quaternion.identity);
        damageEffect.transform.GetChild(1).GetComponent<ShockWaveController>().CallShockWave();
        
        //대미지 웨이브 오브젝트 생성
        var damageWave = Instantiate(_damageWave, _player.transform.position, Quaternion.identity);
        damageWave.transform.localScale = Vector3.zero;
        damageWave.GetComponent<ResurrectDamageWave>().Init(_player, _data.Damage);

        while (damageWave.transform.localScale.x < _data.DamageWaveRadius)
        {
            //대미지 웨이브 크기 키우기
            var localScale = damageWave.transform.localScale;
            var speed = Mathf.Lerp(_data.MinWaveSpeed, _data.MaxWaveSpeed, localScale.x / _data.DamageWaveRadius);
            localScale += Vector3.one * speed * Time.deltaTime;
            damageWave.transform.localScale = localScale;
            yield return null;
        }
        
        //충분히 커지면 삭제
        damageWave.SetActive(false);
        Destroy(damageWave);
    }
}
