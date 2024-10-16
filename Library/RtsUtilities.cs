using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RtsUtilities
{
    public static List<Vector2> GetOrderedPositionOffsetsForUnits(List<Unit> units_, Vector2 targetCenter_, float startingRadius_,
        float radiusIncrement_, int startingUnitCountInRing, int unitCountMultiplier, int totalUnitCount)
    {
        //가장 먼 유닛부터 정렬
        List<Unit> unitsByFarthest = units_
            .OrderByDescending(x => Vector2.Distance(x.transform.position, targetCenter_))
            .ToList();
        
        //목적지들 생성
        List<Vector2> offsets = GetPositionOffsetsInCircle(startingRadius_, radiusIncrement_, startingUnitCountInRing,
            unitCountMultiplier, totalUnitCount);

        //가장 먼 유닛 기준으로 목적지들 정렬
        List<Vector2> orderedOffsets = new();
        for (int i = 0; i < unitsByFarthest.Count; i++)
        {
            Vector2 unitPos = unitsByFarthest[i].transform.position;
            Vector2 targetOffset = offsets.OrderBy(x => Vector2.Distance(unitPos, x + targetCenter_)).FirstOrDefault();
            offsets.Remove(targetOffset);
            orderedOffsets.Add(targetOffset);
        }

        List<Vector2> result = new();
        for (int i = 0; i < units_.Count; i++)
        {
            for (int j = 0; j < unitsByFarthest.Count; j++)
            {
                if (units_[i] == unitsByFarthest[j])
                {
                    result.Add(orderedOffsets[j]);
                    break;
                }
            }
        }

        return result;
    }
    
    public static List<Vector2> GetPositionOffsetsInCircle(float startingRadius_, float radiusIncrement_, int startingUnitCountInRing, int unitCountMultiplier_, int totalUnitCount_)
    {
        List<Vector2> positionList = new List<Vector2>();
        float radius = startingRadius_;
        int unitCountInRing = startingUnitCountInRing;
        int remainingUnitCount = totalUnitCount_;

        if (totalUnitCount_ < 1)
        {
            Debug.LogWarning("Tried to get position list in circle with zero or less total unit count.");
            return null;
        }
        
        while (remainingUnitCount > 0)
        {
            if (remainingUnitCount < unitCountInRing)
            {
                unitCountInRing = remainingUnitCount;
            }
        
            List<Vector2> positionListInRing = GetPositionOffsetsInRing(radius, unitCountInRing);
            positionList.AddRange(positionListInRing);
        
            remainingUnitCount -= unitCountInRing;
            unitCountInRing *= unitCountMultiplier_;
            radius += radiusIncrement_;
        }
        return positionList;
    }
    
    private static List<Vector2> GetPositionOffsetsInRing(float radius_, int unitCount_)
    {
        List<Vector2> positionList = new List<Vector2>();
        float angle = 360f / unitCount_;
        for (int i = 0; i < unitCount_; i++)
        {
            float x = radius_ * Mathf.Cos(angle * i * Mathf.Deg2Rad);
            float y = radius_ * Mathf.Sin(angle * i * Mathf.Deg2Rad);
            positionList.Add(new Vector2(x, y));
        }
        return positionList;
    }
}
