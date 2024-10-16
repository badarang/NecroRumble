using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySeconds : MonoBehaviour
{
    [SerializeField] private float destroySeconds = 1f;

    void Update()
    {
        destroySeconds -= Time.deltaTime;
        if (destroySeconds <= 0)
        {
            Destroy(gameObject);
        }
    }
}
