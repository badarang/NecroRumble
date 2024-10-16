using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using UnityEngine;

// Circle + Charge
public class ChargingRangeSelector : RangeSelector
{
    //GameObject _previewCircleGO;
    GameObject _affectCircleGO;

    //float _currentPreviewRadius;
    float _currentCircleRadius; // NECRORADIUS // pointer

    // float _circleExpandSpeed = 1f;
    // float _circleExpandStep  = 1f;

    float _initCircleRadius = 0f;  // todo: DataSO에서 slider로 조절 가능하도록
    float _maxCircleRadius = 5f;

    float _delayTimer = 1f; // NECRORADIUSIncreaseDelay 기본 값. // charge
    //float _increaseDelayTime = 1f;

    float _corpseWeight = 1f;
    
    //float _elapsedNormalizedTime = 0f;
    float _elapsedTime = 0f;
    private LayerMask _selectLayers;


    public ChargingRangeSelector(LayerMask layers_, float maxCircleRadis_ = 5f, float delayTimer_ = 1f)
    {
        Init();
        _selectLayers = layers_;
        _maxCircleRadius = maxCircleRadis_;
        _delayTimer = delayTimer_;

        _affectCircleGO = Resources.Load<GameObject>("Prefabs/NecroCircleArea");
        _affectCircleGO = Object.Instantiate(_affectCircleGO); // 꼭 필요?

        // _previewCircleGO = Resources.Load<GameObject>("Prefabs/NecroCircleArea");
        // _previewCircleGO = Object.Instantiate(_previewCircleGO);

        _affectCircleGO.SetActive(false);
        _affectCircleGO.GetComponent<SpriteRenderer>().sortingOrder = 10;
        //_affectCircleGO.GetComponent<SpriteRenderer>().color = new Color32(128, 255, 128, 100);

        // _previewCircleGO.SetActive(false);
        // _previewCircleGO.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 100);
        // _previewCircleGO.GetComponent<SpriteRenderer>().sortingOrder = -1;

    }

    public override void Init()
    {
        base.Init();
        // _currentPreviewRadius = 0;
        _currentCircleRadius = 0;
        // _revive.reviveState = ReviveState.Casting;
    }

    public override void RangeSelect()
    {
        base.RangeSelect();
        // 범위 중심 위치 셋팅
        Vector3 circleCenter = _player.transform.position; // 플레이어 위치 의도했음


        // 리바이브 미리보기
        // _previewCircleGO.SetActive(true);
        // _previewCircleGO.transform.position = (Vector2)circleCenter;
        //
        // _currentPreviewRadius = ExpandValueByTime(_currentPreviewRadius, _maxCircleRadius, _circleExpandSpeed);
        //
        // _previewCircleGO.transform.localScale = Vector3.one * _currentPreviewRadius * 2;


        // 리바이브
        _affectCircleGO.SetActive(true);
        _affectCircleGO.transform.position = (Vector2)circleCenter;
        
        //_currentCircleRadius = ExpandValueByTimeStep(_currentCircleRadius, _maxCircleRadius, _circleExpandSpeed, _circleExpandStep, _increaseDelayTime );
        //_currentCircleRadius = ExpandValueByEaseInOutExpo(_currentCircleRadius, _maxCircleRadius, _circleExpandSpeed);
        // if (_elapsedTime >= 1f)
        // {
        //     _elapsedTime = 0f;
        //     _currentStep++;
        // }
        
        _currentCircleRadius = GetValueAtTime();
        // _elapsedNormalizedTime += Time.deltaTime;
        // if (_elapsedNormalizedTime >= 1f)
        // {
        //     _elapsedNormalizedTime = 0f;
        // }
        _elapsedTime += Time.deltaTime;
        _affectCircleGO.transform.localScale = new Vector3(_currentCircleRadius * 2,_currentCircleRadius * 1f, 1f);
        
        SelectCorpseCircle();
    }

    public override void ResetValues()
    {
        base.ResetValues();
        _affectCircleGO.SetActive(false);
        // _previewCircleGO.SetActive(false);

        _currentCircleRadius  = _initCircleRadius;
        //_currentPreviewRadius = _initCircleRadius;

        _affectCircleGO.transform.localScale  = default;
        _elapsedTime = 0f;
        //_elapsedNormalizedTime = 0f;
        // _previewCircleGO.transform.localScale = default;
    }


    float ExpandValueByTime(float value_, float max_, float speed_)
    {
        if (value_ < max_)
        {
            value_ += Time.deltaTime * speed_ ;
        }
        return value_;
    }
    
    private float ExpandValueByEaseInOutExpo(float initValue, float maxValue, float totalDuration)
    {
        // Calculate the percentage of completion based on elapsed time
        float t = _elapsedTime;

        // Determine the current step based on the percentage of completion
        int currentStep = Mathf.FloorToInt(t * (maxValue - initValue + 1));

        // Calculate the eased value for the current step
        float easedValue = EaseInOutExpo(t * (maxValue - initValue + 1) - currentStep);

        // Calculate the expanded value based on the current step
        float expandedValue = Mathf.Lerp(initValue + currentStep, initValue + currentStep + 1, easedValue);

        // Increment elapsed time by 1 second per frame
        _elapsedTime += Time.deltaTime;

        return expandedValue;
    }
    float EaseInOutExpo(float t)
    {
        if (t == 0.0f) return 0.0f;
        if (t == 1.0f) return 1.0f;

        if (t < 0.5f)
        {
            return 0.5f * Mathf.Pow(2.0f, 10.0f * (2.0f * t - 1.0f));
        }
        else
        {
            return 0.5f * (-Mathf.Pow(2.0f, -10.0f * (2.0f * t - 1.0f)) + 2.0f);
        }
    }

    float GetValueAtTime()
    {
        var stair = Mathf.FloorToInt(_elapsedTime);
        var normalizedTime = _elapsedTime - stair;
        
        if (_elapsedTime >= _maxCircleRadius) return _maxCircleRadius;
        float y = EaseInOutExpo(normalizedTime) + Mathf.Min(stair, _maxCircleRadius);
        return y;
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
        Collider2D[] selected = GetCorpseListEllipse(_affectCircleGO.transform.position, _currentCircleRadius * .9f, _currentCircleRadius * .9f * .5f);
        EnqueueSelected(selected);
        DequeueOoRSelected(selected);
    }

    Collider2D[] GetCorpseListCircle(Vector3 centerPoint_, float currentCircleRadius_)
    {
        var selected = Physics2D.OverlapCircleAll(centerPoint_, currentCircleRadius_, Layers.HumanCorpse.ToMask());
        return selected;
    }
    
    Collider2D[] GetCorpseListEllipse(Vector3 centerPoint, float _width, float _height)
    {
        var selected = Physics2D.OverlapAreaAll(
            new Vector2(centerPoint.x - _width, centerPoint.y - _height),
            new Vector2(centerPoint.x + _width, centerPoint.y + _height),
            _selectLayers
        );

        return selected;
    }
}
