using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using MoreMountains.Feedbacks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;


// Circle // no charge
public class EclipseRangeSelector : RangeSelector
{

    private float _range;
    private LayerMask _selectLayers;
    private const float ELITE_GUARANTEE_RANGE = 1.3f;

    public EclipseRangeSelector(LayerMask layers_, float range_)
    {
        Init();
        _range = range_;
        _selectLayers = layers_;
    }

    public override void Init()
    {
        base.Init();
    }

    public override void RangeSelect()
    {
        base.RangeSelect();
        // if(GameManager.Instance.State == GameManager.GameState.WaveEnd)
        //     SelectCorpseAndUndeadCircle();
        // else
            SelectCircle();
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }

    protected void SelectCircle()
    {
        //원 방식
        Collider2D newSelectedOne = GetSingleClosestOneInCircle(_player.transform.position, _range, _selectLayers);
        
        //찾은 시체 없으면 기존 시체 선택 취소
        if (newSelectedOne == null)
        {
            ClearSelectedQueue();
            return;
        }
        
        //이미 선택된 시체라면 아무일도 안함.
        if (SelectedQ.TryPeek(out var oldSelectedOne))
        {
            if (oldSelectedOne == newSelectedOne.GetComponent<Unit>())
            {
                return;
            }
        }
        
        //기존 선택 취소
        ClearSelectedQueue();
        //새로운 시체 선택
        EnqueueSelected(newSelectedOne);
    }

    protected Collider2D GetSingleClosestOneInCircle(Vector2 centerPoint_, float currentCircleRadius_, LayerMask layer_)
    {
        var colliders = Physics2D.OverlapCircleAll(centerPoint_, currentCircleRadius_, layer_)
            .OrderBy(col => Vector2.Distance(col.transform.position, centerPoint_))
            .ToList();

        // 엘리트 우선 선택
        var elites = colliders
            .Where(col => col.GetComponent<Unit>() is { IsElite: true, CanRevive: true })
            .ToList();
        if (elites.Count > 0)
        {
            var elite = elites
                .OrderBy(col => Vector2.Distance(col.transform.position, centerPoint_))
                .FirstOrDefault(x => Vector2.Distance(x.transform.position, centerPoint_) < ELITE_GUARANTEE_RANGE);
            if (elite != null) return elite;
        }

        Collider2D closestOne = null;

        foreach (var collider in colliders)
        {
            var unit = collider.GetComponent<Unit>();
            if (unit != null && unit.CanRevive)
            {
                closestOne = collider;
                break;
            }
        }

        return closestOne;
    }

    Collider2D[] GetListEclipse(Vector3 centerPoint, float _width, float _height)
    {
        var selectedThings = Physics2D.OverlapAreaAll(
            new Vector2(centerPoint.x - _width, centerPoint.y - _height),
            new Vector2(centerPoint.x + _width, centerPoint.y + _height),
            _selectLayers
        );

        return selectedThings;
    }

}
