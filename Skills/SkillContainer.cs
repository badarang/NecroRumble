using System.Collections;
using UnityEngine;

public class SkillContainer : MonoBehaviour
{
	private SpellCooldown[] _cooldowns;
	private SkillBase[] _skills;
	
	private void Start()
	{
		_cooldowns = this.gameObject.GetComponentsInChildren<SpellCooldown>();
		_skills = GameManager.Instance.GetPlayer().skills;
		
		Match();
	}
	
	private void Match()
	{
		for(int i = 0; i < 4; i++)
		{
			_cooldowns[i].Skill = _skills[i];
		}
	}
}
