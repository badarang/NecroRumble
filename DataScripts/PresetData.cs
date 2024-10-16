using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Wave/" + nameof(PresetData), fileName = nameof(PresetData))]
public class PresetData : ScriptableObject
{
    [SerializeField] private List<PresetInfo> _info;

    public List<PresetInfo> GetInfo => _info;
}

[Serializable]
public struct PresetInfo
{
    [SerializeField] private UnitType _unitType;
    [SerializeField] private int _quantity;

    public UnitType UnitType => _unitType;
    public int Quantity => _quantity;
}

