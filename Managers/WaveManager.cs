using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LOONACIA.Unity.Managers;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaveManager
{
	private WaveData _waveData;
	private Queue<WaveTimelineNode> _timelineQue = new();
	private Queue<OverridingTimelineNode> _overrideQue = new();
	private Queue<BlessingTimelineNode> _blessingQue = new();
	
	public Density Density { get; private set; } = Density.Sparse;
	public float CurrentTime { get; private set; } = 0;
	
	private float _currentCooldown = 0;
	private WaveTimelineNode _currentPopulation;
	private OverridingTimelineNode _currentOverride;
	private BlessingTimelineNode _currentBlessing;

	public void Init()
	{
		if (SceneManagerEx.CurrentScene.SceneType is SceneType.Title ) return;
		_waveData = Resources.Load<WaveData>("Data/WaveData");
		
		UpdateDensity();
		SetTimelineQues();
	}

	public void Clear()
	{
		CoroutineHandler.StopAllCoroutines();
		CurrentTime = 0;
	}
	
	public void CreateWave()
	{
		var isOverride = CheckOverride();
		
		SetWaveCooldown(isOverride, out var cooldownPopulationMultiplier);
		
		var formula = SetWaveFormula(isOverride);

		var population = SetWavePopulation(isOverride, cooldownPopulationMultiplier);

		var unitList = SetWaveUnit(isOverride, formula, population);

		var formation = SetFormation(isOverride, unitList.Count);

		var position = GetSpawnPosition(formation, ref unitList);

		var spawnUnits = SpawnWave(isOverride, formation, unitList, position);

		GetCurrentBlessing();

		StatModify(isOverride, spawnUnits, formation);

		StartWaveTimer();

		if (isOverride) _overrideQue.Dequeue();
	}

	private bool CheckOverride()
	{
		bool isOverride = false;
		
		if(_overrideQue.TryPeek(out var node))
		{
			if(node.Point * 60 <= CurrentTime)
			{
				isOverride = true;
				_currentOverride = node;
			}
		}
		return isOverride;
	}

	private void SetWaveCooldown(bool isOverride_, out float populationMultiplier_)
	{
		var randomCooldown = Random.Range(_waveData.MinWaveCoolDown, _waveData.MaxWaveCoolDown);
		_currentCooldown = isOverride_ ? _currentOverride.Duration : randomCooldown;
		
		populationMultiplier_ = Mathf.Lerp(_currentCooldown / _waveData.StandardWaveCoolDown, 1, _waveData.WavePopulationGravity);
	}

	private WaveFormula SetWaveFormula(bool isOverride_)
	{
		return isOverride_ ? _overrideQue.Peek().WaveFormula : GetRandomFormula(); 
	}
	
	private WaveFormula GetRandomFormula()
	{
		WaveFormula formula = new();
		var swordMan = _waveData.SwordManWeightedChance;
		var archer = _waveData.ArcherWeightedChance;
		var assassin = _waveData.AssassinWeightedChance;

		var total = swordMan + archer + assassin;
		
		for(int i = 0; i < formula.Types.Length; i++)
		{
			var randomNumber = Random.Range(0f, total);
			UnitType type = new();
			
			if(randomNumber < swordMan)
			{
				type = UnitType.SwordMan;
			}
			else if(randomNumber < swordMan + archer)
			{
				type = UnitType.ArcherMan;
			}
			else
			{
				type = UnitType.Assassin;
			}
			
			formula.Types[i] = type;
		}

		return formula;
	}

	private int SetWavePopulation(bool isOverride_, float cooldownMultiplier_)
	{
		var standard = GetStandardPopulation();

		standard *= GetDensityPopulationMultiplier();
		standard *= cooldownMultiplier_;
		if(isOverride_)
		{
			standard *= _currentOverride.PopulationMultiplier;
		}
		
		if (standard < 1 && standard > 0) standard = 1;
		return Mathf.RoundToInt(standard);
	}
	
	private float GetStandardPopulation()
	{
		var currentNode = _currentPopulation;
		var currentValue = currentNode.Value;
		
		if(_timelineQue.TryPeek(out var nodeCheck))
		{
			if(nodeCheck.Point * 60 < CurrentTime)
			{
				currentNode = _timelineQue.Dequeue();
				_currentPopulation = currentNode;
			}
			
			if(_timelineQue.TryPeek(out var nextNode))
			{
				currentValue = currentNode.Value + ((CurrentTime - currentNode.Point * 60) * (nextNode.Value - currentNode.Value) / (nextNode.Point * 60 - currentNode.Point * 60));
			}
		}

		return currentValue;
	}

	private List<UnitType> SetWaveUnit(bool isOverride_, WaveFormula formula_, int population_)
	{
		var formula = isOverride_ ? _currentOverride.WaveFormula : formula_;
		var unitList = new List<UnitType>();
		
		for (int i = 0; i < population_; i++)
		{
			unitList.Add(formula.Types[i % formula.Types.Length]);
		}

		if (isOverride_)
		{
			if (_currentOverride.Unit != UnitType.None)
			{
				unitList.Insert(0, _currentOverride.Unit);
			}
		}

		return unitList;
	}

	private WaveFormation SetFormation(bool isOverride_, int population_)
	{
		var formation = isOverride_ ? _currentOverride.Formation : GetRandomFormation(population_);

		return formation;
	}
	
	private WaveFormation GetRandomFormation(int population_)
	{
		var formation = WaveFormation.Random;
		var total = 100;
		
		if(population_ > _waveData.DoublePopulationThreshold && population_ > _waveData.EncirclePopulationThreshold)
		{
			var randomNumber = Random.Range(0, total);
			if(randomNumber < _waveData.DoubleChance)
			{
				formation = WaveFormation.DoubleSide;
			}
			else if(randomNumber < _waveData.DoubleChance + _waveData.EncircleChance)
			{
				formation = WaveFormation.Encircle;
			}
		}
		else if(population_ > _waveData.DoublePopulationThreshold)
		{
			var randomNumber = Random.Range(0, total);
			if(randomNumber < _waveData.DoubleChance)
			{
				formation = WaveFormation.DoubleSide;
			}
		}
		else if(population_ > _waveData.EncirclePopulationThreshold)
		{
			var randomNumber = Random.Range(0, total);
			if(randomNumber < _waveData.EncircleChance)
			{
				formation = WaveFormation.Encircle;
			}
		}

		return formation;
	}

	private Vector2 GetSpawnPosition(WaveFormation formation_, ref List<UnitType> types_)
	{
		var position = new Vector2();
		var randomNumber = Random.Range(0, 360);

		switch (formation_)
		{
			case WaveFormation.Random:
				position = Quaternion.Euler(0, 0, randomNumber) * Vector3.right * _waveData.Distance;
				break;
			case WaveFormation.DoubleSide:
				types_ = types_.OrderBy(x => x, UnitTypeComparer.Default).ToList();
				position = Quaternion.Euler(0, 0, randomNumber) * Vector3.right * _waveData.Distance;
				break;
			case WaveFormation.Encircle:
				var degree = 360 / types_.Count();
				position = new Vector2(randomNumber, degree);
				break;
		}
		
		return position;
	}
	
	private List<Unit> SpawnWave(bool isOverride_, WaveFormation formation_, List<UnitType> types_, Vector2 position_)
	{
		var unitList = new List<Unit>();
		var centerPos = GameManager.Instance.GetPlayer().transform.position;
		var spawnPos = new Vector3();

		switch (formation_)
		{
			case WaveFormation.Random:
				foreach (var type in types_)
				{
					spawnPos = (Vector2)centerPos + position_ + new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f));

					ManagerRoot.Unit.InstantiateUnit(type, spawnPos, Faction.Human, out var unit);
					
					
					unitList.Add(unit);
				}
				break;
			case WaveFormation.DoubleSide:
				for (int i = 0; i < types_.Count; i++)
				{
					if (i < types_.Count / 2)
					{
						spawnPos = (Vector2)centerPos + position_ + new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f));
					}
					else
					{
						spawnPos = (Vector2)centerPos - position_ + new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f));
					}

					ManagerRoot.Unit.InstantiateUnit(types_[i], spawnPos, Faction.Human, out var unit);

					unitList.Add(unit);
				}
				break;
			case WaveFormation.Encircle:

				float startAngle = position_[0];
				float angle = position_[1];
				spawnPos = new Vector2();

				for (int i = 0; i < types_.Count; i++)
				{
					spawnPos = centerPos + Quaternion.Euler(0, 0, startAngle + angle * i) * Vector3.right * _waveData.Distance;

					ManagerRoot.Unit.InstantiateUnit(types_[i], spawnPos, Faction.Human, out var unit);

					unitList.Add(unit);
				}
				break;
		}
		return unitList;
	}
	
	public float GetCurrentBlessing()
	{
		var currentNode = _currentBlessing;

		// if (_blessingQue.Count == 0 && _currentBlessing.Value <= 0)
		// {
		// 	var data = ManagerRoot.Resource.Load<WaveData>("Data/WaveData");
		// 	return data.BlessingTimeline[0].Value;
		// }

		if (_blessingQue.TryPeek(out var node))
		{
			if (node.Point * 60 <= CurrentTime)
			{
				currentNode = _blessingQue.Dequeue();
				_currentBlessing = currentNode;
			}
		}

		var currentValue = currentNode.Value;
		return currentValue;
	}

	private void StatModify(bool isOverride_, List<Unit> units_, WaveFormation formation_)
	{
		List<StatModifier> stats = new();
		int unitCount = isOverride_ ? units_.Count - 1 : units_.Count;
		if (isOverride_ && _currentOverride.Unit == UnitType.None) unitCount = units_.Count;

		switch (formation_)
		{
			case WaveFormation.Random:
				break;
			case WaveFormation.DoubleSide:
				stats.AddRange(_waveData.DoubleDisadvantage);
				break;
			case WaveFormation.Encircle:
				stats.AddRange(_waveData.DoubleDisadvantage);
				break;
		}

		if (isOverride_)
		{
			foreach (var stat in _currentOverride.StatModifier)
			{
				stats.Add(stat);
			}
		}

		for (int i = 0; i < unitCount; i++)
		{
			var unit = units_[i];
			
			foreach (var stat in stats)
			{
				ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(unit, stat);
			}

			var blessingPercent = (GetCurrentBlessing() - 1) * 100;
			
			StatModifier blessHp = new StatModifier(StatType.MaxHp,"Blessing_Hp", blessingPercent, StatModifierType.FinalPercentage, false);
			StatModifier blessAtk = new StatModifier(StatType.AttackDamage, "Blessing_Atk", blessingPercent, StatModifierType.FinalPercentage, false);

			ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(unit, blessHp);
			ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(unit, blessAtk);

			if (CurrentTime >= 600f) //10분 이상일 때
			{
				StatModifier blessMoveSpeed = new StatModifier(StatType.MoveSpeed, "Blessing_MoveSpeed", 20, StatModifierType.Percentage, false);
				ManagerRoot.UnitUpgrade.ApplyUpgradeToSingleUnit(unit, blessMoveSpeed);
				unit.Feedback.SetMoveSpeedEffect(true);
			}
		}
	}


	private IEnumerator UpdateDensityCo()
	{
		Density previousDensity = Density.Sparse;
		WaitForSeconds waitSparse = new WaitForSeconds(_waveData.SparseDuration * 60);
		WaitForSeconds waitNormal = new WaitForSeconds(_waveData.NormalDuration * 60);
		WaitForSeconds waitDense = new WaitForSeconds(_waveData.DenseDuration * 60);
		yield return null;

		while (true)
		{
			switch (Density)
			{
				case Density.Sparse:
					yield return waitSparse;
					previousDensity = Density.Sparse;
					Density = Density.Normal;
					break;
				case Density.Normal:
					yield return waitNormal;
					if (previousDensity == Density.Sparse)
					{
						Density = Density.Dense;
					}
					else
					{
						Density = Density.Sparse;
					}
					previousDensity = Density.Normal;
					break;
				case Density.Dense:
					yield return waitDense;
					previousDensity = Density.Dense;
					Density = Density.Normal;
					break;
			}
		}
	}

	private float GetDensityPopulationMultiplier()
	{
		var multiplier = 1f;
		
		switch (Density)
		{
			case Density.Sparse:
				multiplier = _waveData.SparsePopulationMultiplier;
				break;
			case Density.Normal:
				multiplier = 1;
				break;
			case Density.Dense:
				multiplier = _waveData.DensePopulationMultiplier;
				break;
		}

		return multiplier;
	}

	private void SetTimelineQues()
	{
		_timelineQue.Clear();
		_overrideQue.Clear();
		_blessingQue.Clear();
		
		foreach(var node in _waveData.WaveTimeline)
		{
			_timelineQue.Enqueue(node);
		}
		foreach (var node in _waveData.OverridingTimeline)
		{
			_overrideQue.Enqueue(node);
		}
		foreach(var node in _waveData.BlessingTimeline)
		{
			_blessingQue.Enqueue(node);
		}
	}

	#region Coroutine Handle
	private void UpdateDensity()
	{
		CoroutineHandler.StartCoroutine(UpdateDensityCo());
	}

	public void StartGameTimer()
	{
		CoroutineHandler.StartCoroutine(StartGameTimerCo());
		GameManager.Instance.SetRoundTimer();
	}

	private void StartWaveTimer()
	{
		CoroutineHandler.StartCoroutine(StartWaveTimerCo());
	}
	#endregion
	
	private IEnumerator StartGameTimerCo()
	{
		while(true)
		{
			CurrentTime += Time.deltaTime;
			yield return null;
			
			if(CurrentTime > Statics.GameClearTime * 60)
			{
				GameManager.Instance.ChangeState(GameManager.GameState.GameClear);
				break;
			}
		}
	}
	
	private IEnumerator StartWaveTimerCo()
	{
		while (_currentCooldown > 0)
		{
			_currentCooldown -= Time.deltaTime;
			yield return null;
		}
		CreateWave();
	}
    
	
	#region Debug
	public void SetCurrentTime(float time_){
		CurrentTime = time_;
	}
	#endregion
}
