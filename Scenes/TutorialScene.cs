using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TutorialScene : BaseScene
{
    public UIOverlayCanvasScene _UIOverlayCanvasScene;

    public PortalController portalController;

    public CameraController camCtrl = null;
	public CameraController GetCam()
	{
		if (camCtrl == null)
		{
			camCtrl = FindObjectOfType<CameraController>();
		}
		return camCtrl;
	}

	private Player player = null;
	public Player GetPlayer()
	{
		if (player == null)
		{
			player = FindObjectOfType<Player>();
		}
		return player;
	}

    protected override void Init()
    {
        _bgmName = "Comet";
        base.Init();
        Debug.Log("TutorialScene | Init");
        SceneType = SceneType.Tutorial;

		if (_UIOverlayCanvasScene != null)
		{
			ManagerRoot.Resource.Release(_UIOverlayCanvasScene.gameObject);
		}
        _UIOverlayCanvasScene = ManagerRoot.UI.ShowSceneUI<UIOverlayCanvasScene>();

        RegisterEvent();

        StartCoroutine(GameStartCo());
    }

    public IEnumerator GameStartCo()
    {
        GetPlayer().gameObject.GetComponent<BoneSpear>().enabled = false;

        yield return new WaitForSeconds(1f);

        GameManager.Instance.ChangeState(GameManager.GameState.Prepare);
    }


	void RegisterEvent()
	{
        ManagerRoot.Input.OnPausePressed -= GameManager.Instance.PauseGame;
		ManagerRoot.Input.OnPausePressed += GameManager.Instance.PauseGame;

		ManagerRoot.Input.OnDisplayUnitHpPressed -= GameUIManager.Instance.ToggleDisplayHPbar;
		ManagerRoot.Input.OnDisplayUnitHpPressed += GameUIManager.Instance.ToggleDisplayHPbar;

		ManagerRoot.Input.OnRecallOrderPressed -= GetPlayer().GetComponent<Recall>().ExecuteRecallCommand;
		ManagerRoot.Input.OnRecallOrderPressed += GetPlayer().GetComponent<Recall>().ExecuteRecallCommand;
	}

    public override void Clear()
    {
        Debug.Log("TutorialScene | Clear");
    }

    void SetCamera()
    {
        Debug.Log("TutorialScene | Set Camera");

        Vector2Int curResol = new (ManagerRoot.Settings.CurrentResolutionWidth, ManagerRoot.Settings.CurrentResolutionHeight);
        GetCam().SetRefResolution( curResol );
        GetCam().SetPPU(ManagerRoot.Settings.CurrentPPU);
        GetCam().SetOrthgraphicSize( ManagerRoot.Settings.CurrentOrthgraphicSize );
    }

    public override void SceneChanged(string preSceneName_, string nextSceneName_)
    {
        base.SceneChanged(preSceneName_, nextSceneName_);
        Debug.Log("TutorialScene | Scene Changed");

        if( preSceneName_ == null){ Debug.Log("TutorialScene | Direct to TutorialScene"); return; }
        
        SetCamera();

        ManagerRoot.Input.DisableUIMode();
    }

    public override void Redraw()
    {
        Debug.Log("TutorialScene | Redraw");
        SetCamera();

        _UIOverlayCanvasScene?.SetHpBarLocalScale();
    }
}
