using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class SteamClientHandler : MonoBehaviour
{
	private void Awake()
	{
		try
		{
			Steamworks.SteamClient.Init(2735950, true);
		}
		catch (System.Exception e)
		{
			// Something went wrong! Steam is closed?
			Debug.LogError("steam init failed");
		}
		
		DontDestroyOnLoad(this.gameObject);
	}

	private void Start()
	{
		Steamworks.SteamUserStats.OnAchievementProgress -= AchievementChanged;
		Steamworks.SteamUserStats.OnAchievementProgress += AchievementChanged;
	}

	private void Update()
	{
		if (Steamworks.SteamClient.IsValid)
		{
			Steamworks.SteamClient.RunCallbacks();
		}
	}
	
	public void AchievementChanged(Steamworks.Data.Achievement achievement_, int currentProgress_, int maxProgress_)
	{
		if (currentProgress_ < maxProgress_) return;
		if(!achievement_.State)
		{
			achievement_.Trigger();
		}
	}
	
	private void OnDisable()
	{
		Steamworks.SteamClient.Shutdown();
	}

	private void OnApplicationQuit()
	{
		Steamworks.SteamClient.Shutdown();
	}
}
