using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tactical.Tasks;
using LOONACIA.Unity.Managers;
using Unity.VisualScripting;
using UnityEngine;

public class FireProjectile : MonoBehaviour
{

    public float speed;
    public float accel;
    public float bezierDelta = 1f; // 베지언 가운데점까지의 거리
    // private float retargetRadius = 5f; // 대상 죽었을때 리타겟 범위

    Rigidbody2D rigid;

    Vector2 startPos;
    Vector2 delta;

    Transform target;
    Vector2 endPos;

    float timer = 0f;

    // bool initialized = false;

    LayerMask mask;
    AttackInfo attackInfo; 

    public void Init(Vector2 startPos_, Transform target_, LayerMask targetLayer_, AttackInfo attackInfo_)
    {
        rigid = GetComponent<Rigidbody2D>();
        startPos = startPos_;
        mask = targetLayer_;
        attackInfo = attackInfo_;
        target = target_;
        delta = SetDelta();
        endPos = target.transform.position;
        // if (target_ == null)
        // {
        //     FindNewTarget();
        // }
        // else
        // {
        //     target = target_;
        //     delta = SetDelta();
        // }

        // initialized = true;
    }

    private void FixedUpdate()
    {
        CalculateDelta();
        Move();
    }

    public void CalculateDelta()
    {
        speed += accel * Time.fixedDeltaTime;
    }

    private Vector2 SetDelta()
    {
        float x, y;
        Vector2 targetDirection = (Vector2)target.position - startPos;
        float targetAngle = targetDirection.x < 0 ? 180f : 0f;
        x = Mathf.Cos(targetAngle + Random.Range(-60f, 60f) * Mathf.Deg2Rad) * bezierDelta + startPos.x;
        y = Mathf.Sin(targetAngle + Random.Range(-60f, 60f) * Mathf.Deg2Rad) * bezierDelta + startPos.y;
        return new Vector2(x, y);
    }

    // private void FindNewTarget()
    // {
    //     // LayerMask mask = attacker.damageLayer;
    //     Collider2D[] t = Physics2D.OverlapCircleAll(transform.position, retargetRadius, mask);
    //     if (t.Length > 0)
    //     {
    //         Collider2D c = t[Random.Range(0, t.Length)];
    //         target = c.gameObject.transform;
    //         delta = SetDelta();
    //     }
    //     else
    //     {
    //         Destroy(gameObject);
    //     }
    // }

    public void Move()
    {
        // if (target == null || !target.gameObject.activeSelf)
        // {
        //     FindNewTarget();
        // }
        // if (!initialized) return;

        Vector2 prevPosition = rigid.position;
        if(target != null){
            endPos = target.transform.position;
        }
        rigid.position = new Vector2(
            Bezier(timer, startPos.x, delta.x, endPos.x),
            Bezier(timer, startPos.y, delta.y, endPos.y));

        SetRotation((rigid.position- prevPosition).normalized);
        timer += Time.fixedDeltaTime * speed;

        if (timer >= 2f)
        {
            Destroy(gameObject);
        }
    }

    private float Bezier(float t, float a, float b, float c)
    {
        return Mathf.Pow((1 - t), 2) * a
            + 2*t * (1 - t) * b
            + Mathf.Pow(t, 2) * c;
    }

    public virtual void SetRotation(Vector2 direction, bool isFLipX = false)
    {
        // 계산된 각도
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float rotationSpeed = 30f;

        // 방향에 따른 회전 설정
        if (direction.x < 0)
        {
            // 왼쪽을 보도록 회전
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

            // 스케일 설정
            Vector3 scale = transform.localScale;
            scale.x = isFLipX ? -1 : 1;
            transform.localScale = scale;

            // 부드러운 회전 보간
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        else
        {
            // 오른쪽을 보도록 회전
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle) * Quaternion.Euler(0, 180, 0);

            // 스케일 설정
            Vector3 scale = transform.localScale;
            scale.x = isFLipX ? 1 : -1;
            transform.localScale = scale;

            // 부드러운 회전 보간
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        // collision 계산 없음
            if (Unit.IsExistInLayerMask(other.gameObject.layer,mask) && other.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(attackInfo);
                ManagerRoot.Sound.PlaySfx("Flame Whoosh 7");
                ManagerRoot.Sound.PlaySfx("Dagger Stab (Flesh) 2");
                
                Destroy(gameObject);
                // if (SlowPercent > 0)
                // {
                //     damageable.TakeSlow(SlowPercent, SlowDuration);
                // }
            }
    }
}
