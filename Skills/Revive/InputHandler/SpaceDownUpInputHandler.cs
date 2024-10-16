using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpaceDownUpInputHandler : IInputHandler
{
    private Revive _reviveBase;

    public SpaceDownUpInputHandler(Revive reviveBase_)
    {
        _reviveBase = reviveBase_;
    }

    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if( !_reviveBase.GetCanCast() ){ return; }

            _reviveBase.reviveState = ReviveState.CastStart;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if( _reviveBase.reviveState == ReviveState.Casting ){
                _reviveBase.reviveState = ReviveState.CastEnd;
            }
        }
    }
}