using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;

public interface ISkill
{
	/// <summary>
	/// 스킬의 아이디
	/// </summary>
	uint Id { get; set;}

	/// <summary>
	/// 스킬의 이름
	/// </summary>
	string Name { get; set;}


}
