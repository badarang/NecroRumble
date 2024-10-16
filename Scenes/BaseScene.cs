using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseScene : MonoBehaviour
{
	public string _bgmName = "";
	public SceneType SceneType { get; protected set; } = SceneType.Unknown;

	void Awake()
	{
		Init();
	}

	protected virtual void Init()
	{
		Debug.Log("BaseScene | Init");

		Object obj = GameObject.FindObjectOfType(typeof(EventSystem));
		if (obj == null)
		{
			ManagerRoot.Resource.Instantiate("UI/@EventSystem", usePool: false).name = "@EventSystem";
		}
	}

	protected virtual void Start()
	{
		if (!string.IsNullOrEmpty(_bgmName))
		{
			PlayBGM(_bgmName);
		}
	}

	public abstract void Clear();
    public abstract void Redraw();

	public virtual void PlayBGM(string bgmName)
	{
		ManagerRoot.Sound.PlayBgm(true, bgmName);
	}

	public virtual void SceneChanged(string preSceneName_, string nextSceneName_)
	{
		ManagerRoot.I18N.setLanguage(System.Convert.ToString(ManagerRoot.Settings.CurrentLanguage));
	}
}
