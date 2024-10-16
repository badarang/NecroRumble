using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailController : MonoBehaviour
{
    public float Damage { get; private set; }
    public void Init(float damage_)
    {
        Damage = damage_;
    }
    
}
