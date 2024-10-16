using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ParabolaEffectController : MonoBehaviour
{
    public float projectileHeight;
    public float speed;
    public Vector3 StartPos;
    public Vector3 EndPos;
    public float fireLerp = 0;
    public bool isGrounded = false;

    protected bool _useLocal = false;
    
    protected virtual void Update()
    {
        if (fireLerp < 1)
        {
            Vector3 newProjectilePos = CalculateTrajectory(StartPos, EndPos, fireLerp);
            if (!_useLocal)
            {
                transform.position = newProjectilePos;
            }
            else
            {
                transform.localPosition = newProjectilePos;
            }
            fireLerp += speed * Time.deltaTime;
        }
    }

    public void FireProjectile(Vector3 firePoint, Vector3 targetPos)
    {
        StartPos = firePoint;
        EndPos = targetPos;
        fireLerp = 0;
    }
    
    private Vector3 CalculateTrajectory(Vector3 firePos, Vector3 targetPos, float t)
    {
        Vector3 linearProgress = Vector3.Lerp(firePos, targetPos, t);
        float perspectiveOffset = Mathf.Sin(t * Mathf.PI) * projectileHeight;

        Vector3 trajectoryPos = linearProgress + (Vector3.up * perspectiveOffset);
        trajectoryPos.z = 0;
        return trajectoryPos;
    }
    
    public virtual void SetRotation(Vector2 direction, bool isFLipX = false)
    {
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        if (direction.x < 0)
        {
            transform.rotation *= Quaternion.Euler(0, 180, 0);
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            scale.y *= -1;
            transform.localScale = scale;
        }
    }
    
    
}
