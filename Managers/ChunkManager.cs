using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.SceneManagement;

public class ChunkManager
{
    //dictionary of Blocks with key of Vector2Int
    private Dictionary<Vector2Int, Block> _allBlocks = new Dictionary<Vector2Int, Block>();
    private ChunkData _data;
    private Player _player;
    
    private Vector2 CenterPosition => AllBlocks[Vector2Int.zero].worldPosition;
    public Dictionary<Vector2Int, Block> AllBlocks => _allBlocks;
    public ChunkData Data => _data;

    private static Coroutine tickBlockGenRoutine;

    private List<ItemBase> _allItemsInScene = new();

    public List<ItemBase> AllItemsInScene
    {
        get
        {
            _allItemsInScene = _allItemsInScene
                .Where(item => item != null)
                .Where(item => item.gameObject.activeInHierarchy)
                .Where(item => item is SkullUpgrade or Chest)
                .ToList();
            return _allItemsInScene;
        }
        private set => _allItemsInScene = value;
    }

    public void Init()
    {
        if (SceneManagerEx.CurrentScene.SceneType is SceneType.Title or SceneType.Tutorial) return;

        _player = Object.FindObjectOfType<Player>();
        _data = Resources.Load<ChunkData>("Data/ChunkData");
        
        //첫 블럭 생성
        InstantiateNewBlock(Vector2Int.zero);
        //시작시 주변 데드존 생성: 첫 블럭 주변으로 범위 내 블럭 생성하고 전부 청크 만들지 않고 고정
        var deadZoneBlocks = GetEnsuredBlocksInRange(CenterPosition, Data.StartingDeadzoneRange);
        deadZoneBlocks.ForEach(block => block.isFixed = true);
        //데드존, 그리드와 무관하게 시작 청크 생성.
        var randomPos = CenterPosition.PointInCircle(_data.StartingChunkRange);
        InstantiateGameObject(_data.StartingChunkPrefab, randomPos);
        
        //반복 코루틴 시작: 시시각각 변하는 플레이어 위치를 기준으로 블럭 생성
        if (tickBlockGenRoutine != null) CoroutineHandler.StopCoroutine(tickBlockGenRoutine);
        tickBlockGenRoutine = CoroutineHandler.StartCoroutine(TickBlockGenerationRoutine());
    }

    private IEnumerator TickBlockGenerationRoutine()
    {
        while (true)
        {
            //플레이어 주변에 블럭 생성
            var playerPos = _player.transform.position;
            var inRangeBlocks = GetEnsuredBlocksInRange(playerPos, Data.BlockGenerationRange);
            //주변 블럭 중 고정되지 않고 청크가 없는 블럭만 남기고 셔플하기.
            var shuffledUnfixedBlocks = inRangeBlocks.Where(x => x.isFixed == false && x.chunk == null);
            //남은 블럭들을 순회하면서 고정되지 않았다면 청크를 생성하기. 청크 생성시 해당 블럭 주변 블럭은 고정 (자신 포함).
            foreach (var block in shuffledUnfixedBlocks)
            {
                //블럭이 고정되지 않았다면 
                if (block.isFixed == false)
                {
                    //확률에 따라 청크 생성
                    if (Random.Range(0f, 100f) <= Data.ChunkChance)
                    {
                        //블럭 주변 블럭들을 고정시키기
                        var unfixedBlocks = GetExistingBlocksInRange(block.worldPosition, Data.ChunkGuaranteedSpace)
                            .Where(x => x.isFixed == false);
                        unfixedBlocks.ForEach(x => x.isFixed = true);
                        
                        //청크 생성될 블럭에는 장식품 생성할 수 없게
                        block.isPropFixed = true;
                        
                        //청크 생성
                        SpawnRandomChunk(block.blockCode);
                    }
                    else //확률에 따라 청크를 생성하지 않으면 고정
                    {
                        block.isFixed = true;
                        block.isPropFixed = false;
                    }
                }
            }
            
            //장식물 생성
            var propUnfixedBlocks = inRangeBlocks.Where(x => x.isPropFixed == false && x.chunk == null);
            foreach (var propBlock in propUnfixedBlocks)
            {
                if (Random.Range(0f, 100f) <= Data.PropChance)
                {
                    SpawnRandomChunk(propBlock.blockCode, true);
                }
                
                propBlock.isPropFixed = true;
            }
            
            //블럭 삭제 범위 밖의 블럭들 삭제
            var inSafeRangeBlocks = GetExistingBlocksInRange(playerPos, _data.BlockRemovalRange);
            var blocksToBeRemoved = AllBlocks.Values.Except(inSafeRangeBlocks).ToList();
            for (int i = blocksToBeRemoved.Count - 1; i > 0; i--)
            {
             var block = blocksToBeRemoved[i];
             if (block == null) continue;
             RemoveBlock(block);
            }
            
            //데이터의 틱 주기만큼 기다리기
            var t = _data.TickInterval <= 0 ? 0.1f : _data.TickInterval;
            yield return new WaitForSeconds(_data.TickInterval);
        }
    }

    public void Clear()
    {
        AllBlocks.Clear();
        _data = null;
        _player = null;
        
        //반복 코루틴 중지 (플레이어 주변 블럭 생성하는 코루틴)
        if (tickBlockGenRoutine != null) CoroutineHandler.StopCoroutine(tickBlockGenRoutine);
    }
    
    private void RemoveBlock(Block block_)
    {
        if (AllBlocks.ContainsValue(block_))
        {
            //청크 삭제
            if (block_.chunk != null)
            {
                //인간 유닛 삭제한 것 유닛 매니저에 알리기
                var unitsInChunk = block_.chunk.GetComponentsInChildren<Unit>(includeInactive: true);
                foreach (var unit in unitsInChunk)
                {
                    if (unit.CurrentFaction != Faction.Human)
                    {
                        unit.transform.SetParent(null);
                        continue;
                    }
                    if (UnitManager.UnitsInScene.Contains(unit))
                    {
                        UnitManager.UnitsInScene.Remove(unit);
                    }
                }
                
                //청크 내 보상들 리스트에서 삭제
                var itemsInChunk = block_.chunk.GetComponentsInChildren<ItemBase>(includeInactive: true);
                foreach (var item in itemsInChunk)
                {
                    if (_allItemsInScene.Contains(item))
                    {
                        AllItemsInScene.Remove(item);
                    }
                }
                
                //실제 오브젝트 삭제
                block_.chunk.SetActive(false);
                Object.Destroy(block_.chunk);
            }
            
            //장식품 삭제
            if (block_.prop != null)
            {
                block_.prop.SetActive(false);
                Object.Destroy(block_.prop);
            }
            
            AllBlocks.Remove(block_.blockCode);
        }
    }
    
    private void SpawnRandomChunk(Vector2Int blockCode_, bool isProp_ = false)
    {
        var infoList_ = isProp_ ? Data.PropInfos : Data.ChunkInfos;
        var chunkPrefab = GetRandomChunkInfo(infoList_).chunkPrefab;
        var position = GetWorldPositionByCode(blockCode_) + GetRandomOffsetInBlock(0.8f);
        var chunk = InstantiateGameObject(chunkPrefab, position);
        
        //청크 차일드 오브젝트들 중 Unit을 모두 가져와서 UnitManager에 등록
        var unitsInChunk = chunk.GetComponentsInChildren<Unit>();
        UnitManager.UnitsInScene.AddRange(unitsInChunk);
        
        //유닛들 한 그룹으로 묶기
        UnitGroup newGroup = new UnitGroup();
        newGroup.MemeberUnits.AddRange(unitsInChunk);
        foreach (var unit in unitsInChunk)
        {
            unit.InitFactionSet(Faction.Human);
            
            unit.Group = newGroup;
            
            //선공격하지 않는 유닛들로 설정.
            unit.IsCampUnit = true;
            
            //units의 유닛들 onUnitSpawn 이벤트 발생시키기.
            ManagerRoot.Event.onUnitSpawn?.Invoke(unit);
            
            //기본 유닛인 경우 축복 타임라인 적용
            if ((UnitType)unit.UnitType is UnitType.SwordMan or UnitType.ArcherMan or UnitType.Assassin)
            {
                var blessPercent = (ManagerRoot.Wave.GetCurrentBlessing() - 1) * 100;
                var blessModHp = new StatModifier(StatType.MaxHp, "Blessing_Hp", blessPercent, StatModifierType.FinalPercentage, false);
                var blessModDmg = new StatModifier(StatType.AttackDamage, "Blessing_Damage", blessPercent, StatModifierType.FinalPercentage, false);
                ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(unit, blessModHp);
                ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(unit, blessModDmg);
            }
        }
        
        //장식품이 아닌 경우 블럭에 청크 정보 저장
        if (!isProp_) AllBlocks[blockCode_].chunk = chunk;
        else AllBlocks[blockCode_].prop = chunk;
    }
    
    //데이터의 청크 리스트중 weighted random 방식으로 청크 하나를 반환
    public ChunkInfo GetRandomChunkInfo(List<ChunkInfo> infoList_)
    {
        if (infoList_.Count == 0)
        {
            Debug.LogWarning( infoList_+ " ChunkInfos is empty.");
            return default;
        }

        //시간 조건을 만족시킨 청크들만 남기기
        var currentTimeInSeconds = ManagerRoot.Wave.CurrentTime;
        int currentTimeInMinutes = Mathf.FloorToInt(currentTimeInSeconds / 60f);
        var availableChunks = infoList_.Where(x => x.MinTimeCondition <= currentTimeInMinutes && x.MaxTimeCondition >= currentTimeInMinutes).ToList();
        
        //Weighted Random에 따라 청크 하나 반환
        float totalWeight = availableChunks.Sum(x => x.chunkWeightedChance);
        float random = Random.Range(0f, totalWeight);
        float weightSum = 0f;
        foreach (var chunk in availableChunks)
        {
            weightSum += chunk.chunkWeightedChance;
            if (random <= weightSum)
            {
                return chunk;
            }
        }

        Debug.LogError("Something went wrong with weighted random logic.");
        return Data.ChunkInfos[0];
    }
    
    //원 범위 내 모든 블럭들을 반환. 아직 생성되지 않았다면 생성 후 반환
    public List<Block> GetEnsuredBlocksInRange(Vector2 center_, float range_)
    {
        GenerateBlocksInRange(center_, range_);
        return GetExistingBlocksInRange(center_, range_);
    }
    
    //원 범위 내 모든 블럭들을 반환
    private List<Block> GetExistingBlocksInRange(Vector2 center_, float range_)
    {
        List<Block> blocks = new List<Block>();
        List<Vector2Int> blockCodes = GetBlockCodesInRange(center_, range_);
        foreach (Vector2Int code in blockCodes)
        {
            if (AllBlocks.ContainsKey(code) == false)
            {
                //Debug.LogWarning("Block not found. Use GenerateBlocksInRange() first.");
                continue;
            }
            blocks.Add(AllBlocks[code]);
        }
        return blocks;
    }
    
    //원 범위 내 블럭들을 생성
    private void GenerateBlocksInRange(Vector2 center_, float range_)
    {
        List<Vector2Int> blockCodes = GetBlockCodesInRange(center_, range_);
        foreach (Vector2Int code in blockCodes)
        {
            if (AllBlocks.ContainsKey(code) == false)
            {
                InstantiateNewBlock(code);
            }
        }
    }
    
    private Block InstantiateNewBlock(Vector2Int blockCode_)
    {
        //센터블럭이 만들어지지 않았는데 다른 블럭을 만들려고 하는지 체크
        if (AllBlocks.ContainsKey(Vector2Int.zero) == false && blockCode_ != Vector2Int.zero)
        {
            Debug.LogWarning("Center block must be instantiated first");
            return null;
        }
        
        //블럭 필드 변수 설정
        Block newBlock = new Block
        {
            blockCode = blockCode_,
            worldPosition = GetWorldPositionByCode(blockCode_),
            isFixed = IsChunkInRange(blockCode_, Data.ChunkGuaranteedSpace), //주변에 청크가 있으면 청크밴 
        };
        
        //블럭 사전에 추가
        AllBlocks.Add(blockCode_, newBlock);
        
        return newBlock;
    }
    
    //원 범위 내에 청크를 가진 블럭이 있는지 확인
    private bool IsChunkInRange(Vector2Int blockCode_, float range_)
    {
        //첫번째 블럭일 경우 false 반환
        if (blockCode_ == Vector2Int.zero)
        {
            return false;
        }
        
        var centerPoint = GetWorldPositionByCode(blockCode_);
        var blockCodes = GetBlockCodesInRange(centerPoint, range_);
        foreach (var code in blockCodes)
        {
            if (AllBlocks.ContainsKey(code) && AllBlocks[code].chunk != null)
            {
                return true;
            }
        }

        return false;
    }
    
    //센터 포지션을 기준으로 원 범위 내의 블럭 코드들을 반환
    private List<Vector2Int> GetBlockCodesInRange(Vector2 center_, float range_)
    {
        //정사각형 범위 내의 모든 블럭코드 구하기.
        List<Vector2Int> blockCodes = new List<Vector2Int>();
        Vector2Int centerBlockCode = GetBlockCode(center_);
        int range = Mathf.CeilToInt(range_ / Data.BlockSize);
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector2Int blockCode = centerBlockCode + new Vector2Int(x, y);
                blockCodes.Add(blockCode);
            }
        }
        
        //정사각형 범위 내의 모든 블럭코드 중 원 범위 내의 블럭코드만 남기기.
        for (int i = blockCodes.Count - 1; i >= 0; i--)
        {
            Vector2Int code = blockCodes[i];
            Vector2 pos = GetWorldPositionByCode(code);
            float dist = Vector2.Distance(center_, pos);
            if (dist > range_)
            {
                blockCodes.RemoveAt(i);
            }
        }
        
        return blockCodes;
    }
    
    //월드 포지션이 속해야하는 블럭의 코드를 반환
    private Vector2Int GetBlockCode(Vector2 position_)
    {
        Vector2Int blockCode = Vector2Int.zero;
        Vector2 offset = position_ - CenterPosition;
        blockCode.x = Mathf.RoundToInt(offset.x / Data.BlockSize);
        blockCode.y = Mathf.RoundToInt(offset.y / Data.BlockSize);
        return blockCode;
    }
    
    /// <summary>
    /// 블럭 내에서 랜덤한 오프셋을 반환
    /// </summary>
    /// <param name="thresholdRatio_">0~1의 값. 0.7일 경우 정사각형 중 가장자리 30% 내에는 결과값이 나올 수 없다. </param>
    /// <returns></returns>
    private Vector2 GetRandomOffsetInBlock(float thresholdRatio_)
    {
        Vector2 offset = Vector2.zero;
        offset.x = Random.Range(-Data.BlockSize / 2f, Data.BlockSize / 2f);
        offset.y = Random.Range(-Data.BlockSize / 2f, Data.BlockSize / 2f);

        thresholdRatio_ = Mathf.Clamp(thresholdRatio_, 0f, 1f);
        return offset * thresholdRatio_;
    }
    
    private Vector2 GetWorldPositionByCode(Vector2Int blockCode_)
    {
        var offset = (Vector2) blockCode_ * Data.BlockSize;
        Vector2 pos = blockCode_ == Vector2Int.zero ? _player.transform.position : CenterPosition + offset;
        return pos;
    }
    
    private GameObject InstantiateGameObject(GameObject prefab_, Vector2 position_)
    {
        var obj = Object.Instantiate(prefab_, position_, Quaternion.identity);
        var allItems = obj.GetComponentsInChildren<ItemBase>(includeInactive: true);
        AllItemsInScene.AddRange(allItems);
        return obj;
    }

    public class Block
    {
        public Vector2Int blockCode;
        public Vector2 worldPosition;

        public bool isFixed = false;
        public bool isPropFixed = false;
        public GameObject chunk;
        public GameObject prop;
    }
}
