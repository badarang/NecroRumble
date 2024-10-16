using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomChildSelector : MonoBehaviour
{
    private void OnEnable()
    {
        var childCount = transform.childCount;
        var randomIndex = UnityEngine.Random.Range(0, childCount);
        for (int i = 0; i < childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i == randomIndex);
        }
    }
}
