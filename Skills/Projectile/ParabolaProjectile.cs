using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParabolaProjectile : Projectile
{
    [SerializeField] private float _projectileHeight = 4f;
    private float _fireLerp = 1;
    
    protected override void Update()
    {
        if (Vector3.Distance(_startPos, transform.position) > _destroyRange)
        {
            Destroy(gameObject);
        }
        if (_fireLerp < 1)
        {
            Vector3 newProjectilePos = CalculateTrajectory(StartPos, EndPos, _fireLerp);
            
            Vector3 direction = (newProjectilePos - transform.position).normalized;
            SetRotation(direction);
            
            transform.position = newProjectilePos;
            _fireLerp += Speed * Time.deltaTime;
        }
        else //포물선 끝났을 때 타겟에게 데미지 주기
        {
            if (Target != null)
            {
                TryTakeDamage();
            }
            Destroy(gameObject);
        }
        
        if (Target != null)
        {
            EndPos = Target.position;
        }
    }
    
    public void FireProjectile(Vector3 firePoint, Vector3 targetPos)
    {
        StartPos = firePoint;
        EndPos = targetPos;
        _fireLerp = 0;
        
        Vector3 direction = (EndPos - StartPos).normalized;
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        if (direction.x > 0) transform.rotation *= Quaternion.Euler(0, 180, 0);
    }
    
    Vector3 CalculateTrajectory(Vector3 firePos, Vector3 targetPos, float t)
    {
        Vector3 linearProgress = Vector3.Lerp(firePos, targetPos, t);
        float perspectiveOffset = Mathf.Sin(t * Mathf.PI) * _projectileHeight;

        Vector3 trajectoryPos = linearProgress + (Vector3.up * perspectiveOffset);
        trajectoryPos.z = 0;
        return trajectoryPos;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // collision 계산 없음
    }
    
}