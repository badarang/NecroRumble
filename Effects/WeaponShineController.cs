using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponShineController : MonoBehaviour
{
    private Material _material;
    void Start()
    {
        _material = GetComponent<SpriteRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (_material.HasProperty("_ShineLocation"))
        {
            float repeatValue = Mathf.Repeat(Time.time, 3f);
            float normalizedValue = Mathf.InverseLerp(1f, 0f, repeatValue);
            _material.SetFloat("_ShineLocation", normalizedValue);
        }
        else
        {
            Debug.LogWarning("Material doesn't have _Shine property");
        }
    }
}
