using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

[CreateAssetMenu(menuName = "Data/Skill/"+nameof(NormalUnitUpgradeData), fileName = "New Upgrade")]
public class NormalUnitUpgradeData : SkillData
{
    [SerializeField] private List<StatModifier> _statModifiers = new();
    [SerializeField] private bool _isRecallUpgrade = false;

    public List<StatModifier> StatModifiers
    {
        get
        {
            foreach (var mod in _statModifiers)
            {
                mod.modifierName = Name;
            }
            return _statModifiers;
        }
    }

    public bool IsRecallUpgrade => _isRecallUpgrade;

    [Button("Auto Fill Description")]
    private void AutoSetDescription()
    {
        #if UNITY_EDITOR
         GetCurSkillData(0).Description = "";
         for (int i = 0; i < _statModifiers.Count; i++)
         {
             var mod = _statModifiers[i];
             
             bool hasDuration = mod.duration < float.MaxValue;
             bool isFlat = mod.type == StatModifierType.BaseAddition;
             bool isPlus = mod.value > 0;
             var condition = _isRecallUpgrade ? "when recalled." : hasDuration ? "when summoned." : "";
             var duration = hasDuration ? $" for {mod.duration} seconds " : "";
             var statName = mod.statType switch
             {
                 StatType.MaxHp => isFlat ? "Health " : "Health multiplier ",
                 StatType.AttackDamage => isFlat ? "Damage " : "Damage multiplier ",
                 StatType.AttackRange => "Range ",
                 StatType.AttackPerSec => "Attack Speed ",
                 StatType.MaxHatred => "Hatred ",
                 StatType.MoveSpeed => "Movespeed ",
                 StatType.MaxAggroNum => "Max Aggro Capacity ",
                 _ => throw new ArgumentOutOfRangeException()
             };

             //숫자 부분 색깔 선택
             var colorIndex = isPlus ? 1 : 0;
             var textColor = ColorPaletteManager.Instance.ColorPalettes
                 .Find(x => x.Name == "Text Colors")
                 .Colors[colorIndex];
             var hexCode = ColorUtility.ToHtmlStringRGB(textColor);
             
             //숫자 부분
             var statPlusMinus = isPlus ? "+" : "-";
             var statValue = Mathf.Abs(mod.value);
             var statTypeSymbol = mod.type switch
             {
                 StatModifierType.BaseAddition => "",
                 StatModifierType.Percentage => "%",
                 _ => ""
             };
             
             var nextLine = i == _statModifiers.Count - 1 ? "" : "\n";
             
             var modLine = $"{statName}<color=#{hexCode}>{statPlusMinus}{statValue}{statTypeSymbol}</color>{duration}{condition}{nextLine}";
             GetCurSkillData(0).Description += modLine;
         }
         

        //저장 가능하게
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
}
