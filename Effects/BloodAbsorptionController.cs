using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BloodAbsorptionController : MonoBehaviour
{
    private Unit _target;
    private Vector2 _targetPos = Vector2.zero;
    private float movementSpeed = 40f;
    private float time = 0f;
    private float duration = 2f;
    private Vector2 initialPlayerPosition;
    private Vector2 initialTargetPos;
    public int unitType;
    
    public void JumpToRandomHeight()
    {
        float randomHeight = Random.Range(2.5f, 4f);

        transform.DOJump(_target.transform.position, randomHeight, 1, .5f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.DOMove(new Vector3(_target.transform.position.x, _target.transform.position.y + .8f, _target.transform.position.z), .1f)
                    .SetEase(Ease.OutExpo)
                    .OnComplete(() =>
                    {
                        Destroy(gameObject);
                    });
            });
    }

    public void JumpToPosition(Vector2 targetPos_)
    {
        // float randomHeight = Random.Range(-4f, 4f);
        // _targetPos = targetPos_;
        // transform.DOJump(_targetPos, randomHeight, 1, .5f)
        //     .SetEase(Ease.OutQuad)
        //     .OnComplete(() =>
        //     {
        //         //ResetTargetPos();
        //         transform.DOMove(new Vector3(_targetPos.x, _targetPos.y + .8f, 0f), .1f)
        //             .SetEase(Ease.OutExpo)
        //             .OnComplete(() =>
        //             {
        //                 Destroy(gameObject);
        //             });
        //     });
        
    }

    
    private void Start()
    {
        if (GameManager.Instance.GetPlayer() != null)
        {
            initialPlayerPosition = GameManager.Instance.GetPlayer().transform.position;
        }
    }

    private void Update()
    {
        if (GameManager.Instance.GetPlayer() != null)
        {
            Vector2 playerPositionDelta = GameManager.Instance.GetPlayer().transform.position - (Vector3)initialPlayerPosition;
            _targetPos = initialTargetPos + playerPositionDelta;

            time += Time.deltaTime;
            if (time < duration)
            {
                Vector3 target = Vector3.Slerp(transform.position, _targetPos, time / duration);
                transform.position = Vector3.MoveTowards(transform.position, target, movementSpeed * Time.deltaTime);
            }
            //거리 비교
            if (Vector2.Distance(transform.position, _targetPos) < .1f)
            {
                Destroy(gameObject);
            }
        }
    }
    
    
    public Unit target
    {
        get => _target;
        set
        {
            _target = value;
        }
    }
    
    public Vector2 targetPos
    {
        get => _targetPos;
        set
        {
            _targetPos = value;
            initialTargetPos = _targetPos;
        }
    }
}