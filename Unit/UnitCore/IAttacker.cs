
using UnityEngine;

public interface IAttacker
{
    GameObject GameObject { get; }
    Transform  Transform  { get; }

    bool ActiveSelf { get; }
}
