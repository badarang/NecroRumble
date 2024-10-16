
using System.Collections.Generic;
using UnityEngine;
using DigitalRuby.LightningBolt;
using LOONACIA.Unity.Managers;
using System.Linq;

public enum RangeSelectorType
{
    CircleRangeSelector,
    EclipseRangeSelector,
    ChargingRangeSelector,
    FrontSquareRangeSelector,
    FrontCircleRangeSelector,
    PointerCircleRangeSelector,
    UnderfootRangeSelector,
}

public abstract class RangeSelector
{
    protected Player _player;
    private List<Transform> _lightningBoltTransforms = new();

    #region RangeSelect
    private Queue<Unit> _selectedQ = new();
    public Queue<Unit> SelectedQ => _selectedQ;
    #endregion

    #region Effect
    private GameObject ElectricLinePrefab;
    public static bool CanDoScale;

    #endregion
    
    public virtual void Init(){
        
        ElectricLinePrefab = Resources.Load<GameObject>("Prefabs/ElectricLine");
        _player = GameManager.Instance.GetPlayer();
    }
    public virtual void RangeSelect(){}
    public virtual void ResetValues(){
        DeleteLightningBolt();
        while(SelectedQ.Count > 0)
        { 
            UnselectOne( SelectedQ.Dequeue() );
        }
        SelectedQ.Clear();    
    }
    protected virtual void EnqueueSelected(Collider2D[] selectedThings_)
    {
        if(selectedThings_ == null){ return; }

        foreach(var selected in selectedThings_)
        {
            EnqueueSelected(selected);
        }
    }
    
    protected virtual void EnqueueSelected(Collider2D selected_)
    {
        if (selected_.TryGetComponent(out Unit unit))
        {
            if (_selectedQ.Contains(unit))
            {
                return;
            }
            if (unit.CanRevive == false) return;
            if (_player.CanAction == false) return;
            _selectedQ.Enqueue(unit);
            SelectOne(unit);
            if (unit.CurrentFaction == Faction.Human)
            {
                CreateLightningBolt(unit); //리바이브 때만 번개 생성하도록
                // var distance = Vector3.Distance(_player.transform.position, unit.transform.position);
                // ManagerRoot.Sound.PlaySfx("Electric Plazma", distance / 10f);
            }
            // // speed에 weight 증가량 반영
            // _corpseWeight += 3f; // todo: corpse.GetComponent<Unit>().Weight;
            // StartCoroutine(WeightDecreaseCO());
        }
    }

    public void EnqueueSelected(Unit unit_)
    {
        _selectedQ.Enqueue(unit_);
    }

    
    public void DequeueOoRSelected(Collider2D[] selectedThings_)
    {
        var selectedSet = new HashSet<Unit>();
        foreach (var selected in selectedThings_)
        {
            if( selected.TryGetComponent(out Unit unit)){
                selectedSet.Add(unit);
            }
        }

        int originalCount = _selectedQ.Count;
        for (int i = 0; i < originalCount; i++)
        {
            var currentSelected = _selectedQ.Dequeue();
            if (selectedSet.Contains(currentSelected))
            {
                _selectedQ.Enqueue(currentSelected);
            }
            else
            {
                UnselectOne( currentSelected );
            }
        }
    }
    
    public void ClearSelectedQueue()
    {
        foreach (var corpse in _selectedQ)
        {
            UnselectOne(corpse);
        }
        _selectedQ.Clear();
        DeleteLightningBolt();
    }

    

    void SelectOne(Unit unit_)
    {
        if (unit_.TryGetComponent(out FeedbackController feedback))
        {
            // range selector는 revive랑 별개여서 다른 곳에서 판단해야 할 듯
            // feedback.ChangeMaterialBasedOnIsSelected(true, _revive.CanCast);
            feedback.ChangeMaterialBasedOnIsSelected(true);
        }
        //_playerMove.GetComponent<Player>().IncreaseNecroLightIntensity(1f);
    }

    void UnselectOne(Unit unit_)
    {
        if (unit_ == null) return;
        
        if (unit_.TryGetComponent(out FeedbackController feedback)) 
        {
            feedback.ChangeMaterialBasedOnIsSelected(false);
        }
    }
    
    public void UpdateSelectedMaterials()
    {
        foreach (var corpse in _selectedQ)
        {
            SelectOne(corpse);
        }
    }

    public void CreateLightningBolt(Unit _unit)
    {
        // GameObject ElectricLine = GameObject.Instantiate(ElectricLinePrefab, _player.transform.position, Quaternion.identity);
        GameObject ElectricLine = ManagerRoot.Resource.Instantiate(ElectricLinePrefab, _player.transform.position, Quaternion.identity, usePool: false);
        ElectricLine.SetActive(false);
        ElectricLine.transform.SetParent(_player.transform);
        ElectricLine.name = "ElectricLine";
        _lightningBoltTransforms.Add(ElectricLine.transform);
        if (ElectricLine.TryGetComponent(out LightningBoltScript _lightningBoltScript))
        {
            _lightningBoltScript.StartObject = _player.gameObject;
            _lightningBoltScript.EndObject = _unit.transform.gameObject;
        }
        ElectricLine.SetActive(true);
        CanDoScale = true;
    }
    
    public void DeleteLightningBolt()
    {
        foreach (Transform bolt in _lightningBoltTransforms)
        {
             ManagerRoot.Resource.Release(bolt.gameObject);
            // Destroy(ElectricLine.gameObject);
        }
        _lightningBoltTransforms.Clear();
        CanDoScale = false;
    }
}
