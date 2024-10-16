using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class SwordEffectController : MonoBehaviour
{
    public void Init(float scale_)
    {
        GameObject effect = transform.parent.Find("VFXSwordHitEffect").gameObject;
        GameObject waveParticleSys = effect.transform.GetChild(3).gameObject;
        waveParticleSys.transform.localScale = new Vector3(scale_, scale_, scale_);
        
        StartCoroutine(StartCallShockWave());
        StartDoTween();
    }
    
    void StartDoTween()
    {
        Vector3 initialScale = transform.localScale;
        Vector3 initialPosition = transform.parent.localPosition;
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        
        if (renderer == null || renderer.material == null)
        {
            return;
        }
        
        transform.position = new Vector3(initialPosition.x, initialPosition.y + 10f, initialPosition.z);

        StartCoroutine(PlayEffectSound());
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(renderer.material.DOFade(1f, 0.1f));
        sequence.Append(renderer.material.DOFade(0f, 0.6f));
        sequence.Insert(0f, transform.DOMoveY(initialPosition.y, 0.1f));
        sequence.Insert(0.7f, transform.DOScaleX(initialScale.x, 0.1f));
        sequence.Play();
        
        sequence.OnComplete(() =>
        {
            sequence.Kill();
            Destroy(transform.parent.gameObject);
        });
    }
    
    IEnumerator PlayEffectSound()
    {
        yield return new WaitForSeconds(0.1f);
        var effectSounds = new List<string> {"Heavy Sword Swing 10", "Heavy Sword Swing 12", "Heavy Sword Swing 17", "Heavy Sword Swing 19"};
        ManagerRoot.Sound.PlaySfx(effectSounds[Random.Range(0, effectSounds.Count)], 1f);
        
        var effectSounds2 = new List<string> {"Punch Impact (Flesh) 3", "Punch Impact (Flesh) 4", "Punch Impact (Flesh) 6"};
        ManagerRoot.Sound.PlaySfx(effectSounds2[Random.Range(0, effectSounds2.Count)], 1f);
    }
    
    IEnumerator StartCallShockWave()
    {
        yield return new WaitForSeconds(0.1f);
        transform.parent.Find("ShockWavePanel").GetComponent<ShockWaveController>().CallShockWave();
    }
}