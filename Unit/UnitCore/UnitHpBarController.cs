using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity;
using UnityEngine;
using UnityEngine.UI;

public class UnitHpBarController : MonoBehaviour
{
	public Color HumanHPBarColor = Color.red;
	public Color UndeadHPBarColor = Color.green;
	
	private Unit _unit;
	private Transform _hpbarContainer;
	private Transform _hpBar;
	private Image _hpBarImage;
	private Transform _hpBarBackground;
	private float _hpBarDisplayTime = 0f;

	private bool _prevDisplayHPBar;


	void Start()
	{
		_unit = GetComponent<Unit>();

		_hpbarContainer = gameObject.FindChild("HpBarContainer").transform;
		_hpBar = _hpbarContainer.Find("HpBarValue");

		_hpBarImage = _hpBar.GetComponent<Image>();
		_hpBarBackground = _hpbarContainer.Find("HpBarBackground");
		UpdateHpBar();
		_prevDisplayHPBar = GameUIManager.DisplayHPBar;
		_hpbarContainer.gameObject.SetActive(true);
	}

	void Update()
	{
		// if(_prevDisplayHPBar != GameUIManager.DisplayHPBar){
		// 	_prevDisplayHPBar = GameUIManager.DisplayHPBar;
		// 	if(GameUIManager.DisplayHPBar){
		// 		UpdateHpBar();
		// 	}
		// 	else{
		// 		_hpbarContainer.gameObject.SetActive(false);
		// 	}
		// }
		// if(!GameUIManager.DisplayHPBar){
		// 	if (_hpBarDisplayTime > 0f)
		// 	{
		// 		_hpBarDisplayTime -= Time.deltaTime;
		// 		if (_hpBarDisplayTime <= 0f)
		// 		{
		// 			_hpbarContainer.gameObject.SetActive(false);
		// 		}
		// 	}
		// }
	}
	
	public void UpdateHpBar()
	{
		if (_hpbarContainer == null) return;
		
		_hpBarDisplayTime = 1f;
		_hpbarContainer.gameObject.SetActive(true);
		_hpBar.localScale = new Vector3((float)_unit.CurrentHp / _unit.instanceStats.FinalMaxHp, 1f);
		if(_unit.CurrentFaction == Faction.Human)
			_hpBarImage.color = HumanHPBarColor;
		else if(_unit.CurrentFaction == Faction.Undead)
			_hpBarImage.color = UndeadHPBarColor;
		if(_unit.IsDead)
			_hpbarContainer.gameObject.SetActive(false);
	}
	
	public void TurnOffHpBar()
	{
		if (_hpbarContainer == null) return;
		_hpbarContainer.gameObject.SetActive(false);
	}
}
