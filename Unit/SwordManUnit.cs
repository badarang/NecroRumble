using System;
using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity;
using UnityEngine;

public class SwordManUnit : Unit
{
    public bool isGiantUnit { get; set; } = false;
    public bool isHaveFuryHeart { get; set; } = true;
    public override void Start()
    {
        base.Start();
    }
    
    protected override void Init()
    {
        base.Init();
        if(CurrentFaction == Faction.Undead){
            UIWinPopup.IsFamilyImage[(int)UIWinPopup.Images.SkeletonImage] = true;

			SteamUserData.IsWarriorRevived = true;
		}
    }

}
