using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity;
using UnityEngine;


// Circle // no charge
public class CircleRangeSelector : RangeSelector
{

    private LayerMask _selectLayers;
    protected GameObject _affectCircleGO;
    protected float _currentCircleRadius = 1.2f; // NECRORADIUS // pointer


    public CircleRangeSelector(LayerMask layers_, float currentCircleRadius_ = 1.2f)
    {
        Init();
        _selectLayers = layers_;
        _currentCircleRadius = currentCircleRadius_;
        _affectCircleGO = Resources.Load<GameObject>("Prefabs/NecroCircleArea");
        _affectCircleGO = Object.Instantiate(_affectCircleGO); // 꼭 필요?

        _affectCircleGO.SetActive(false);
        _affectCircleGO.GetComponent<SpriteRenderer>().sortingOrder = 10;
    }

    public override void Init()
    {
        base.Init();
        _currentCircleRadius = 1.2f;
    }

    public override void RangeSelect()
    {
        base.RangeSelect();
        _affectCircleGO.SetActive(true);
        _affectCircleGO.transform.position   = _player.transform.position; // 플레이어 위치
        _affectCircleGO.transform.localScale = Vector3.one * _currentCircleRadius * 2;

        SelectCorpseCircle(); 
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }

    protected void SelectCorpseCircle()
    {
        Collider2D[] selected = GetCorpseListCircle(_affectCircleGO.transform.position, _currentCircleRadius);
        EnqueueSelected(selected);
        DequeueOoRSelected(selected);
        
    }

    protected Collider2D[] GetCorpseListCircle(Vector3 centerPoint_, float currentCircleRadius_)
    {
        var selected = Physics2D.OverlapCircleAll(centerPoint_, currentCircleRadius_, _selectLayers);
        return selected;
    }

}
