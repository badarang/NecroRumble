using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityTransform;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class AurorProjectile : Projectile
{
    LayerMask _targetLayer;
    float _timer = 0;
    public void SetFactonInfo(Sprite sprite_, LayerMask targetLayerMask_)
    {
        // _dealtFirstDamage = false;
        // _originalDamage = _damage;
        _targetLayer = targetLayerMask_;
        GetComponent<SpriteRenderer>().sprite = sprite_;
    }

    protected override void Update()
    {
        _timer += Time.deltaTime;
        if (Vector3.Distance(_startPos, transform.position) > _destroyRange)
        {
            DestroyAuror();
        }
        if(_timer >= 3f)
        {
            DestroyAuror();
        }
    }

    void DestroyAuror()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(transform.localScale.x);
        scale.y = Mathf.Abs(transform.localScale.y);
        transform.localScale = scale;

        ManagerRoot.Resource.Release(gameObject);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (Unit.IsExistInLayerMask(other.gameObject.layer, _targetLayer))
        {
            TryTakeDamage(other.transform);
            var effectSounds = new List<string> {"Dagger Stab (Flesh) 2", "Dagger Stab (Flesh) 3", "Dagger Stab (Flesh) 4", "Dagger Stab (Flesh) 5"};
            ManagerRoot.Sound.PlaySfx(effectSounds[Random.Range(0, effectSounds.Count)], 1f);
        }
    }

}
