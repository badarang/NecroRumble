using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Wave/" + nameof(WaveData), fileName = nameof(WaveData))]
public class WaveData : ScriptableObject
{
	[TitleGroup("Settings")]
	[TabGroup("Settings/tabs", "Probability", SdfIconType.Dice3Fill, TextColor = "Orange", TabLayouting = TabLayouting.Shrink)]
	[SerializeField][Range(0, 100)] private float _swordMan, _archer, _assassin;


	[TabGroup("Settings/tabs", "CoolDown", SdfIconType.HourglassSplit, TextColor = "Blue", TabLayouting = TabLayouting.Shrink)]
	[MinValue(0)]
	[SerializeField] private float _standard, _min, _max;
	
	[TabGroup("Settings/tabs", "CoolDown")]
	[Range(0, 0.4f)][SerializeField] private float _population;


	[TabGroup("Settings/tabs", "Density", SdfIconType.Joystick, TextColor = "Red", TabLayouting = TabLayouting.Shrink)]
	[TabGroup("Settings/tabs/Density/SubTabGroup", "Sparse")]
	[SerializeField] private float _sparseMultiplier;
	[TabGroup("Settings/tabs/Density/SubTabGroup", "Sparse")]
	[SerializeField, SuffixLabel(label: "min", overlay: true)] private float _sparseDuration;
	
	[TabGroup("Settings/tabs/Density/SubTabGroup", "Normal")]
	[SerializeField, SuffixLabel(label: "min", overlay: true)] private float _normalDuration;

	[TabGroup("Settings/tabs/Density/SubTabGroup", "Dense")] 
	[SerializeField] private float _denseMultiplier;
	[TabGroup("Settings/tabs/Density/SubTabGroup", "Dense")]
	[SerializeField, SuffixLabel(label: "min", overlay: true)] private float _denseDuration;


	[TabGroup("Settings/tabs", "Formation", SdfIconType.Compass, TextColor = "Green", TabLayouting = TabLayouting.Shrink)]
	[TabGroup("Settings/tabs/Formation/SubTabGroup", "General")]
	[SerializeField] private float _distance;

	[TabGroup("Settings/tabs/Formation/SubTabGroup", "DoubleSide")]
	[SerializeField] private int _doubleThreshold;
	
	[TabGroup("Settings/tabs/Formation/SubTabGroup", "DoubleSide")]
	[Range(0, 100)]
	[SerializeField] private float _doubleChance;
	
	[TabGroup("Settings/tabs/Formation/SubTabGroup", "DoubleSide")]
	[SerializeField] private List<StatModifier> _doubleDisadvantage;


	[TabGroup("Settings/tabs/Formation/SubTabGroup", "Encircle")]
	[SerializeField] private int _encircleThreshold;
	
	[TabGroup("Settings/tabs/Formation/SubTabGroup", "Encircle")]
	[Range(0, 100)]
	[SerializeField] private float _encircleChance;
	
	[TabGroup("Settings/tabs/Formation/SubTabGroup", "Encircle")]
	[SerializeField] private List<StatModifier> _encircleDisadvantage;

	// [TitleGroup("Graph 자리만, 지금은 눌러도 뭐 안됨")]
	// [Space(3)]
	// [SerializeField] private AnimationCurve _graph;

	[TitleGroup("Timelines")]
	[HorizontalGroup("Timelines/Split", Width = 0.4f)]
	[SerializeField] public List<WaveTimelineNode> WaveTimeline;
	
	[HorizontalGroup("Timelines/Split")]
	[TabGroup("Timelines/Split/other", "Override", TabLayouting = TabLayouting.Shrink)]
	[SerializeField] public List<OverridingTimelineNode> OverridingTimeline;

	[TabGroup("Timelines/Split/other", "Blessing", TabLayouting = TabLayouting.Shrink)]
	[SerializeField] public List<BlessingTimelineNode> BlessingTimeline;

	#region Getter
	public float SwordManWeightedChance => _swordMan;
	public float ArcherWeightedChance => _archer;
	public float AssassinWeightedChance => _assassin;
	public float StandardWaveCoolDown => _standard;
	public float MinWaveCoolDown => _min;
	public float MaxWaveCoolDown => _max;
	public float WavePopulationGravity => _population;
	public float SparsePopulationMultiplier => _sparseMultiplier;
	public float SparseDuration => _sparseDuration;
	public float NormalDuration => _normalDuration;
	public float DensePopulationMultiplier => _denseMultiplier;
	public float DenseDuration => _denseDuration;
	public float Distance => _distance;
	public int DoublePopulationThreshold => _doubleThreshold;
	public float DoubleChance => _doubleChance;
	public List<StatModifier> DoubleDisadvantage => _doubleDisadvantage;
	public int EncirclePopulationThreshold => _encircleThreshold;
	public float EncircleChance => _encircleChance;
	public List<StatModifier> EncircleDisadvantage => _encircleDisadvantage;
	// public AnimationCurve Graph => _graph;
#endregion
}

[Serializable]
public class WaveFormula
{
	[SerializeField]
	public UnitType[] Types = new UnitType[4];
}

[Serializable]
public struct WaveTimelineNode
{
	[SuffixLabel(label: "minute", overlay: true)]
	[SerializeField] private float _point;
	[SuffixLabel(label: "unit", overlay: true)]
	[SerializeField] private float _value;

	#region Getter
	public float Point => _point;
	public float Value => _value;
	#endregion
}

[Serializable]
public struct BlessingTimelineNode
{
	[SuffixLabel(label: "minute", overlay: true)]
	[SerializeField] private float _point;
	[SuffixLabel(label: "ratio", overlay: true)]
	[SerializeField] private float _value;

#region 
	public float Point => _point;
	public float Value => _value;
#endregion
}

[Serializable]
public class OverridingTimelineNode
{
	[SerializeField] private float _point;
	[SerializeField] private float _duration;
	[SerializeField] private WaveFormula _formula;
	[SerializeField] private float _populationMultiplier;
	[SerializeField] private WaveFormation _formation;
	[SerializeField] private List<StatModifier> _statModifier;
	[SerializeField] private UnitType _uniqueUnit;

	#region Getter
	public float Point => _point;
	public float Duration => _duration;
	public WaveFormula WaveFormula => _formula;
	public float PopulationMultiplier => _populationMultiplier;
	public WaveFormation Formation => _formation;
	public List<StatModifier> StatModifier => _statModifier;
	public UnitType Unit => _uniqueUnit;
	#endregion
}

[Serializable]
public enum Density
{
	Sparse,
	Normal,
	Dense,
}

[Serializable]
public enum WaveFormation
{
	Random,
	DoubleSide,
	Encircle,
}