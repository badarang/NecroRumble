using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public class MMFPlayerController : MonoBehaviour
{
    private MMF_Player myPlayer;
    private Transform _feelFeedbacks;
    private Transform _modelContainer;
    
    void OnPlayerComplete()
    {
        if (_modelContainer != null)
            _modelContainer.transform.localRotation = Quaternion.identity;
    }
    private void OnEnable()
    {
        myPlayer.Events.OnComplete.AddListener(OnPlayerComplete);
    }
    
    private void OnDisable()
    {
        myPlayer.Events.OnComplete.RemoveListener(OnPlayerComplete);
    }
    
    // Start is called before the first frame update
    void Awake()
    {
        myPlayer = GetComponent<MMF_Player>();
        _feelFeedbacks = transform.parent.transform;
        _modelContainer = _feelFeedbacks.parent.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
