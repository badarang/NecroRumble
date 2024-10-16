using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceDownMouseDownUpInputHandler : IInputHandler
{
    private Revive _reviveBase;
    private bool _isReady = false;

    public SpaceDownMouseDownUpInputHandler(Revive reviveBase)
    {
        _reviveBase = reviveBase;
    }

    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if( !_reviveBase.GetCanCast() ){ return; }

            _isReady = true;

        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && _isReady)
        {
            _reviveBase.reviveState = ReviveState.CastStart;
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if( _reviveBase.reviveState == ReviveState.Casting ){
                // charging end
                // perform revive
                _reviveBase.reviveState = ReviveState.CastEnd;
                _isReady = false;
            }
        }
    }


}
