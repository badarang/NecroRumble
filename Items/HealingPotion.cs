using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;
public class HealingPotion : ItemBase
{
    private uint _value;
    private float _delay;
    protected override void Start()
    {
        base.Start();
        SetValue(20);
        SetDelay(.5f);

    }

    protected override void Update()
    {
        if (_delay > 0)
        {
            _delay -= Time.deltaTime;
        }
        else
        {
            base.Update();
        }
    }

    public void SetValue(uint value)
    {
        _value = value;
    }

    private void SetDelay(float delay)
    {
        _delay = delay;
    }

    protected override void HandleCollectEvent()
    {
        if (_delay > 0) return;
        if (player != null)
        {
            if (player.TryGetComponent(out Player _player))
            {
                _player.TakeHeal(_value);
                ManagerRoot.Sound.PlaySfx("Power up 5", 0.4f);
            }
        }
        ManagerRoot.Resource.Release(gameObject);
    }


}
