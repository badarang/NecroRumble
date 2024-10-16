    using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity;
using UnityEngine;


public class FrontCircleRangeSelector : RangeSelector
{
    private LayerMask _selectLayers;
    protected const float NECRORADIUSIncreaseDelay = 1f;

    protected GameObject _affectCircleGO;

    float _angle;
    Vector2 frontDirection;

    Vector2 _circleCenter;

    float _currentCircleRadius = 0;
    float _currentCircleDistance = 0f;

    float _maxCircleDistance = 10f;
    float _circleDistanceExpandSpeed = 1f;

    protected float _corpseWeight = 1f;


    // float _initCircleRadius = 1f;
    // float _maxCircleRadius  = 5f;

    public FrontCircleRangeSelector(LayerMask layers_, float maxCircleDistance_ = 10f, float circleDistanceExpandSpeed_ = 1f)
    {
        Init();
        _selectLayers = layers_;
        _maxCircleDistance = maxCircleDistance_;
        _circleDistanceExpandSpeed = circleDistanceExpandSpeed_;


        _affectCircleGO = Resources.Load<GameObject>("Prefabs/NecroCircleArea");
        _affectCircleGO = Object.Instantiate(_affectCircleGO);

        _affectCircleGO.SetActive(false);
        _affectCircleGO.GetComponent<SpriteRenderer>().color = new Color32(128, 255, 128, 100);

    }


    public override void Init()
    {
        base.Init();
        _currentCircleRadius = 0;
        _currentCircleDistance = 0;
        _circleCenter = default;
    }

    public override void RangeSelect()
    {
        base.RangeSelect();
        // angle 계산 : Direction 계산과 비교
        // Vector2 playerPosition = _reviveBase.transform.position;
        // Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // frontDirection = (mousePosition - playerPosition).normalized;
        // _angle = Mathf.Atan2(frontDirection.y, frontDirection.x) * Mathf.Rad2Deg;

        // Direction 계산
        Vector2 playerPosition = _player.transform.position;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        frontDirection = (mousePosition - playerPosition).normalized;


        // 범위 중심 위치 셋팅
        _currentCircleDistance = ExpandValueByTime( _currentCircleDistance, _maxCircleDistance, _circleDistanceExpandSpeed);
        Vector3 circleCenter = (Vector2)_player.transform.position + (frontDirection * _currentCircleDistance);


        // 리바이브
        _affectCircleGO.SetActive(true);
        _affectCircleGO.transform.position = (Vector2)circleCenter;
        
        _currentCircleRadius = ExpandValueByTime( _currentCircleDistance, _maxCircleDistance, _circleDistanceExpandSpeed );
        
        _affectCircleGO.transform.localScale = Vector3.one * _currentCircleRadius   * 2;
          
        // Selector는 RangeCalculator와 분리될수가 없음
        SelectCorpseSquare(_circleCenter, _affectCircleGO.transform.localScale, _angle);

    }

    float ExpandValueByTime(float value_, float max_, float speed_)
    {
        if (value_ < max_)
        {
            value_ += Time.deltaTime * speed_ ;
        }
        return value_;
    }

    public override void ResetValues()
    {
        base.ResetValues();
        Debug.Log($"ResetValues");

        _affectCircleGO.SetActive(false);
        _affectCircleGO.transform.localScale  = default;

        _currentCircleRadius = 0;
        _currentCircleDistance = 0f;
    }

    void SelectCorpseSquare(Vector3 centerPoint_, Vector2 size_, float angle_)
    {
        Collider2D[] select = GetCorpseListSquare(centerPoint_, size_, angle_);
        
        EnqueueSelected(select);
        DequeueOoRSelected(select);
    }


    Collider2D[] GetCorpseListSquare(Vector3 centerPoint_, Vector2 size_, float angle_)
    {
        var select = Physics2D.OverlapBoxAll(centerPoint_, size_, angle_, _selectLayers);
        return select;
    }

}
