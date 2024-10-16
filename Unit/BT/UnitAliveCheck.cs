using Unity.VisualScripting;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.UnitTask
{
    public class UnitAliveCheck : Action
    {
        public SharedUnit _unit;
        public override void OnStart()
        {
            _unit.Value = GetComponent<Unit>();
            if(_unit.Value == null)
                return;
        }

        public override TaskStatus OnUpdate()
        {
            if (_unit.Value == null) 
            {
                return TaskStatus.Failure;
            }

            if(_unit.Value.IsUnitDisable()){
                _unit.Value.AIStop();
                return TaskStatus.Failure;
            }
            else{
                return TaskStatus.Success;
            }
        }

        public override void OnEnd(){
            base.OnEnd();   
        }
        public override void OnReset()
        {
            base.OnReset();
        }

    }
}