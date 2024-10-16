using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity;
using UnityEngine;

public static class Layers
{
    public static readonly int Player = LayerMask.NameToLayer("Player");
    public static readonly int Obstacle = LayerMask.NameToLayer("Obstacle");
    public static readonly int HumanCorpse = LayerMask.NameToLayer("HumanCorpse");
    public static readonly int UndeadCorpse = LayerMask.NameToLayer("UndeadCorpse");
    public static readonly int HumanUnit = LayerMask.NameToLayer("HumanUnit");
    public static readonly int UndeadUnit = LayerMask.NameToLayer("UndeadUnit");
    public static readonly int PlayerObstacle = LayerMask.NameToLayer("PlayerObstacle");
    public static readonly int Area = LayerMask.NameToLayer("Area");
    public static readonly int GridGround = LayerMask.NameToLayer("GridGround");
    public static readonly int CorpseOverlapPrevent = LayerMask.NameToLayer("CorpseOverlapPrevent");

    public static readonly LayerMask Mask_BothUnit = HumanUnit.ToMask() + UndeadUnit.ToMask();
    public static readonly LayerMask Mask_BothCorpse = HumanCorpse.ToMask() + UndeadCorpse.ToMask();
    
}
