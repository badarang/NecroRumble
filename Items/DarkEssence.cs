using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;
public class DarkEssence : ItemBase
{
    private uint _value;
    protected override void Start()
    {
        base.Start();
        SetValue(1);
    }

    protected override void Update()
    {
        base.Update();
    }

    public void SetValue(uint value)
    {
        _value = value;
    }

    protected override void HandleCollectEvent()
    {
        if (player != null)
        {
            if (player.TryGetComponent(out Player _player))
            {
                _player.GainDarkEssence(_value);
                ManagerRoot.Sound.PlaySfx("Click sound 17", 0.4f);
                GameManager.Instance.GameUI.SetDarkEssenceUI(_player.DarkEssence);
            }
        }
        ManagerRoot.Resource.Release(gameObject);
    }
}
