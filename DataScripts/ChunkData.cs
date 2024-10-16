using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ChunkData", menuName = "Data/ChunkData")]
public class ChunkData : ScriptableObject
{
    [SerializeField] private float _blockSize;
    [SerializeField] private float _chunkGuaranteedSpace;
    [SerializeField] private float _startingDeadzoneRange;
    [Tooltip("시작시 무조건 생성되는 청크(보물상자)가 어떤 거리에 생성되어야 하는지")]
    [SerializeField] private float _startingChunkRange;
    [SerializeField] private GameObject _startingChunkPrefab;
    //[SerializeField] private float _blockGenRange;
    //[MinValue("_blockCheckRange")]
    [SerializeField] private float _blockGenerationRange;
    [MinValue("_blockGenerationRange")]
    [SerializeField] private float _blockRemovalRange;
    [SerializeField, SuffixLabel("%", overlay: true), Range(0f,100f)] private float _chunkChance;
    [SerializeField, SuffixLabel("%", overlay: true), Range(0f,100f)] private float _propChance;
    [MinValue(0.1f)]
    [SerializeField] private float _tickInterval;
    [SerializeField] private List<ChunkInfo> _chunkInfos;
    [PropertySpace(SpaceBefore = 25)]
    [SerializeField] private List<ChunkInfo> _propInfos;
    
    public float BlockSize => _blockSize;
    public float ChunkGuaranteedSpace => _chunkGuaranteedSpace;
    public float StartingDeadzoneRange => _startingDeadzoneRange;
    public float StartingChunkRange => _startingChunkRange;
    public GameObject StartingChunkPrefab => _startingChunkPrefab;
    //public float BlockGenRange => _blockGenRange;
    public float BlockGenerationRange => _blockGenerationRange;
    public float BlockRemovalRange => _blockRemovalRange;
    public float ChunkChance => _chunkChance;
    public float TickInterval => _tickInterval;
    public List<ChunkInfo> ChunkInfos => _chunkInfos;
    public List<ChunkInfo> PropInfos => _propInfos;
    public float PropChance => _propChance;
}

[System.Serializable]
public struct ChunkInfo
{
    [AssetsOnly] public GameObject chunkPrefab;
    public float chunkWeightedChance;

    [SuffixLabel("minute", overlay: false), MinMaxSlider(0, 20, showFields: true)]
    [SerializeField] private Vector2Int _minMaxTimeCondition;
    
    public int MinTimeCondition => _minMaxTimeCondition.x;
    public int MaxTimeCondition => _minMaxTimeCondition.y;
    
}
