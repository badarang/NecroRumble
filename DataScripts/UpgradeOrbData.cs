using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/UpgradeOrbData", fileName = nameof(UpgradeOrbData))]
public class UpgradeOrbData : ScriptableObject
{
    [InfoBox("StatModifierType은 Multiplier만 사용할 것!!\n" +
             "MaxHp, AttackDamage, AttackPerSec, MoveSpeed만 조정 가능!!", InfoMessageType.Info)]
    [SerializeField] private List<StatModifier> _possibleUpgrades = new();
    
    public List<StatModifier> PossibleUpgrades => _possibleUpgrades;
}
