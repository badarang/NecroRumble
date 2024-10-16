using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using DG.Tweening;
using UnityEngine;

public class TutorialUnitController : MonoBehaviour
{
	private CameraController _cameraController;
	void Start()
	{
		_cameraController = Camera.main.GetComponent<CameraController>();
	}

	void Update()
	{
		if (_cameraController.ZoomTarget != GameManager.Instance.GetPlayer().transform || TutorialManager.Instance.IsUnitStop)
		{
			transform.DOMove(transform.position, 1f).SetEase(Ease.Linear);
		}
	}
}