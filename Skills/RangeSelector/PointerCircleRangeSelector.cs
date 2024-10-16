using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LOONACIA.Unity.Managers;
using System.Linq;
using LOONACIA.Unity;

public class PointerCircleRangeSelector : RangeSelector
{
    LayerMask _selectedLayer;
    GameObject _affectCircleGO;
    GameObject _previewCircleGO;

    float _currentCircleRadius; // NECRORADIUS // pointer
    float _currentPreviewRadius;

    float _circleExpandSpeed = 1f;
    float _circleExpandStep  = 1f;

    float _initCircleRadius = 1f;  // todo: DataSO에서 slider로 조절 가능하도록
    float _maxCircleRadius = 5f;
    
    float _delayTimer = 1f;
    float _increaseDelayTime = 1f;

    float _corpseWeight = 1f;
    
    const float NECRORADIUSIncreaseDelay = 1f;

    Camera _cam;


    public PointerCircleRangeSelector(LayerMask layer_)
    {
        _selectedLayer = layer_;

        _affectCircleGO  = ManagerRoot.Resource.Load<GameObject>("Prefabs/NecroCircleArea");
        _affectCircleGO = Object.Instantiate(_affectCircleGO);

        _previewCircleGO = ManagerRoot.Resource.Load<GameObject>("Prefabs/NecroCircleArea");
        _previewCircleGO = Object.Instantiate(_previewCircleGO);

        _affectCircleGO.SetActive(false);
        _affectCircleGO.GetComponent<SpriteRenderer>().color = new Color32(128, 255, 128, 100);

        _previewCircleGO.SetActive(false);
        _previewCircleGO.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 100);
        _previewCircleGO.GetComponent<SpriteRenderer>().sortingOrder = -1;

        _cam = Camera.main;
    }


    public override void Init()
    {
        base.Init();
        _currentPreviewRadius = 0;
        _currentCircleRadius = 0;

    }
    public override void RangeSelect()
    {
        base.RangeSelect();
        // 범위 중심 위치 셋팅
        Vector3 circleCenter = (Vector2) _cam.ScreenToWorldPoint(Input.mousePosition); // 마우스 위치

        // 리바이브 미리보기
        _previewCircleGO.SetActive(true);
        _previewCircleGO.transform.position = (Vector2)circleCenter;

        _currentPreviewRadius = ExpandValueByTime(_currentPreviewRadius, _maxCircleRadius, _circleExpandSpeed);
        _previewCircleGO.transform.localScale = Vector3.one * _currentPreviewRadius * 2;


        // 리바이브
        _affectCircleGO.SetActive(true);
        _affectCircleGO.transform.position = (Vector2)circleCenter;

        _currentCircleRadius = ExpandValueByTimeStep(_currentCircleRadius, _maxCircleRadius, _circleExpandSpeed, _circleExpandStep, _increaseDelayTime );

        _affectCircleGO.transform.localScale = Vector3.one * _currentCircleRadius   * 2;
     
        SelectCorpseCircle();
    }
    public override void ResetValues()
    {
        base.ResetValues();
        _affectCircleGO.SetActive(false);
        _previewCircleGO.SetActive(false);

        _currentCircleRadius  = _initCircleRadius;
        _currentPreviewRadius = _initCircleRadius;

        _affectCircleGO.transform.localScale  = default;
        _previewCircleGO.transform.localScale = default;
    }


    float ExpandValueByTime(float value_, float max_, float speed_)
    {
        if (value_ < max_)
        {
            value_ += Time.deltaTime * speed_ ;
        }
        return value_;
    }

    float ExpandValueByTimeStep(float value_, float max_, float speed_, float step_, float delayTime_)
    {
        _delayTimer -= Time.deltaTime * speed_ * 1f/_corpseWeight;
        if (_delayTimer <= 0)
        {
            if (value_ < max_)
            {
                value_ += step_;
            }
            _delayTimer = delayTime_;
        }
        return value_;
    }


    void SelectCorpseCircle()
    {
        Collider2D[] selected = GetCorpseListCircle(_affectCircleGO.transform.position, _currentCircleRadius);
        EnqueueSelected(selected);
        DequeueOoRSelected(selected);
        
    }

    Collider2D[] GetCorpseListCircle(Vector3 centerPoint_, float currentCircleRadius_)
    {
        var selected = Physics2D.OverlapCircleAll(centerPoint_, currentCircleRadius_, _selectedLayer);
        return selected;
    }

}
