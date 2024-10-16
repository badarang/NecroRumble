using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using LOONACIA.Unity.Managers;
using Pathfinding.RVO;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameScene : BaseScene
{
    UIOverlayCanvasScene _UIOverlayCanvasScene;
    
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
        Debug.Log("GameScene | Init");
        SceneType = SceneType.Game;


		if (_UIOverlayCanvasScene != null)
		{
			ManagerRoot.Resource.Release(_UIOverlayCanvasScene.gameObject);
		}
        _UIOverlayCanvasScene = ManagerRoot.UI.ShowSceneUI<UIOverlayCanvasScene>();

        GameObject portal = ManagerRoot.Resource.Load<GameObject>("Prefabs/Effects/@Portal");
        portalController = Instantiate(portal).GetComponent<PortalController>();
        portalController.transform.position = Statics.PortalPosition;
        portalController.gameObject.SetActive(false);

        RegisterEvent();
        // StartCoroutine(GameStartCo());
    }

    // public IEnumerator GameStartCo()
    // {
    //     yield return new WaitForSeconds(1f);
    //     GameManager.Instance.ChangeState(GameManager.GameState.PreRound);
    // }


	void RegisterEvent()
	{
        ManagerRoot.Input.OnPausePressed -= GameManager.Instance.PauseGame;
		ManagerRoot.Input.OnPausePressed += GameManager.Instance.PauseGame;

		ManagerRoot.Input.OnDisplayUnitHpPressed -= GameUIManager.Instance.ToggleDisplayHPbar;
		ManagerRoot.Input.OnDisplayUnitHpPressed += GameUIManager.Instance.ToggleDisplayHPbar;

		ManagerRoot.Input.OnRecallOrderPressed = null;
		ManagerRoot.Input.OnRecallOrderPressed += GetPlayer().GetComponent<Recall>().ExecuteRecallCommand;
	}

    public override void Clear()
    {
        Debug.Log("GameScene | Clear");
        if( portalController ){
            Destroy(portalController.gameObject);
        }

		if (_UIOverlayCanvasScene != null)
		{
			ManagerRoot.Resource.Release(_UIOverlayCanvasScene.gameObject);
		}
    }

    void SetCamera()
    {
        Debug.Log("GameScene | Set Camera");

        Vector2Int curResol = new (ManagerRoot.Settings.CurrentResolutionWidth, ManagerRoot.Settings.CurrentResolutionHeight);
        GetCam().SetRefResolution( curResol );
        GetCam().SetPPU(ManagerRoot.Settings.CurrentPPU);
        GetCam().SetOrthgraphicSize( ManagerRoot.Settings.CurrentOrthgraphicSize );
    }

    public override void SceneChanged(string preSceneName_, string nextSceneName_)
    {
        base.SceneChanged(preSceneName_, nextSceneName_);
        Debug.Log("GameScene | Scene Changed");

        if( preSceneName_ == null){ Debug.Log("GameScene | Direct to GameScene"); return; }
        
        SetCamera();

        ManagerRoot.Input.DisableUIMode();
    }

    public override void Redraw()
    {
        Debug.Log("GameScene | Redraw");
        SetCamera();

        _UIOverlayCanvasScene?.SetHpBarLocalScale();
    }
}
