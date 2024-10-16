using Unity.VisualScripting;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.UnitTask
{
    public class UnitSpecialAttack : Action
    {
        [UnityEngine.Serialization.FormerlySerializedAs("target")]
        public SharedUnit _unit;
        public override void OnStart()
        {
            if(_unit.Value == null) return;

            // _unit.Value.AIStop();
            _unit.Value.BackToOriginalRotation();
        }

        public override TaskStatus OnUpdate()
        {
            if (_unit.Value == null) 
            {
                return TaskStatus.Failure;
            }

            if(_unit.Value.AISpecialAttack()){
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
        }

    }
}