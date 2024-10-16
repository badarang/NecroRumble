using Unity.VisualScripting;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.UnitTask
{
    public class UnitAttackCheck : Action
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

            switch (_unit.Value.AIAttackCheck())
            {
                case EnumBTState.Failure:
                    return TaskStatus.Failure;
                case EnumBTState.Success:
                    return TaskStatus.Success;
                case EnumBTState.Running:
                    return TaskStatus.Running;
            }
            return TaskStatus.Failure;
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