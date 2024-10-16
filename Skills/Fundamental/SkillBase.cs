using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;

public abstract class SkillBase : MonoBehaviour, ISkill
{
	public uint Id { get; set;}
	public string Name { get; set;}
	
	public virtual float CurrentCooldown{ get; set; }
	public virtual float MaxCooldown { get; protected set; }
	public virtual bool IsCooldown{ get; set; }

	/// <summary>
	/// 현재 업그레이드 단계. 스킬 획득 시 1인 상태로 시작.
	/// </summary>
	public int SkillLevel { get; protected set; } = 1;
	
	public void UpgradeSkill()
	{
		if (GameData.MaxSkillLevel <= SkillLevel)
		{
			Debug.LogWarning("레벨 최대치인 스킬을 레벨업하려고 시도함.");
			return;
		}
		SkillLevel++;
		
		OnSkillUpgrade();
	}
	
	protected T LoadData<T>() where T : SkillData
	{
		return ManagerRoot.Resource.Load<T>("Data/Skill/" + typeof(T).Name);
	}

	public abstract void OnSkillAttained();
	
	/// <summary>
	/// 스킬 업그레이드 완료 후 호출됨. switch문 활용해 스킬 레벨별 효과 구현 필요.
	/// </summary>
	public abstract void OnSkillUpgrade();
	
	/// <summary>
	/// 전투(웨이브) 시작 시 호출
	/// </summary>
	public abstract void OnBattleStart(); //TODO: 전투 시작시 호출하기
	
	/// <summary>
	/// 전투(웨이브) 종료 시 호출
	/// </summary>
	public abstract void OnBattleEnd();
}
