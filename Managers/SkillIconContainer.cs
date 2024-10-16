using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillIconContainer : MonoBehaviour
{
    [HeaderAttribute("Skill Icon Property")]
    public int skillIdx;
    public float maxCooltime;
    public int curUpgradeNum;
    
    private Skill _curSkill;
    public float _currentCooltime = 0f;
    
    [HeaderAttribute("Skill GameObject")]
    private GameObject _skillIconOutline;
    private GameObject _skillIconBackground;
    private GameObject _skillIcon;
    private GameObject _skillIconCooltime;
    private float _skillIconCooltimeOriginalHeight;
    private GameObject _skillIconHoverContainer;
    
    // Start is called before the first frame update
    void Start()
    {
        _skillIconOutline = transform.Find("SkillIconOutline").gameObject;
        _skillIconBackground = transform.Find("SkillIconBackground").gameObject;
        _skillIcon = transform.Find("SkillIcon").gameObject;
        _skillIconCooltime = transform.Find("SkillIconCooltime").gameObject;
        _skillIconCooltimeOriginalHeight = _skillIconCooltime.GetComponent<RectTransform>().rect.height;
        _skillIconHoverContainer = transform.Find("SkillIconHoverContainer").gameObject;
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCooltime();
        if (Input.GetKey(KeyCode.LeftShift) && _skillIconHoverContainer.activeSelf)
        {
            ViewSpecificSkillDescription();
        }
        else
        {
            ViewOriginalSkillDescription();
        }
    }
    
    void Init()
    {
        _curSkill = OldSkillManager.Instance.currentSkillList[skillIdx];
        string spritePath = "Sprites/Skills/Skill_" + _curSkill.info.Name;
        _skillIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>(spritePath);
        _skillIconHoverContainer.transform.Find("SkillIconHoverTitle").GetComponent<TextMeshProUGUI>().text = _curSkill.info.Displayname;
        _skillIconHoverContainer.transform.Find("SkillIconHoverCooltime").GetComponent<TextMeshProUGUI>().text = _curSkill.info.Cooltime.ToString();
        _skillIconHoverContainer.transform.Find("SkillIconHoverCost").GetComponent<TextMeshProUGUI>().text = _curSkill.info.Cost.ToString();
        _skillIconHoverContainer.transform.Find("SkillIconHoverDescription").GetComponent<TextMeshProUGUI>().text = _curSkill.info.Description;
    }

    void UpdateCooltime()
    {
        if (_currentCooltime > 0f)
        {
            _currentCooltime -= Time.deltaTime;
            _skillIconOutline.GetComponent<Image>().color = new Color(1f, .5f, 0.5f);
        }
        else
        {
            if (_currentCooltime <= 0f)
            {
                _currentCooltime = 0f;
            }
            _skillIconOutline.GetComponent<Image>().color = new Color(0.5f, 1f, 0.5f);
        }
        _skillIconCooltime.GetComponent<RectTransform>().sizeDelta = new Vector2(_skillIconCooltime.GetComponent<RectTransform>().rect.width, _skillIconCooltimeOriginalHeight * (_currentCooltime / maxCooltime));
    }
    
    void ViewSpecificSkillDescription()
    {
        if (_curSkill.info.UpgradeValues.Count < 2)
            return;
        
        string skillDescription = _skillIconHoverContainer.transform.Find("SkillIconHoverDescription").GetComponent<TextMeshProUGUI>().text;

        string newSkillDescription = "{";
        for (int i = 0; i < _curSkill.info.UpgradeValues.Count; i++)
        {
            newSkillDescription += $"{_curSkill.info.UpgradeValues[i]}";
            if (i < _curSkill.info.UpgradeValues.Count - 1)
            {
                newSkillDescription += "/";
            }
        }
        newSkillDescription += "}";
        
        string matchValue = "{" + $"{_curSkill.info.UpgradeValues[curUpgradeNum - 1]}" + "}";
        skillDescription = skillDescription.Replace(matchValue,$"<color=green>{newSkillDescription}</color>");


        // Update the skillDescription
        _skillIconHoverContainer.transform.Find("SkillIconHoverDescription").GetComponent<TextMeshProUGUI>().text = skillDescription;
    }
    
    void ViewOriginalSkillDescription()
    {
        _skillIconHoverContainer.transform.Find("SkillIconHoverDescription").GetComponent<TextMeshProUGUI>().text = _curSkill.info.Description;
    }

    public Skill GetCurSkill()
    {
        return _curSkill;
    }
}
