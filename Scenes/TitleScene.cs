    using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using LOONACIA.Unity.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TitleScene : BaseScene
{
    UITitleScene _uITitleScene;
    GameObject _campFireObj;


    protected override void Init() // ManagerRoot.Init 보다 빠름
    {
        Debug.Log("TitleScene | Init");

        _bgmName = "Azimuth";
        base.Init();
        SceneType = SceneType.Title;

	}
	public override void Clear(){}


	protected override void Start()
	{
		base.Start();
        Debug.Log("TitleScene | Start");

		if(ManagerRoot.Settings.IsTutorialCleared)
		{
			
		}
	}

	// void Update()
	// {
	// 	if (Input.GetKeyDown(KeyCode.F5))
	// 	{
	// 		//SceneManagerEx.LoadScene(SceneType.Game);
	// 		SceneManagerEx.LoadScene("Main");
	// 	}
	// }

    public void EnableUITitleScene()
    {
        Debug.Log("TitleScene | EnableUITitleScene ");
        if(_uITitleScene){
            _uITitleScene.enabled = true;
        }
    }
    public void DisableUITitleScene()
    {
        Debug.Log("TitleScene | DisableUITitleScene ");
        if(_uITitleScene){
            _uITitleScene.enabled = false;
        }
    }
    public override void SceneChanged(string preSceneName_, string nextSceneName_)// ManagerRoot.Init 이후
    {
        base.SceneChanged(preSceneName_, nextSceneName_);
        Debug.Log("TitleScene | Scene Changed");

        _uITitleScene = ManagerRoot.UI.ShowSceneUI<UITitleScene>();
        FindObjectOfType<EventSystem>().firstSelectedGameObject = GameObject.Find("UIPausePopupButton");

        SetScreen();

        _campFireObj = GameObject.Find("@TitleCampFire");
        SetCampFireVolume();
    }

    void SetScreen()
    {
        int width                 = ManagerRoot.Settings.CurrentResolutionWidth;
        int height                = ManagerRoot.Settings.CurrentResolutionHeight;
        FullScreenMode screenMode = ManagerRoot.Settings.CurrentScreenMode;

        Debug.Log($"TitleScene | Set Screen {width}x{height} {screenMode}");
        Screen.SetResolution(width, height, screenMode);

        Redraw();
    }

    public override void Redraw()
    {
        Debug.Log("TitleScene | Redraw");
        _uITitleScene.Redraw();
    }

    public void SetCampFireVolume() // 예외처리용 함수 차후 SoundManager에 통합필요함
    {
        if( _campFireObj == null){ Debug.LogWarning("Title Scene | _campFireObj is null");return; }
        
        AudioSource campFireAudio = _campFireObj.GetComponent<AudioSource>();
        if( campFireAudio == null){ Debug.LogWarning("Title Scene | campFireAudio is null"); return; }

        float defaultVol = 0.1f;
        campFireAudio.volume = defaultVol * ManagerRoot.Sound.MasterVolume * ManagerRoot.Sound.SfxVolume;
    }

	void OnDisable()
	{
		Debug.Log("TitleScene | OnDestroy");
		//ManagerRoot.Settings.Save();	
	}
}
