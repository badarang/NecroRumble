using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class SkillData : ScriptableObject, ISkill
{
    [DetailedInfoBox("Id 규칙..","<ID> \n0~9 = 플레이어 스킬\n10~99 = 주물\n100~199 = 일반 유닛 보상\n200~299 = 전사 보상\n300~399 = 궁수 보상\n400~499 = 암살자 보상")]
    [SerializeField] protected uint _id;
    
    [SerializeField] protected string _name;
    
    [DetailedInfoBox("Skill Grade 규칙..","<SKILL GRADE>\n-1 (및 0이하) = 보상으로 주어지지 않음\n001~010 = 주물 보상\n100~110 = 일반 유닛 보상\n200~210 = 전용 유닛 보상\n-99 = 사용 안함")]
    [SerializeField] protected int _skillGrade;
    
    protected int maxSkillLevel;
    
    //TODO: SO 데이터 밖으로 빼기
    //[SerializeField] protected int curSkillLevel;
    
    [SerializeField] private List<SkillDataAttribute> _skillDataList;
    public int SkillGrade
    {
        get => _skillGrade;
        set => _skillGrade = value;
    }
    public uint Id
    {
        get => _id;
        set => _id = value;
    }
    public string Name
    {
        get => _name;
        set => _name = value;
    }
    
    public List<SkillDataAttribute> SkillDataList => _skillDataList;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="skillLevel">방금 얻은 스킬은 스킬레벨이 1이다.</param>
    /// <returns></returns>
    public SkillDataAttribute GetCurSkillData(int skillLevel)
    {
        if (skillLevel <= 0) skillLevel = 1;
        if (skillLevel > _skillDataList.Count) return null;
        return _skillDataList[skillLevel - 1];
    }
    
    public int GetMaxSkillLevel()
    {
        maxSkillLevel = Mathf.Max(_skillDataList.Count, maxSkillLevel);
        return maxSkillLevel;
    }
}

[System.Serializable]
public class SkillDataAttribute
{
    [AssetSelector(Paths = "Assets/Resources/Sprites/Skills", FlattenTreeView = true)]
    [PreviewField(ObjectFieldAlignment.Center)]
    
    [SerializeField] private Sprite _iconSprite;
    
    [SerializeField] protected string _displayName;
    [ColorPalette("Text Colors")] [SerializeField, HideLabel]
    [InlineButton("CopyHexCodeToClipboard",SdfIconType.Clipboard, label: "HexCode")] private Color _textColor;
    [SerializeField, TextArea] protected string _description;
    [SerializeField] protected float _cooldown;

    public UnitType unitType; // todo 고도화
    public Sprite IconSprite => _iconSprite;
    
    public float Cooldown => _cooldown;

    public string Description
    {
        get => _description;
        set => _description = value;
    }
    public string DisplayName => _displayName;
    
    //TODO: Generic 으로 해결?

    
    private void CopyHexCodeToClipboard()
    {
        #if UNITY_EDITOR
        var hexCode = ColorUtility.ToHtmlStringRGB(_textColor);
        var finalString = $"<color=#{hexCode}>";
        GUIUtility.systemCopyBuffer = finalString;
        #endif
    }

}


