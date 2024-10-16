using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class FireMagicCircleSoundController : MonoBehaviour
{
    public void PlaySoundStart()
    {
        ManagerRoot.Sound.PlaySfx("Impact 12");
    }
    
    public void PlaySound1()
    {
        ManagerRoot.Sound.PlaySfx("Stabbing (Person) Larger 18", 1f);
    }
    
    public void PlaySound2()
    {
        ManagerRoot.Sound.PlaySfx("Stabbing (Person) Larger 15", 1f);
    }
    
    public void PlaySound3()
    {
        ManagerRoot.Sound.PlaySfx("Stabbing (Person) Larger 12", 1f);
    }
    
    public void PlaySound4()
    {
        ManagerRoot.Sound.PlaySfx("Stabbing (Person) Larger 9", 1f);
    }
}
