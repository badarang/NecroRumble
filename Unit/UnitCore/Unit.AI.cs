using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Pathfinding;
using System.Linq;
using TMPro;
using BehaviorDesigner.Runtime;
using Sirenix.Utilities;
using LOONACIA.Unity;
using Sirenix.OdinInspector;
using System;
using BehaviorDesigner.Runtime.Tasks;


public partial class Unit
{
    public LayerMask targetLayerMask;
    [HideInInspector] public LayerMask detectLayer;
    Vector3 _direction;
    protected Player _player;

    #region Strategy
    protected AddWeightBehavior _addAttackWeightBehaviorUndead;
    protected AddWeightBehavior _addMoveWeightBehaviorUndead;
    protected AddWeightBehavior _addAttackWeightBehaviorHuman;
    protected AddWeightBehavior _addMoveWeightBehaviorHuman;
    #endregion
    #region Property
        [ShowInInspector]
        bool _isAttackable = true;
        protected bool IsAttackable { get => _isAttackable; set => _isAttackable = value; }
        bool _isSpecialAttackable;
        protected bool IsSpecialAttackable { get => _isSpecialAttackable; set => _isSpecialAttackable = value; }

        // IsEnableAttack : 유닛 공격 비활성화 체크. IsAttackable : 공격하고 싶지만 쿨타임 같은 일시적 사유로 불가 체크
        bool _isEnableAttack = true;
        public bool IsEnableAttack { get => _isEnableAttack; set => _isEnableAttack = value; }
        bool _isEnableMove = true;
        public bool IsEnableMove {
                                    get => _isEnableMove; 
                                    set {
                                        _isEnableMove = value;
                                        // AIStop();
                                    }
                                 }
                                 
        public bool IsCampUnit = false;
        protected bool IsAggressive = true;
        protected Transform CampPosition = null;
                                 
        private GameObject _attackTarget = null;
        protected GameObject AttackTarget {
                                    get {
                                        return _attackTarget;
                                    }
                                    set {
                                        if(_attackTarget != null && _attackTarget.layer != Layers.Player){
                                            _attackTarget.GetComponent<Unit>().SubAggroCount();
                                        }
                                        _attackTarget = null;
                                        if(value != null){
                                            if(value.layer == Layers.Player){
                                                _attackTarget = value;
                                            }
                                            else if(value.GetComponent<Unit>().AddAggroCount()){
                                                _attackTarget = value;
                                            }
                                        }
                                    }
                                 }
        protected GameObject AttackingTarget;
        Transform _moveDestination;
        protected Transform MoveDestination {
                                    get {
                                        return _moveDestination;
                                    }
                                    set {
                                        _moveDestination = value;
                                    }
                                 }
        protected bool _isUnitCommandDone = false;
        
        protected int _maxTakeAggroSize = 1;
        protected int _currentTakeAggroCount = 0;
        protected bool _isRangedDealer = false;
    #endregion
    #region Status
        public float DetectRadius => Mathf.Max(IsAggressive?Statics.NormalDetectRadius:Statics.CampDetectRadius, instanceStats?.FinalAttackRange ?? 0);
        //TODO: detect 작을 때 플레이어 탐지 못해서 이상하게 움직이는 버그 있음. 
        
        //float _moveVelocity = 2f;
        protected float _arriveDistance = 3f;
        protected float _currentArriveDistance = 3f;
    #endregion
    #region Component
        protected Collider2D _collider;
        protected Vector2 _velocity;
        protected Rigidbody2D _rigid;
        protected void SetVelocity(Vector2 velocity_){
            // if(velocity_ == Vector2.zero) _rigid.mass = _mass;
            if(velocity_ == Vector2.zero) _rigid.mass = 999f;
            else _rigid.mass = _mass;
            _velocity = velocity_;
        }
        private BehaviorTree _behaviorTree;
    #endregion
    #region MoveControlMembers
        // protected AIPath _aiPath;
        // protected AIDestinationSetter _aiDestinationSetter;
        int _divisionNum = 24;
        float _unitDegree;
        Vector3[] _normalDirections;
        float[] _currentDirectionWeights;
        List<Tuple<float, GameObject>> _attackWeightList;
        protected List<Vector3> _moveWeightList;
        protected float _threshold = 0.5f;
        protected float _targetMaxWeight = 1f;
        protected int _targetWeightExponent = 4; // 타겟 가중치 그래프 지수, 원하는 n차함수 기울기 사용
        protected float _avoidMaxWeight = 1.2f;
        protected float _avoidRadius = 0.5f;
        protected float _avoidNoiseRange = 30f; // euler angle
        
        protected float _destinationSlowRange = 2f; // 도착지 슬로우 범위
        //float _returnLimitDistance = 5f;
        float _inertiaPortion = 0.3f; // 관성? 이전 이동 방향 유지 비율
        float _mass = 1f;
    #endregion
    #region AttackAggroMembers
        // undead
        private static float g_aggroDistByInertia_u = 2f; // 기존에 치던 타겟 거리 가중치 (언데드), 이 수치만큼 치던애면 점수 더 줌
        private static float g_aggroDistByCommander_u = 4f; // 커맨더 거리 가중치 (언데드), 이 수치만큼 커맨더면 점수 더 줌
        private static float g_aggroDistByRangedDealer_u = 4f; // 원딜 거리 가중치 (언데드), 이 수치만큼 원딜이면 점수 더 줌
        // human
        private static float g_aggroDistByInertia_h = 2f;  // 기존에 치던 타겟 거리 가중치 (휴먼)
        private static float g_aggroDistByCommander_h = 4f; // 커맨더 거리 가중치 (휴먼)
        private static float g_aggroDistByRangedDealer_h = 4f; // 원딜 거리 가중치 (휴먼)
        private static float g_aggroDistByPresence_h = 2.5f; // 남은 어그로 수용량 가중치 (휴먼), (이 수치 * 남은 어그로 수용량)만큼 점수 더 줌
        private static int g_aggroRemainByPlayerPresence_h = 1; // 플레이어 가중치 (휴먼), 플레이어가 이 수치만큼 어그로 수용량이 남은것처럼 점수 더 줌
        private static float g_aggroDistByHP_h = 10f; //  HP 가중치 (휴먼 힐러)
        //unit
        // 아래는 위의 가중치들이 유닛의 타겟 선택에 얼마나 영향을 끼치는지 (위의 보너스 점수에 곱 연산됨)
        private float _aggroInertiaWeight => PairUnitData.UnitAggroStats.AggroInertiaWeight;
        private float _aggroCommanderWeight => PairUnitData.UnitAggroStats.AggroCommanderWeight;
        private float _aggroRangedDealerWeight => PairUnitData.UnitAggroStats.AggroRangedDealerWeight;
        private float _aggroPresenceWeight_h => PairUnitData.UnitAggroStats.AggroPresenceWeight_h; // only human
    #endregion
    private void InitAI()
    {
        _behaviorTree = GetComponent<BehaviorTree>();
        //detect 레이어 초기화
        detectLayer = Layers.HumanUnit.ToMask() 
                    | Layers.UndeadUnit.ToMask() 
                    | Layers.Obstacle.ToMask() 
                    | Layers.Player.ToMask();
        _player = (GameManager.Instance == null) ? FindObjectOfType<Player>() : GameManager.Instance.GetPlayer();
        CampPosition = null;
        if(IsCampUnit){
            Transform campPosition = new GameObject().transform;
            campPosition.position = transform.position;
            SetCampUnit(campPosition);
        }
        #region MoveControlInit
        // _aiPath = GetComponent<AIPath>();
        // _aiPath.canMove = false;
        // _aiDestinationSetter = GetComponent<AIDestinationSetter>();

        _normalDirections = new Vector3[_divisionNum];
        _currentDirectionWeights = new float[_divisionNum];
        _attackWeightList = new List<Tuple<float, GameObject>>();
        _moveWeightList = new List<Vector3>();

        _mass = _rigid.mass;

        _unitDegree = 2 * Mathf.PI / _divisionNum;
        for (int i = 0; i < _divisionNum; ++i)
        {
            _normalDirections[i] = new Vector3(Mathf.Cos(_unitDegree * i), Mathf.Sin(_unitDegree * i), 0);
        }

        _addAttackWeightBehaviorUndead = new BaseAddAttackWeightBehavior_U();
        _addMoveWeightBehaviorUndead = new BaseAddMoveWeightBehavior();
        _addAttackWeightBehaviorHuman = new BaseAddAttackWeightBehavior_H();
        _addMoveWeightBehaviorHuman = new BaseAddMoveWeightBehavior();

        #endregion
    }
    
    public virtual bool AIPlayerCommandCheck(){
        if(CurrentFaction == Faction.Human) return false;
        else
            return true;
    }

    public virtual bool AIPlayerCommandAction(){
        switch (Player.currentCommand)
        {
            case EnumPlayerCommand.Recall:
                AIStop();
                break;
            case EnumPlayerCommand.None:
                _isUnitCommandDone = false;
                return false;
        }
        return true;
    }
    public virtual void AIDetectAround(Collider2D[] detectThings_){
        _moveWeightList.Clear();
        _attackWeightList.Clear();
        if(CurrentFaction == Faction.Undead){
            // Add aggroWeight
            _addAttackWeightBehaviorUndead.AddWeight(this, detectThings_);
            // Add moveWeight
            _addMoveWeightBehaviorUndead.AddWeight(this, detectThings_);
        }
        else{
            // Add aggroWeight
                _addAttackWeightBehaviorHuman.AddWeight(this, detectThings_);
            // Add moveWeight
            _addMoveWeightBehaviorHuman.AddWeight(this, detectThings_);
        }
        AttackTarget = CalculateAggroTarget();
    }
    protected void AddDestinationWeight(float arriveDistance_){
        if(MoveDestination != null){
            float destinationDist = Vector3.Distance(MoveDestination.position, transform.position);
            if(destinationDist >= arriveDistance_){
                AddMoveWeightOne(AIPathUpdate(), _targetMaxWeight);
            }
        }
    }
    
    public virtual EnumBTState AIAttackCheck(){     
        if(IsDead) return EnumBTState.Failure;    
        if(IsAttacking || IsSpecialAttacking) return EnumBTState.Running;
        if(!IsEnableAttack) return EnumBTState.Failure;
        bool canAttack = AttackTarget != null && _collider.Distance(AttackTarget.GetComponent<Collider2D>()).distance <= instanceStats.FinalAttackRange;
        if(canAttack) return EnumBTState.Success;
        return EnumBTState.Failure;
    }
    public virtual bool AISpecialAttack(){
        return false;
    }
    public virtual bool AIAttack(){
        if(IsAttackable){
            AIStop();
            IsAttackable = false;
            TryAttack(AttackTarget);
            StartCoroutine(AtkCooltimeRoutine());
            return true;
        }
        return false;
    }
    protected IEnumerator AtkCooltimeRoutine(){
        yield return new WaitForSeconds(1 / instanceStats.FinalAttackPerSec); //애니메이션으로 대체
        IsAttackable = true;
    }
    public virtual bool AIMoveCheck(){   
        if(IsDead) return false;    
        if(!IsEnableMove) return false;
        if(AttackTarget == null) return false;
        
        MoveDestination = AttackTarget.transform;
        
        if (AttackTarget.layer == Layers.Player)
        {
            FeedbackController feedbackController = this.GetComponent<FeedbackController>();
            if (feedbackController != null)
            {
                feedbackController.SetOutlineWhenTargetPlayer(true);
            }
        }
        else
        {
            FeedbackController feedbackController = this.GetComponent<FeedbackController>();
            if (feedbackController != null)
            {
                feedbackController.SetOutlineWhenTargetPlayer(false);
            }
        }
        
        float dist =  _collider.Distance(AttackTarget.GetComponent<Collider2D>()).distance;
        if(dist >= instanceStats.FinalAttackRange)
            AddWeightTargetOuterAttackRange(AIPathUpdate(), dist, _targetWeightExponent, _targetMaxWeight);
        return true;
    }
        public virtual bool AISpecialMove(){
        return false;
    }
    public virtual bool AIMove(){
    //     BackToOriginalRotation();
    //     _facingTarget = null;
        _direction = _inertiaPortion * _direction.normalized + (1-_inertiaPortion) * CalculateDirections(_threshold).normalized;
        Vector2 dir = _direction;
        Vector2 vel = _velocity + instanceStats.FinalMoveSpeed * dir;
        if (_direction == Vector3.zero)
        {
            // AIStop();
            IsWalking = false;
            return true;
        }
        Vector3 normVel = vel.normalized * instanceStats.FinalMoveSpeed;
        if(MoveDestination != null){
            float destinationDist = Vector3.Distance(MoveDestination.transform.position, transform.position);
            if(destinationDist <= _destinationSlowRange)
                SetVelocity((destinationDist /_destinationSlowRange) * normVel);
            else
                SetVelocity(normVel);
        }
        else{
            SetVelocity(normVel);
        }
        
        // if(vel.magnitude >= instanceStats.FinalMoveSpeed)
        //     _rigid.velocity = vel.normalized * instanceStats.FinalMoveSpeed;
        // else
        //     _rigid.velocity = vel;

        IsWalking = true; //AIStop 함수 호출시 false로 바뀜
        ShakeRotate();
        return true;
    }
    public virtual void AIIdle(){
        if(IsDead){
            AIStop();
            return;  
        } 
        if(CurrentFaction == Faction.Undead){
            AIIdle_U();
        }
        else{
            AIIdle_H();
        }
    }
    protected virtual void AIIdle_U(){
        if(Player.currentCommand == EnumPlayerCommand.None)
        {
            Transform playerTransform = _player.transform;
            float dist = Vector3.Distance(playerTransform.position, transform.position);
            if(dist >= _arriveDistance){
                MoveDestination = playerTransform;
                AddDestinationWeight(_arriveDistance);
            }
            AIMove();
            if (dist <= _arriveDistance){
                if(_velocity.magnitude <=  1.5f) AIStop();
                Collider2D[] detectFriends_ = new Collider2D[100];
                Physics2D.OverlapCircleNonAlloc(transform.position + (_player.transform.position - transform.position) * 0.5f, 0.5f, detectFriends_, Layers.UndeadUnit.ToMask());
                if(detectFriends_[1] != null){

                    AIStop();
                }
            }
            else if (dist <= _arriveDistance * 0.6f){
                AIStop();
            }
            
        }
    }
    protected virtual void AIIdle_H(){
        if(IsCampUnit && !IsAggressive)
        {
            AIGoToDestination(CampPosition, 0.5f);
        }
        else
        {
            AIGoToPlayer();
        }
        // AIStop();
    }

    public void AIStop(){
        SetVelocity(Vector2.zero);
        _direction = Vector3.zero;
        MoveDestination = null;
        _moveWeightList.Clear();
        for (int i = 0; i < _divisionNum; ++i){
            _currentDirectionWeights[i] = 0;
        }
        IsWalking = false;
        BackToOriginalRotation();
    }
    protected virtual void AIGoToPlayer(){
        AIGoToDestination(_player.transform, _currentArriveDistance);
    }
    protected virtual void AIGoToDestination(Transform transform_, float _arriveDistance){
        float dist = Vector3.Distance(transform_.position, transform.position);
        if(dist >= _arriveDistance){
            MoveDestination = transform_;
            AddDestinationWeight(_arriveDistance);
            // AddWeight(AIPathUpdate(), (dist -_limitPlayerDistance) / dist * _targetMaxWeight);
            
            AIMove();
        }
        else{
            AIStop();
        }
    }
    
    public void SetCampUnit(Transform position_){
        CampPosition = position_;
        IsAggressive = false;
    }
    
    private void SetAggressive(bool isAggressive_)
    {
        if (Group != null)
        {
            Group.MemeberUnits.ForEach(x => x.IsAggressive = isAggressive_);
        }
        else
        {
            IsAggressive = isAggressive_;
        }
    }
    
    #region MoveControl
        #region AstarPAthFinding
        protected Vector3 AIPathUpdate(){
            // Vector3 nextPosition;
            // Quaternion nextRotation;
            // _aiPath.MovementUpdate(BehaviorManager.instance.UpdateIntervalSeconds, out nextPosition, out nextRotation);
            // return nextPosition - transform.position;
            if(MoveDestination == null) return Vector3.zero;
            return MoveDestination.position - transform.position;
        }

        #endregion
    protected void AddMoveWeightOne(Vector3 vector_){
        _moveWeightList.Add(vector_);
    }
    protected void AddMoveWeightOne(Vector3 direction_, float weight_){
        _moveWeightList.Add(weight_ * direction_.normalized);
    }
    protected void AddAvoidWeight(Vector3 obstaclePos_, float distance_,  float avoidRadius_, float maxWeight_, float noiseAmount_, float noiseMin_ = 0){
        float noiseAngle;
        
        Vector3 opObstacle = transform.position - obstaclePos_;
        Vector3 desire = Vector3.zero;
        if(AttackTarget != null)
            desire = AttackTarget.transform.position;
        if(Mathf.Abs(Vector3.Dot(desire, Quaternion.Euler(0, 0, 60) * opObstacle)) >= Mathf.Abs(Vector3.Dot(desire, Quaternion.Euler(0, 0, -60) * opObstacle)))
            noiseAngle = UnityEngine.Random.Range(noiseMin_, -(noiseMin_ + noiseAmount_));
        else
            noiseAngle = UnityEngine.Random.Range(noiseMin_, (noiseMin_ + noiseAmount_));
        AddMoveWeightOne(maxWeight_ * (avoidRadius_-distance_) * (Quaternion.Euler(0, 0, noiseAngle)*opObstacle).normalized);
    }
    // distance must be attackRange > distance
    protected void AddWeightTargetInnerAttackRange(Vector3 vector_, float distance_, float exponent_, float maxWeight_ = 1.0f){
        if(distance_ > instanceStats.FinalAttackRange || distance_ < 0) return;
        AddMoveWeightOne(vector_, -(_threshold + maxWeight_)*Mathf.Pow((instanceStats.FinalAttackRange - distance_)/instanceStats.FinalAttackRange, exponent_) + maxWeight_);
    }
    // distance must be attackRange < distance < detectRadius
    protected void AddWeightTargetOuterAttackRange(Vector3 vector_, float distance_, float exponent_, float maxWeight_ = 1.0f){
        if(distance_ > DetectRadius || distance_ < 0) return;
        AddMoveWeightOne(vector_, (_threshold - maxWeight_)*Mathf.Pow((DetectRadius-distance_)/(DetectRadius -instanceStats.FinalAttackRange), exponent_) + maxWeight_);
    }
    protected Vector3 CalculateDirections(float threshold_ = 0){
        for (int i = 0; i < _divisionNum; ++i){
            _currentDirectionWeights[i] = 0;
        }
        int maxIdx = 0;
        for (int i = 0; i < _divisionNum; ++i){
            foreach (var weight in _moveWeightList){
                _currentDirectionWeights[i] += Vector3.Dot(_normalDirections[i], weight);
            }
            if(_currentDirectionWeights[i] > _currentDirectionWeights[maxIdx]){
                maxIdx = i;
            }
        } 
        _moveWeightList.Clear();
        if(Mathf.Abs(_currentDirectionWeights[maxIdx]) <= threshold_) {
            return Vector3.zero;
        }
        
        // // normalize for gizmo
        // if(_currentDirectionWeights[maxIdx] != 0){
        //     for (int i = 0; i < _divisionNum; ++i){
        //         _currentDirectionWeights[i] = _currentDirectionWeights[i] / _currentDirectionWeights[maxIdx];
        //     } 
        // }

        return _normalDirections[maxIdx];
    }

    private void DrawAIGizmos()
    {
        if(_currentDirectionWeights != null && _currentDirectionWeights.Length == _divisionNum && _currentDirectionWeights.Sum() != 0){
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            for (int i = 0; i < _divisionNum; ++i){
                if(_currentDirectionWeights[i] >= 0){
                    Gizmos.color = Color.green;
                }
                else{
                    Gizmos.color = Color.red;
                }
                Gizmos.DrawLine(transform.position + 0.5f * _normalDirections[i], transform.position + (0.5f + Mathf.Abs(_currentDirectionWeights[i])) * _normalDirections[i]);
            } 
        }
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, DetectRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, instanceStats?.FinalAttackRange ?? 0);
    }
        
    #endregion
    
    #region AggroCalculate
    
    protected void AddAttackWeightOne(GameObject object_,float weight_){
        _attackWeightList.Add(new Tuple<float, GameObject>(weight_, object_));
    }
    
    protected GameObject CalculateAggroTarget(){
        if(_attackWeightList.Count == 0) return null;
        Tuple<float, GameObject> max = new Tuple<float, GameObject>(float.MinValue, null);
        foreach(var weight in _attackWeightList){
            if(weight.Item1 >= max.Item1)
                max = weight;
        }
        _attackWeightList.Clear();
        
        return max.Item2;
        
    }

    private bool AddAggroCount(){
        if(_currentTakeAggroCount >= _maxTakeAggroSize){
            Debug.LogWarning("Current Aggro Count is Max. Cannot Add aggro count. current : " + _currentTakeAggroCount + " Max : " + _maxTakeAggroSize);
            return false;
        } 
        _currentTakeAggroCount += 1;
        return true;
    }
    private void SubAggroCount(){
        if(_currentTakeAggroCount == 0) Debug.LogError("Current Aggro Count is zero. Cannot Sub aggro count");
        _currentTakeAggroCount -= 1;
    }

    #endregion

    #region Rotation
    
    protected void ShakeRotate()
    {
        // 균일한 속도로 뒤뚱거리는 방식 (옛날 방식)
        //_flipableTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 10f) * 10f);
        
        // Flipable의 별도 애니메이터를 통해 애니메이션을 재생하는 방식 (new)
        _walkAnimRotator.SetIsWalking(true);
    }
    
    public void BackToOriginalRotation()
    {
        //_flipableTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(transform.rotation.eulerAngles.z, 0, 20f * Time.deltaTime));
        _walkAnimRotator.SetIsWalking(false);
    }
    
    
    #endregion
    
    #region Util
    public static bool IsExistInLayerMask(int layer, LayerMask layerMask)
    {
        return ((1 << layer) & layerMask) != 0;
    }
    
    private int GetRemainAggroNum(){
    
        return _maxTakeAggroSize - _currentTakeAggroCount;
    }

    #endregion
}

public enum EnumPlayerCommand
{
    Recall,
    None
}
public enum EnumBTState
{
    Failure,
    Success,
    Running
}

