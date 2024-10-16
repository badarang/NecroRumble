using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal; // For PixelPerfectCam // using UnityEngine.U2D;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _smoothSpeed = 0.25f;
    private float _initialZoomSize = 9f;
    private Transform _target;
    private Vector3 _offset = new Vector3(0f, 0f, -10f);
    private Vector3 velocity = Vector3.zero;
    private Transform _zoomTarget;
   
    public PixelPerfectCamera pixelPerfectCamera;
    private int _ppu;
    private Vector2Int _refResol = new();

    public Transform ZoomTarget
    {
        get => _zoomTarget;
        set => _zoomTarget = value;
    }
    
    public Vector3 ShakeOffset { get; set; }

    private void Start()
    {
        _target = FindObjectOfType<Player>().transform;
        _zoomTarget = _target;
        Camera.main.orthographicSize = _initialZoomSize;

        if( !TryGetComponent(out pixelPerfectCamera) ){
            Debug.LogWarning("CameraController | pixelPerfectCamera is null in Start()"); 
        }
    }

    private void FixedUpdate()
    {
        if (_target != null)
        {
            if (_zoomTarget != _target)
            {
                Vector3 targetPosition = _zoomTarget.position + _offset;
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, _smoothSpeed) + ShakeOffset;
                Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, 7f, 1f);
            }
            else
            {
                Vector3 targetPosition = _target.position + _offset;
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, _smoothSpeed) + ShakeOffset;
                Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, _initialZoomSize, 1f);
            }
        }
    }

    public void SetPPU(int ppu_)
    {
        Debug.Log($"CameraController | Set PPU {ppu_}");
        if( pixelPerfectCamera == null){
            Debug.LogWarning("CameraController | pixelPerfectCamera is null"); 
            return;
        }

        _ppu = ppu_;

        pixelPerfectCamera.assetsPPU = _ppu;
    }
    
    public void SetRefResolution(Vector2Int refResol_)
    {
        Debug.Log($"CameraController | Set Ref Resolution {refResol_.x} x {refResol_.y}");
        if( pixelPerfectCamera == null){
            Debug.LogWarning("CameraController | pixelPerfectCamera is null"); 
            return;
        }

        _refResol = refResol_;

        pixelPerfectCamera.refResolutionX = _refResol.x;
        pixelPerfectCamera.refResolutionY = _refResol.y;
    }

    public void SetOrthgraphicSize(float orthographicSize_)
    {
        Debug.Log($"CameraController | Set Orthographic Size {orthographicSize_}"); 

		UniversalAdditionalCameraData mainCameraData = Camera.main.GetUniversalAdditionalCameraData();
		List<Camera> cameraStackList = mainCameraData.cameraStack;

        if(cameraStackList.Count > 0){
            Camera uIOverlayCam = cameraStackList[0]; // overlaycam
            uIOverlayCam.orthographicSize = orthographicSize_; // 800 x450
        }

    }
}