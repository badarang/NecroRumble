using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

public class ShockWaveController : MonoBehaviour
{
    [SerializeField] private float _shockWaveTime = .75f;
    private Coroutine _shockWaveCoroutine;
    private Material _shockWaveMaterial;
    private static int _waveDistanceFromCenter = Shader.PropertyToID("_WaveDistanceFromCenter");

    private void Awake()
    {
        _shockWaveMaterial = GetComponent<SpriteRenderer>().material;
    }

    public void CallShockWave()
    {
        _shockWaveCoroutine = StartCoroutine(ShockWaveAction(-0.1f, 1f));
    }
    
    private IEnumerator ShockWaveAction(float startPos_ = -.1f, float endPos_ = 1f)
    {
        _shockWaveMaterial.SetFloat(_waveDistanceFromCenter, startPos_);
        float lerpedAmount = 0f;
        float elapsedTime = 0f;
        while(elapsedTime < _shockWaveTime)
        {
            lerpedAmount = Mathf.Lerp(startPos_, endPos_, elapsedTime / _shockWaveTime);
            _shockWaveMaterial.SetFloat(_waveDistanceFromCenter, lerpedAmount);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    
    [Button("Start Shock Wave")]
    private void StartShockWave()
    {
        CallShockWave();
    }
}