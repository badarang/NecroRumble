using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;


public class ReviveNewInputHandler : IInputHandler
{
    private Revive _reviveBase;

    public ReviveNewInputHandler(Revive reviveBase_)
    {
        _reviveBase = reviveBase_;

        ManagerRoot.Input.OnRevivePressed += RevivePressed;
        ManagerRoot.Input.OnReviveReleased += ReviveReleased;
    }

    ~ReviveNewInputHandler()
    {
        ManagerRoot.Input.OnRevivePressed -= RevivePressed;
        ManagerRoot.Input.OnReviveReleased -= ReviveReleased;
    }

    void RevivePressed()
    {
        if (!_reviveBase.GetCanCast())
        {
            ManagerRoot.Sound.PlaySfx("Error Sound 4", 0.6f);
            
            if (_reviveBase.IsCooldown)
            {
                GameUIManager.Instance.SetSkillCoolDownText();
                GameUIManager.Instance.SetReviveDoScale();
            }
            else
            {
                if (GameManager.Instance.GetRemainUndeadNum() <= 0)
                {
                    GameUIManager.Instance.SetSkillCoolDownText(1);
                }
            }
            return;
        }

        _reviveBase.reviveState = ReviveState.CastStart;
    }

    void ReviveReleased()
    {
        if (_reviveBase.reviveState == ReviveState.Casting)
        {
            _reviveBase.reviveState = ReviveState.CastEnd;
        }
    }

    public void HandleInput()
    {
        // if (_input.ReviveButton && !isTrigger)
        // {
        //     isTrigger = true;
        //     if( !_reviveBase.CanCast() ){ isTrigger = false; return; }

        //     _reviveBase.reviveState = ReviveState.CastStart;
        // }

        // if (!_input.ReviveButton && isTrigger )
        // {
        //     if( _reviveBase.reviveState == ReviveState.Casting ){
        //         _reviveBase.reviveState = ReviveState.CastEnd;
        //         isTrigger = false;
        //     }
        // }
    }
}