using MoreMountains.Tools;
using Unity.VisualScripting;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.UnitTask
{
    public class UnitIdle : Action
    {
        public SharedUnit _unit;
        public override void OnStart()
        {
        }

        public override TaskStatus OnUpdate()
        {
            if (_unit.Value == null) 
            {
                return TaskStatus.Failure;
            }
            _unit.Value.AIIdle();
            return TaskStatus.Success;
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