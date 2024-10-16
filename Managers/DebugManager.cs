using System.Collections;
using System.Collections.Generic;
using Lofelt.NiceVibrations;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
#if UNITY_EDITOR


	public static DebugManager Instance;

	private Player _player;
	private void Awake()
	{
		#region Singleton
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
		#endregion
		
		_player = FindObjectOfType<Player>();
	}



	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			//토글 갓모드
			_player.IsGodMode = !_player.IsGodMode;
			var onOrOff = _player.IsGodMode ? "ON" : "OFF";
			Debug.Log("갓모드 " + onOrOff);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			Time.timeScale = 1;
			Debug.Log($"current timescale: {Time.timeScale}");
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			if (Time.timeScale < .05f) Time.timeScale = .5f;
			else if (Time.timeScale * 2 <= 100) Time.timeScale *= 2;
			
			Debug.Log($"current timescale: {Time.timeScale}");
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			if (Time.timeScale < .55f) Time.timeScale = 0;
			else Time.timeScale /= 2;
			Debug.Log($"current timescale: {Time.timeScale}");
		}

		// -5 위치에 포션 생성 - 위치는 맘대로, 포션생성은 사용
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			Debug.Log("디버그: 포션 생성");

			//Create Healing Potion
			GameObject healingPotion = ManagerRoot.Resource.Instantiate("Items/HealingPotion");
			healingPotion.transform.position = Vector3.zero + Vector3.left * 5f;
		}

		if (Input.GetKeyDown(KeyCode.F3))
		{
			Debug.Log("Key F3 down");
			if (GameManager.Instance.TimeFlow)
			{
				GameManager.Instance.TimeFlow = false;
			}
			else
			{
				GameManager.Instance.TimeFlow = true;
			}
		}
		if (Input.GetKeyDown(KeyCode.F4))
		{
			Time.timeScale = 0f;
		}
		if (Input.GetKeyDown(KeyCode.F11))
		{
			ManagerRoot.Wave.SetCurrentTime(60f * 19.9f);
		}
		if (Input.GetKeyDown(KeyCode.F12))
		{
			GameManager.Instance.MaxUndeadNum = 10;
		}
	}
#endif
}
