using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class RewardNavigator : MonoBehaviour
{
    public Transform pointerArrow;
    public Vector2 playerPositionOffset;
    public float pointerArrowDistance = 3f;

    private ChunkManager _chunk;
    private List<ItemBase> _rewards = new List<ItemBase>();
    private ItemBase _closestReward;
    private Coroutine _loopUpdateClosestRewardRoutine;
    
    private Vector2 PlayerPos => (Vector2) transform.position + playerPositionOffset;

    public void Awake()
    {
        _chunk = ManagerRoot.Chunk;
        
        //0.1초마다 가장 가까운 보상 뭔지 업데이트 하는 코루틴 실행.
        if (_loopUpdateClosestRewardRoutine != null) StopCoroutine(_loopUpdateClosestRewardRoutine);
        _loopUpdateClosestRewardRoutine = StartCoroutine(LoopUpdateClosestReward());
    }

    void Update()
    {
        //PointerArrow 위치 및 회전 업데이트
        UpdatePointerArrow();

        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log(_closestReward.name);
        }
    }

    private void UpdatePointerArrow()
    {
        //가까운 리워드가 있는지 체크
        pointerArrow.gameObject.SetActive(_closestReward != null);
        if (_closestReward == null) return;
        
        //check closest reward distance
        float distance = Vector2.Distance(_closestReward.transform.position, PlayerPos);
        if (distance < 5f)
        {
            pointerArrow.gameObject.SetActive(false);
            return;
        }
        else
        {
            pointerArrow.gameObject.SetActive(true);
        }
        
        //Update Position
        Vector2 closestRewardPos = _closestReward.transform.position;
        Vector2 dir = (closestRewardPos - PlayerPos).normalized;

        pointerArrow.position = PlayerPos + dir * pointerArrowDistance;
        
        //Update Rotation
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        pointerArrow.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    
    private IEnumerator LoopUpdateClosestReward()
    {
        while (true)
        {
            _closestReward = GetClosestReward();
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private ItemBase GetClosestReward()
    {
        var closestItem = _chunk.AllItemsInScene
            .OrderBy(x => Vector2.Distance(x.transform.position, PlayerPos))
            .FirstOrDefault();
        return closestItem;
    }
}
