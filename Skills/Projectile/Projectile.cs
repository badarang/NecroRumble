using System;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected IAttacker _owner;
    [HideInInspector] [SerializeField] protected float _speed = 10f;
    [HideInInspector] [SerializeField]protected float _destroyRange = 10f;
    [HideInInspector] [SerializeField]protected float _damage;
    [HideInInspector] [SerializeField]protected Vector3 _startPos;
    [HideInInspector] [SerializeField]protected Vector3 _endPos;
    [HideInInspector] [SerializeField]protected Transform _target;
    
    public void Init(IAttacker owner_, float speed_, float destroyRange_, float damage_, Transform target_ = null)
    {
        _startPos = transform.position;
        _owner = owner_;
        _speed = speed_;
        _destroyRange = destroyRange_;
        _damage = damage_;
        _target = target_;
    }
    
    #region GETSET

    public Vector3 StartPos
    {
        get => _startPos;
        set => _startPos = value;
    }
    
    public Vector3 EndPos
    {
        get => _endPos;
        set => _endPos = value;
    }

    public float Speed
    {
        get => _speed;
        set => _speed = value;
    }
    
    public Transform Target
    {
        get => _target;
        set => _target = value;
    }
    
    public float SlowPercent { get; set; }
    public float SlowDuration { get; set; }
    
    #endregion
    
    protected virtual void Update()
    {
        if (Vector3.Distance(_startPos, transform.position) > _destroyRange)
        {
            Destroy(gameObject);
        }
        
        if (_target != null)
        {
            if (Vector3.Distance(_target.position, transform.position) < .3f)
            {
                TryTakeDamage();
                Destroy(gameObject);
            }
        }
    }
    
    public virtual void TryTakeDamage()
    {
        if (_target.TryGetComponent(out IDamageable damageable))
        {
            AttackInfo attackInfo = new (_owner, _damage, attackingMedium: transform);
            damageable.TakeDamage(attackInfo);
            if (SlowPercent > 0)
            {
                damageable.TakeSlow(SlowPercent, SlowDuration);
            }
        }
    }
    public virtual void TryTakeDamage(Transform target_)
    {
        if (target_.TryGetComponent(out IDamageable damageable))
        {
            //TODO: attackingMedium 이 Transform 인 게 마음에 안듬
            AttackInfo attackInfo = new (_owner, _damage, attackingMedium: transform);
            damageable.TakeDamage(attackInfo);
            if (SlowPercent > 0)
            {
                damageable.TakeSlow(SlowPercent, SlowDuration);
            }
        }
    }
    
    public virtual void SetDamage(float damage)
    {
        _damage = damage;
    }
    
    public virtual void MultiplyDamage(float modifier)
    {
        _damage *= modifier;
    }

    public virtual void SlowModifier(float percent, float duration)
    {
        SlowPercent = percent;
        SlowDuration = duration;
    }

    public virtual void SetDirection(Vector2 direction)
    {
        // 투사체에 방향과 속도를 설정하고 이동시키는 코드
        if (TryGetComponent(out Rigidbody2D rb))
        {
            rb.velocity = direction.normalized * _speed;
        }
    }

    public virtual void SetRotation(Vector2 direction, bool isFLipX = false, bool isFLipY = false)
    {
        // 계산된 각도
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // float rotationSpeed = 30f;

        // 방향에 따른 회전 설정
        if (direction.x < 0)
        {
            // 왼쪽을 보도록 회전
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

            // 스케일 설정
            Vector3 scale = transform.localScale;
            scale.x = isFLipX ? -1 * Math.Abs(scale.x) : 1 * Math.Abs(scale.x);
            scale.y = isFLipY ? -1 * Math.Abs(scale.x) : 1 * Math.Abs(scale.x);
            transform.localScale = scale;

            transform.rotation = targetRotation;
            // // 부드러운 회전 보간
            // transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        else
        {
            // 오른쪽을 보도록 회전
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle) * Quaternion.Euler(0, 180, 0);

            // 스케일 설정
            Vector3 scale = transform.localScale;
            scale.x = isFLipX ? 1 * Math.Abs(scale.x) : -1 * Math.Abs(scale.x);
            scale.y = isFLipY ? 1 * Math.Abs(scale.x) : -1 * Math.Abs(scale.x);
            transform.localScale = scale;

            transform.rotation = targetRotation;
            // // 부드러운 회전 보간
            // transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }



    protected abstract void OnTriggerEnter2D(Collider2D other);
}