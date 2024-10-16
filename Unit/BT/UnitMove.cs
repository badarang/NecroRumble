using Unity.VisualScripting;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.UnitTask
{
    public class UnitMove : Action
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

            if (_unit.Value.AIMove())
            {
                return TaskStatus.Success;
            }
            return TaskStatus.Failure;
        }
        public override void OnEnd()
        {
            base.OnEnd();
        }
        public override void OnReset()
        {
            base.OnReset();
        }
    }
}