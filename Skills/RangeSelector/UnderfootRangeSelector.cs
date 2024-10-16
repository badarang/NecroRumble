using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderfootRangeSelector : RangeSelector
{

    public UnderfootRangeSelector()
    {
    }

    public override void Init()
    {
        base.Init();
    }
    public override void RangeSelect()
    {
        base.RangeSelect();
        //Unit corpse = _player.GetLastCorpseUnderfoot();

        List<Unit> selectedUnits = _player.GetListCorpseUnderfoot();
        Unit selectedUnit = FindClosestCorpse(selectedUnits);

        if (selectedUnit == null) return;

        EnqueueSelected(selectedUnit);
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }

    Unit FindClosestCorpse(List<Unit> selectedUnits_)
    {
        Unit closestUnit = null;
        float minDistance = float.MaxValue;

        foreach (var selected in selectedUnits_)
        {
            float distance = Vector3.Distance(_player.transform.position, selected.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestUnit = selected;
            }
        }

        return closestUnit;
    }

}
