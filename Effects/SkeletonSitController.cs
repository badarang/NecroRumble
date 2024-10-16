using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonSitController : MonoBehaviour
{
    public void AnimSleepEvent()
    {
        transform.Find("SleepText").gameObject.SetActive(true);
    }
    
    public void AnimWakeUpEvent()
    {
        transform.Find("SleepText").gameObject.SetActive(false);
    }
}
