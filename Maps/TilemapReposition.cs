using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class TilemapReposition : MonoBehaviour
{
    private const float LOOP_DELTA_TIME = 2f;
    
    [SerializeField] private Transform _gridHolder;

    private Player _player;
    private WaitForSeconds _loopWait = new(LOOP_DELTA_TIME);
    private Coroutine _loopCheckCo;

    private void Start()
    {
        if (_gridHolder == null) Debug.LogError("Grid Holder를 찾지 못함. 할당 필요.");
        
        _player = FindObjectOfType<Player>();
        
        if (_loopCheckCo != null) StopCoroutine(_loopCheckCo);
        _loopCheckCo = StartCoroutine(LoopCheck());
    }

    private IEnumerator LoopCheck()
    {
        while (true)
        {
            //플레이어와 닿아있는 모든 타일의 리스트를 가져오기
            var touchingGrounds = _player.GetGrounds();
            if (touchingGrounds.Length > 0)
            {
                //닿은 타일 중 가운데 타일이 없을 때
                if (touchingGrounds.Any(tile => Vector2.Distance(tile.transform.position, _gridHolder.position) < 1f) == false)
                {
                    //가장 왼쪽 아래에 있는 타일 하나 선택
                    var leftBottomTile = touchingGrounds
                        .OrderBy(tile =>
                        {
                            Vector2 tilePos = tile.transform.position;
                            return (tilePos.x + tilePos.y);
                        })
                        .FirstOrDefault();
                    
                    //그 타일이 이 게임오브젝트이면
                    if (leftBottomTile != null && leftBottomTile.transform == transform)
                    {
                        //해당 타일 위치로 전체 그리드 이동
                        _gridHolder.transform.position = leftBottomTile.transform.position;
                    }
                }
            }
            else
            {
                //만에 하나 플레이어와 닿아있는 땅이 없는 경우 (절대 있어선 안됨)
                Debug.LogWarning("플레이어와 닿아있는 땅이 없음.");
                
                //최후의 수단을 플레이어 위치로 전체 그리드를 이동. (바닥이 갑자기 바뀌는 것처럼 보일 수 있음)
                //_gridHolder.transform.position = _player.transform.position;
            }
            
            yield return _loopWait;
        }
    }
}