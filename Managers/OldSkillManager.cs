using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldSkillManager : MonoBehaviour
{
    private static OldSkillManager instance = null;
    public int currentUpgradeNum;
    public SkillInfo info;
    
    void Awake()
    {
        Init();
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    
    public static OldSkillManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    [HeaderAttribute("Data")]
    private DataDB _dataDB;
    [SerializeField] private GameObject _content;
    public List<SkillIconContainer> skillIconContainerList = new List<SkillIconContainer>();

    public List<Skill> AllSkillList = new List<Skill>();
    public List<Skill> currentSkillList = new List<Skill>();
    
    void Init()
    {
        _dataDB = Resources.Load<DataDB>("Data/DataDB");
        for (int i = 0; i < _dataDB.SkillSheet.Count; i++)
        {
            Skill skill = new Skill();
            skill.info = _dataDB.SkillSheet[i];
            skill.currentupgradeNum = 1;
            
            AllSkillList.Add(skill);
        }
        
        //Test
        currentSkillList.Add(AllSkillList[2]); //뼈창
        currentSkillList.Add(AllSkillList[0]); //네크로맨싱
        currentSkillList.Add(AllSkillList[5]); //시체폭발
        
        IngameOnloadInit();
    }
    
    private void IngameOnloadInit()
    {
        for (int i = 0; i < currentSkillList.Count; i++)
        {
            GameObject skillIconContainer = Instantiate(Resources.Load("Prefabs/UI/SkillIconContainer")) as GameObject;
            skillIconContainer.transform.SetParent(_content.transform);
            skillIconContainer.GetComponent<SkillIconContainer>().skillIdx = i;
            skillIconContainer.GetComponent<SkillIconContainer>().maxCooltime = currentSkillList[i].info.Cooltime;
            skillIconContainer.GetComponent<SkillIconContainer>().curUpgradeNum = currentSkillList[i].currentupgradeNum;
            skillIconContainerList.Add(skillIconContainer.GetComponent<SkillIconContainer>());
        }
    }
}
