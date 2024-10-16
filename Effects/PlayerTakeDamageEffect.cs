using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerTakeDamageEffect : MonoBehaviour
{
    Volume _volume;
    static Vignette vignette;

    void Start()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out vignette);
    }

    public void Update()
    {
        if (vignette != null)
        {
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0f, Time.deltaTime);
        }
    }

    public static void TakeDamageEffect(float curHpRatio)
    {
        vignette.intensity.value = curHpRatio * 0.5f;
    }
    
}
