using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimationEvents : MonoBehaviour
{
    private Unit _unit;
    
    private void Awake()
    {
        _unit = GetComponentInParent<Unit>();
    }

    private void AE_HitMoment()
    {
        _unit.AnimEvent_HitMoment();
    }
    private void AE_GuardMoment()
    {
        _unit.AnimEvent_GuardMoment();
    }
    private void AE_HealMoment()
    {
        _unit.AnimEvent_HealMoment();
    }
}
