using System;
using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using Honeti;
using UnityEngine;

public class Settings
{
	#region private variables


	private float _masterVolume;
	private float _bgmVolume;
	private float _sfxVolume;
	
	#endregion
	private string CurrentVersion { get; set; }

	public int CurrentPPU { get; private set; }
	public int CurrentResolutionWidth  { get; private set; }
	public int CurrentResolutionHeight { get; private set; }

	public FullScreenMode CurrentScreenMode{ get; private set; }

	public float CurrentOrthgraphicSize  { get; private set; }
	
	public Language CurrentLanguage { get; set; }
	public enum Language
	{
		EN,
		KO,
		JA
	}
	public Language SavedLanguage { get; private set; }
	
	public float MasterVolume
	{
		get => _masterVolume;
		set
		{
			_masterVolume = value;
			//SaveVolume();
		}
	}
	
	public float BgmVolume
	{
		get => _bgmVolume;
		set
		{
			_bgmVolume = value;
			//SaveVolume();
		}
	}
	
	public float SfxVolume
	{
		get => _sfxVolume;
		set
		{
			_sfxVolume = value;
			//SaveVolume();
		}
	}

	public bool IsTutorialCleared { get; private set; }

	public Action OnLanguageChangedAction;
	

	public void Init()
	{
		Load();
	}
	public void Clear()
	{
		// 게임 종료 시 실행할 코드

		Debug.Log("Settings | Clear ");
		// 예: 파일 저장, 설정 저장, 네트워크 연결 종료 등
//#if !UNITY_EDITOR
		Save();
//#endif

	}


	public void Load()
	{
		Debug.Log("Settings | Load Settings");

		if( IsMatchVersion() ){
			Debug.Log("Settings | Not First Excute");
			LoadOptions();
		}else{
			Debug.Log("Settings | First Excute");
			FirstExcute();
		}
		Print();
	}
	
	public void Save()
	{
		Debug.Log("Settings | Save Settings");
		SaveOptions();
	}

	private bool IsMatchVersion()
	{
		CurrentVersion = PlayerPrefs.GetString("CurrentVersion"); // 데스크탑 값.
		
		Debug.Log($"Settings | IsMatchVersion | CurrentVersion:{CurrentVersion} Application.version:{Application.version}"); 
		
		if(CurrentVersion == Application.version) // 프로그램의 값(빌드셋팅에 있는 값)
		{
			return true;
		}else{
			return false;
		}
	}
	
	private void FirstExcute()
	{
		Debug.Log("Settings | ------- FirstSetting --------");

		PlayerPrefs.SetString("CurrentVersion", Application.version);

		MasterVolume = 0.8f;
		BgmVolume    = 0.2f;
		SfxVolume    = 0.5f;

		CurrentPPU              = 60;
		CurrentResolutionWidth  = 1920;
		CurrentResolutionHeight = 1080;
		CurrentScreenMode       = FullScreenMode.Windowed;

		CurrentOrthgraphicSize  = 540f;

		IsTutorialCleared = false;
		IsTutorialCleared = true;

		SaveOptions();
	}


	private void LoadOptions()	// 여기서 설정 불러옴
	{
		Debug.Log("Settings | ------- Load Options -------");

		MasterVolume = PlayerPrefs.GetFloat("Master");
		BgmVolume    = PlayerPrefs.GetFloat("Bgm");
        Debug.Log($"<color=green>Settings | Load BgmVolume {BgmVolume}</color>");
		SfxVolume    = PlayerPrefs.GetFloat("Sfx");

		CurrentPPU              = PlayerPrefs.GetInt("PPU");
		CurrentResolutionWidth  = PlayerPrefs.GetInt("ResolutionWidth");
		CurrentResolutionHeight = PlayerPrefs.GetInt("ResolutionHeight");
		CurrentScreenMode       = (FullScreenMode)PlayerPrefs.GetInt("ScreenMode");

		CurrentOrthgraphicSize  = PlayerPrefs.GetFloat("OrthgraphicSize");

		CurrentLanguage         = (Language)PlayerPrefs.GetInt("Language");

		IsTutorialCleared = Convert.ToBoolean(PlayerPrefs.GetInt("Tutorial"));
		IsTutorialCleared = true;

	}
	
	public void SaveOptions()  // 각종 설정들 여기서 저장해주면 됨
	{
		Debug.Log("Settings | ------ Save Options -------");

		SaveVolume();
		SaveResolutions();
		SaveLanguage();

		PlayerPrefs.SetInt("Tutorial", Convert.ToInt32(IsTutorialCleared));

		PlayerPrefs.Save();
	}
	
	public void SaveVolume()
	{
		PlayerPrefs.SetFloat("Master", MasterVolume);
		PlayerPrefs.SetFloat("Bgm", BgmVolume);
		PlayerPrefs.SetFloat("Sfx", SfxVolume);

		Debug.Log($"<color=green>Settings | Save BgmVolume {BgmVolume}</color>");
	}

	public void SaveResolutions()
	{
		PlayerPrefs.SetInt("PPU", CurrentPPU);
		PlayerPrefs.SetInt("ResolutionWidth",  CurrentResolutionWidth);
		PlayerPrefs.SetInt("ResolutionHeight", CurrentResolutionHeight);

		PlayerPrefs.SetInt("ScreenMode", (int)CurrentScreenMode);

		PlayerPrefs.SetFloat("OrthgraphicSize", CurrentOrthgraphicSize);
	}

	public void SaveLanguage()
	{
		PlayerPrefs.SetInt("Language", (int)CurrentLanguage);
		CurrentLanguage = (Language)PlayerPrefs.GetInt("Language");
	}

	public void ChangeLanguage(Language language_){
		CurrentLanguage = language_;
		ManagerRoot.I18N.setLanguage(Convert.ToString(CurrentLanguage));
		SaveLanguage();
	}

	public void ClearTutorial()
	{
		IsTutorialCleared = true;
	}
	
	

	public void SetResolution(int width_, int height_)
	{
		switch(width_)
		{
			case 800: CurrentPPU = 30; CurrentOrthgraphicSize = 225f; break;
			case 1200:CurrentPPU = 40; CurrentOrthgraphicSize = 338f; break;
			case 1600:CurrentPPU = 50; CurrentOrthgraphicSize = 450f; break;
			case 1920:CurrentPPU = 60; CurrentOrthgraphicSize = 540f; break;
			default:  CurrentPPU = 60; CurrentOrthgraphicSize = 540f; break;
		}
		CurrentResolutionWidth  = width_;
		CurrentResolutionHeight = height_;

		Debug.Log( $"Settings | Set Resolution ({CurrentResolutionWidth} x {CurrentResolutionHeight}) ppu: {CurrentPPU}");
	}

	public void SetScreenMode(FullScreenMode screenMode_)
	{
		CurrentScreenMode = screenMode_;
		Debug.Log( $"Settings | SetScreenMode {CurrentScreenMode}");
	}

	public void Print()
	{
		Debug.Log("Settings | ----------------------");
		Debug.LogFormat("Settings | Current PPU: {0}",      CurrentPPU);
		Debug.LogFormat("Settings | Resolution: {0} x {1}", CurrentResolutionWidth, CurrentResolutionHeight);
		Debug.LogFormat("Settings | Full Screen Mode: {0}", CurrentScreenMode);
		Debug.LogFormat("Settings | OrthgraphicSize: {0}", CurrentOrthgraphicSize);
		Debug.LogFormat("Settings | Saved Language: {0}",   SavedLanguage);
		Debug.LogFormat("Settings | Master Volume: {0}",    MasterVolume);
		Debug.LogFormat("Settings | BGM    Volume: {0}",    BgmVolume);
		Debug.LogFormat("Settings | SFX    Volume: {0}",    SfxVolume);
		Debug.LogFormat("Settings | Tutorial Cleared: {0}", IsTutorialCleared);
		Debug.Log("Settings | ----------------------");
	}

}