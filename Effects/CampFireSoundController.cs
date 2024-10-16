using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class CampFireSoundController : MonoBehaviour
{
    private Transform player;  
    private float maxDistance = 6f;
    AudioSource audioSource;
    AudioClip audioClip;
    
    void Start()
    {
        player = GameManager.Instance.GetPlayer().Transform;
        audioSource = GetComponent<AudioSource>();
        audioClip = audioSource.clip;
    }
    void Update()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            float volume = (1f - (distanceToPlayer / maxDistance)) * 2f * ManagerRoot.Sound.SfxVolume * ManagerRoot.Sound.MasterVolume;
            if (volume > 0 && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
            
            if (audioSource.isPlaying)
            {
                audioSource.volume = volume;
                audioSource.panStereo = (transform.position.x - player.position.x) / maxDistance;
                if (volume <= 0)
                {
                    audioSource.Stop();
                }
            }
        }
        else
        {
            Debug.LogWarning("reference CampFireSoundController not set.");
        }
    }
}