using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(ReviveData), fileName = nameof(ReviveData))]
public class ReviveData : SkillData
{

    public int _requiredMP;

    public int _maxReviveCount;
    public InputHandlerType _inputHandlerType;
    public RangeSelectorType _rangeSelectorType;

    public RevivePriorityType prior;

    public bool _canMove;

    public float _afterDelayTime;

}
