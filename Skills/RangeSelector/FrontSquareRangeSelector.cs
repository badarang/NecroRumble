    using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity;
using UnityEngine;


// Square
public class FrontSquareRangeSelector : RangeSelector
{
    private LayerMask _selectLayers;
    protected const float NECRORADIUSIncreaseDelay = 1f;

    protected GameObject _affectSquareGO;

    Vector2 frontDirection;
    float _angle;

    float _currentWidth  = 0;
    float _maxWidth = 2f;
    float _widthExpandSpeed = 0.3f;

    float _currentHeight = 0;
    float _maxHeight = 50f;
    float _heightExpandSpeed = 7f;

    float _currentSquareDistance = 0f;
    float _maxSquareDistance = 25f;
    float _squareDistanceExpandSpeed = 3.5f;// 7f/2f; // _heightExpandSpeed/2f

    Vector2 _squareCenter = default;


    protected float _corpseWeight = 1f;


    public FrontSquareRangeSelector(LayerMask layers_)
    {
        _selectLayers = layers_;

        _affectSquareGO = Resources.Load<GameObject>("Prefabs/NecroSquareArea");
        _affectSquareGO = Object.Instantiate(_affectSquareGO);

        _affectSquareGO.SetActive(false);
        _affectSquareGO.GetComponent<SpriteRenderer>().color = new Color32(128, 255, 128, 100);

    }


    public override void Init()
    {
        base.Init();
    }

    public override void RangeSelect()
    {
        base.RangeSelect();
        // 범위 중심 위치 셋팅
        _currentSquareDistance = ExpandValueByTime(_currentSquareDistance, _maxSquareDistance, _squareDistanceExpandSpeed);
        _squareCenter = (Vector2)_player.transform.position + (frontDirection * _currentSquareDistance);

        // angle 계산
        Vector2 playerPosition = _player.transform.position;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        frontDirection = (mousePosition - playerPosition).normalized;
        _angle = Mathf.Atan2(frontDirection.y, frontDirection.x) * Mathf.Rad2Deg;


        // 리바이브
        _affectSquareGO.SetActive(true);
        _affectSquareGO.transform.position = _squareCenter;
        

        _currentHeight = ExpandValueByTime( _currentHeight, _maxHeight,  _heightExpandSpeed );
        _currentWidth  = ExpandValueByTime( _currentWidth,  _maxWidth,   _widthExpandSpeed  );

        _affectSquareGO.transform.localScale = new Vector3(_currentHeight,_currentWidth, 1);
        _affectSquareGO.transform.rotation   = Quaternion.Euler(0, 0, _angle);
        
        // Selector는 RangeCalculator와 분리될수가 없음
        SelectCorpseSquare(_squareCenter, _affectSquareGO.transform.localScale, _angle);
    }


    protected float ExpandValueByTime(float value_, float max_, float speed_)
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

        _affectSquareGO.SetActive(false);

        _currentWidth  = 0;
        _currentHeight = 0;
        _currentSquareDistance = 0f;
        _squareCenter = default;


        _affectSquareGO.transform.localScale  = default;
    }

    protected void SelectCorpseSquare(Vector3 centerPoint_, Vector2 size_, float angle_)
    {
        Collider2D[] selected = GetCorpseListSquare(centerPoint_, size_, angle_);
        
        EnqueueSelected(selected);
        DequeueOoRSelected(selected);
    }


    protected Collider2D[] GetCorpseListSquare(Vector3 centerPoint_, Vector2 size_, float angle_)
    {
        var selected = Physics2D.OverlapBoxAll(centerPoint_, size_, angle_, _selectLayers);
        return selected;
    }

}
