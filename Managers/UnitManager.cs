using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LOONACIA.Unity.Managers;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
//using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

// todo 따로 파일화
public class UnitExp 
{
	// todo: 어떤 강화가 적용됐는지는 나중에 추가
	public int level;  // 강화 횟수
	public int curExp; // 현 레벨 현재 경험치
	public int maxExp; // 현 레벨 최대 경험치

	public UnitExp(int level_, int curExp_, int maxExp_)
	{
		level = level_;
		curExp = curExp_;
		maxExp = maxExp_;
	}
}

public class UnitManager
{
	public static List<Unit> UnitsInScene = new();

	public static Dictionary<UnitType, GameObject> UnitDictionary = new();
	
	public const int UNIT_MAX_LEVEL = 11;
	public const int MAX_EXP_INCREASE = 1;
	private const int STARTING_MAX_EXP = 4;
	
	private Transform _unitTransform;

	private List<PairUnitData> _allUnitData = new ();
	//private Dictionary<uint, Type> _unitCache = new();

	public static Dictionary<UnitType, UnitExp> UnitExpDic = new Dictionary<UnitType, UnitExp>(); // 일단 네개(창병, 검병, 궁병, 암살자) (기마병)

	public List<PairUnitData> AllUnitData
	{
		get => _allUnitData;
	}

	public bool IsMergeEnd { get; set; } = false;

	private uint _uniqueId = 0;

	#region 캐릭터 변경
	
	private CharacterType _curCharacter;
	public CharacterType CurCharacter
	{
		get => _curCharacter;
		set => _curCharacter = value;
	}
	
	public enum CharacterType
	{
		Necromancer,
		CutePlayer,
	}
	
	#endregion
	
	public void Init()
	{
		GetAllUnitTypesAndData();
		LoadUnits();

		MakeUnitTransform();

		InitUnitExp();
	}
	
	public void Clear()
	{
		UnitsInScene.Clear();
		_allUnitData.Clear();
		UnitExpDic.Clear();
		UnitDictionary.Clear();
		Unit.ClearStaticVariables();
	}

	void InitUnitExp()
	{
		int curExp = 0;
		int level  = 1;
		int maxExp = STARTING_MAX_EXP; //시작 필요 경험치

		UnitExpDic.TryAdd( UnitType.SpearMan,  new UnitExp( level, curExp, 3) );
		UnitExpDic.TryAdd( UnitType.SwordMan,  new UnitExp( level, curExp, maxExp) );
		UnitExpDic.TryAdd( UnitType.ArcherMan, new UnitExp( level, curExp, maxExp) );
		UnitExpDic.TryAdd( UnitType.Assassin,  new UnitExp( level, curExp, maxExp) );
		UnitExpDic.TryAdd( UnitType.HorseMan,  new UnitExp( level, curExp, maxExp) );
	}

	private void GetAllUnitTypesAndData()
	{
		string resourcesPath = "Assets/Resources/Data/Unit";
		PairUnitData[] unitDataArray = LoadAllScriptableObjects<PairUnitData>(resourcesPath);

		_allUnitData.AddRange(unitDataArray);

		// foreach (WholeUnitData _wholeUnitData in _allUnitData)
		// {
		//     BaseFactionUnitInfo BaseInfo = _wholeUnitData.GetInfo(Faction.Undead);
		//     Debug.Log("BaseMaxHp: " + BaseInfo.BaseMaxHp);
		//     Debug.Log("BaseMoveSpeed: " + BaseInfo.BaseMoveSpeed);
		//     Debug.Log("BaseAttackRange: " + BaseInfo.BaseAttackRange);
		//     Debug.Log("BaseAttackPerSec: " + BaseInfo.BaseAttackPerSec);
		//     Debug.Log("BaseAttackDamage: " + BaseInfo.BaseAttackDamage);
		//     Debug.Log("BaseMaxAggroNum: " + BaseInfo.BaseMaxAggroNum);
		//     foreach (UnitData unitData in _wholeUnitData.GetUnitData())
		//     {
		//         Debug.Log("UnitType: " + unitData.UnitType);
		//         Debug.Log("UnitName: " + unitData.UnitName);
		//         Debug.Log("UnitDescription: " + unitData.UnitDescription);
		//     }
		// }
	}

	private T[] LoadAllScriptableObjects<T>(string path) where T : ScriptableObject
	{
		return Resources.LoadAll<T>(path);
	}

	// private T[] LoadAllScriptableObjects<T>(string path) where T : UnityEngine.ScriptableObject
	// {
	//     string[] assetGuids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { path });

	//     T[] objects = new T[assetGuids.Length];

	//     for (int i = 0; i < assetGuids.Length; i++)
	//     {
	//         string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
	//         objects[i] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
	//     }

	//     return objects;
	// }
	
	private void LoadUnits()
	{
		var array = Resources.LoadAll<GameObject>("Prefabs/Unit/");
		foreach (var prefab in array)
		{
			if (prefab.name.EndsWith("Unit"))
			{
				var str = prefab.name.Remove(prefab.name.Length - 4);
				var key = Enum.Parse<UnitType>(str);
				var value = prefab;

				UnitDictionary.TryAdd(key, value);
			}
		}
	}

	private void MakeUnitTransform()
	{
		GameObject obj = new GameObject("Unit");
		obj.transform.position = Vector3.zero;
		_unitTransform = obj.transform;
	}
	
	public void CreateUnit(UnitType unitType, Vector3 position, Faction faction, bool isMagicCircle = true, UndeadForm undeadForm_ = UndeadForm.Normal)
	{
		if (isMagicCircle)
		{
			GameObject magicCircle = ManagerRoot.Resource.Instantiate("Effects/MagicCircle");
			magicCircle.transform.position = position;

			SpriteRenderer spr = magicCircle.GetComponentInChildren<SpriteRenderer>();
			if (spr != null && spr.material.HasProperty("_HsvShift"))
			{
				spr.material.SetFloat("_HsvShift", faction == Faction.Human ? 0f : 180f);

			}
			CoroutineHandler.StartCoroutine(DelayedUnitCreation(unitType, position, faction, undeadForm_, magicCircle));
		}
		else
		{
			// InstantiateUnit(unitType, position, faction, undeadForm_);
		}
	}

	public void InstantiateUnit(UnitType unitType_, Vector3 position_, Faction faction_, out Unit unit_)
	{
		if (unitType_ == UnitType.None) //UnitType.None이면 null 반환
		{
			unit_ = null;
			return;
		}
		
		GameObject unit = ManagerRoot.Resource.Instantiate(UnitDictionary[unitType_], usePool: false);
		unit.transform.position = position_;
		unit.transform.SetParent(_unitTransform);
		var unitScript = SetUnit(unit, faction_);
		unit_ = unitScript;
		UnitsInScene.Add(unitScript);
		unit.gameObject.SetActive(true);
		ManagerRoot.Event.onUnitSpawn?.Invoke(unitScript);
	}

	public static void UpdateUnitsInScene()
	{
		UnitsInScene.Clear();
		Unit[] units = GameObject.FindObjectsOfType<Unit>();
		foreach (Unit unit in units)
		{
			UnitsInScene.Add(unit);
		}
	}
	
	private IEnumerator DelayedUnitCreation(UnitType unitType, Vector3 position, Faction faction, UndeadForm undeadForm_, GameObject magicCircle_)
	{
		yield return new WaitForSeconds(Statics.MagicCircleDuration);
		ManagerRoot.Resource.Release(magicCircle_);
		// InstantiateUnit(unitType, position, faction, undeadForm_);
	}

	private Unit SetUnit(GameObject unit_, Faction faction_)
	{
		var unitScript = unit_.GetComponent<Unit>();
		unitScript.Id = _uniqueId++;
		unitScript.InitFactionSet(faction_);
		return unitScript;
	}
	
	public static void DestroyUnit(uint idx_)
	{
		for (int i = UnitsInScene.Count - 1; i >= 0; i--)
		{
			if (UnitsInScene[i].Id == idx_)
			{
				ManagerRoot.Resource.Release(UnitsInScene[i].gameObject);
				UnitsInScene.Remove(UnitsInScene[i]);
				break;
			}
		}
	}
	
	public static void DestroyUnit(Unit unit_)
	{
		for (int i = UnitsInScene.Count - 1; i >= 0; i--)
		{
			if (UnitsInScene[i] == unit_)
			{
				ManagerRoot.Resource.Release(UnitsInScene[i].gameObject);
				UnitsInScene.Remove(UnitsInScene[i]);
				break;
			}
		}
	}
	
	public List<Unit> GetAllAliveUndeadUnits()
	{
		List<Unit> undeadUnits = new List<Unit>();
		for (int i = UnitsInScene.Count - 1; i >= 0; i--)
		{
			if (UnitsInScene[i].CurrentFaction != Faction.Undead) continue;
			if (UnitsInScene[i].IsDead) continue;
			undeadUnits.Add(UnitsInScene[i]);
		}
		return undeadUnits;
	}
	
	public List<Unit> GetAllAliveHumanUnits()
	{
		List<Unit> humanUnits = new List<Unit>();
		for (int i = UnitsInScene.Count - 1; i >= 0; i--)
		{
			if (UnitsInScene[i].CurrentFaction != Faction.Human) continue;
			if (UnitsInScene[i].IsDead) continue;
			humanUnits.Add(UnitsInScene[i]);
		}
		return humanUnits;
	}
	
	public List<Unit> GetSpecificUnitTypeAliveUnits(UnitType unitType_, Faction faction_)
	{
		List<Unit> specificUnits = new List<Unit>();
		for (int i = UnitsInScene.Count - 1; i >= 0; i--)
		{
			if (UnitsInScene[i].UnitType != (int)unitType_) continue;
			if (UnitsInScene[i].IsDead) continue;
			if (UnitsInScene[i].CurrentFaction != faction_) continue;
			specificUnits.Add(UnitsInScene[i]);
		}
		return specificUnits;
	}
	
	public void DestroyUndeadUnit()
	{
		for (int i = UnitsInScene.Count - 1; i >= 0; i--)
		{
			if (UnitsInScene[i].CurrentFaction != Faction.Undead) continue;

			ManagerRoot.Resource.Release(UnitsInScene[i].gameObject);
			UnitsInScene.Remove(UnitsInScene[i]);
		}
	}

	public void DestroyHumanUnit()
	{
		for (int i = UnitsInScene.Count - 1; i >= 0; i--)
		{
			if (UnitsInScene[i].CurrentFaction != Faction.Human) continue;

			ManagerRoot.Resource.Release(UnitsInScene[i].gameObject);
			UnitsInScene.Remove(UnitsInScene[i]);
		}
	}

	public void KillUndeadUnit(bool isGameEnd = false)
	{
		for (int i = UnitsInScene.Count - 1; i >= 0; i--)
		{
			if (UnitsInScene[i].CurrentFaction != Faction.Undead) continue;
			UnitsInScene[i].KillSelf();
			//if (isGameEnd)
			//{
			//    //TODO: 암흑의 정수 흡수하는 코드 여기에 넣으면댐
			//    Debug.Log("암흑의 정수 흡수");
			//}
		}
	}
	
	public void KillHumanUnit()
	{
		for (int i = UnitsInScene.Count - 1; i >= 0; i--)
		{
			if (UnitsInScene[i].CurrentFaction != Faction.Human) continue;
			UnitsInScene[i].KillSelf();
		}
	}
	
    public List<Unit> GetUnitsInCircle(Vector3 center_, float radius_, Faction factionToFind_)
    {
        List<Unit> units = new List<Unit>();
        foreach (var unit in UnitsInScene)
        {
            if (Vector3.Distance(unit.transform.position, center_) <= radius_)
            {
	            if (unit.CurrentFaction == factionToFind_)
	            {
		            units.Add(unit);
	            }
            }
        }
        return units;
    }
	
	public IEnumerator DestroyAllUnit()
	{
		KillHumanUnit();
		yield return new WaitForSeconds(2f);
		KillUndeadUnit(true);
	}
	

	public void DestroyAllCorpse(bool isNextRound = false)
	{
		for (int i = UnitsInScene.Count - 1; i >= 0; i--)
		{
			if (UnitsInScene[i].IsDead)
			{
				if (isNextRound) //애니메이션 나오고 삭제
				{
					UnitsInScene[i].StartCoroutine(UnitsInScene[i].StartEffectAndAbsorb());
				}
				else //그냥 삭제
				{
					ManagerRoot.Resource.Release(UnitsInScene[i].gameObject);
					UnitsInScene.Remove(UnitsInScene[i]);
				}
			}
		}
	}
	
	public void DestroyCorpse(Unit corpse_)
	{
		if ( corpse_ == null ){ return; }

		if( UnitsInScene.Contains(corpse_) )
		{
			UnitsInScene.Remove(corpse_);
			ManagerRoot.Resource.Release(corpse_.gameObject);
			GameObject.Destroy(corpse_.gameObject);
			
		}
	}
	public int CountEnemyUnit(bool isCountCorpse)
	{
		int count = 0;
		foreach (var unit in UnitsInScene)
		{
			if (unit.CurrentFaction == Faction.Human)
			{
				if (isCountCorpse)
				{
					count++;
				}
				else
				{
					if (!unit.IsDead) count++;
				}
			}
		}
		return count;
	}
	public int CountUndeadUnit()
	{
		int count = 0;
		foreach (var unit in UnitsInScene)
		{
			if (!unit.IsDead && unit.CurrentFaction == Faction.Undead) count++;
		}

		return count;
	}



	public IEnumerator CheckAllSkeletonsInAllUnitsListAndMakeEnhancedSkeleton()
	{
		List<Unit> listDread = new();
		List<Unit> listDeath = new();
		var waitForAnim = new WaitForSeconds(1);

		while (true)
		{
			foreach (var unit in UnitsInScene)
			{
				if (unit.UnitType == (int)UnitType.SwordMan && unit.CurrentFaction == Faction.Undead && !unit.IsDead)
				{
					if (!listDread.Contains(unit))
					{
						listDread.Add(unit);
					}
				}
				else if (unit.UnitType == (int)UnitType.DreadKnight)
				{
					if (!listDeath.Contains(unit))
					{
						listDeath.Add(unit);
					}
				}
			}

			if (listDread.Count > 3)
			{
				for (int i = 0; i < 4; i++)
				{
					UnitsInScene.Remove(listDread[i]);
					listDread[i].KillSelf();
				}
				CreateUnit(UnitType.DreadKnight,
					GameManager.Instance.GetPlayer().transform.position +
					new Vector3(Random.Range(-2, 2), Random.Range(-2, 2), 0), Faction.Undead, false);
				yield return waitForAnim;
			}
			else if (listDeath.Count > 3)
			{
				for (int i = 0; i < 4; i++)
				{
					UnitsInScene.Remove(listDeath[i]);
					listDeath[i].KillSelf();
				}

				CreateUnit(UnitType.DeathKnight,
					GameManager.Instance.GetPlayer().transform.position +
					new Vector3(Random.Range(-2, 2), Random.Range(-2, 2), 0), Faction.Undead, false);
				yield return waitForAnim;
			}
			else
			{
				IsMergeEnd = true;
				break;
			}

			listDeath.Clear();
			listDread.Clear();
		}

	}

	
}
