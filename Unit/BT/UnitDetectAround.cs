using System;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.UnitTask
{
    [TaskDescription("Check to see if the any object specified by the object list or tag is within the distance specified of the current agent.")]
    [TaskCategory("Movement")]
    [HelpURL("https://www.opsive.com/support/documentation/behavior-designer-movement-pack/")]
    [TaskIcon("62dc1c328b5c4eb45a90ec7a75cfb747", "0e2ffa7c5e610214eb6d5c71613bbdec")]
    public class UnitDetectAround : Conditional
    {
        [Tooltip("If using the object layer mask, specifies the maximum number of colliders that the physics cast can collide with")]
        public int m_MaxCollisionCount = 200;
        [Tooltip("The LayerMask of the objects to ignore when performing the line of sight check")]
        public LayerMask m_IgnoreLayerMask = 1 << LayerMask.NameToLayer("Ignore Raycast");

        private Collider2D[] m_Overlap2DColliders;

        public SharedUnit _unit;

        public override void OnStart()
        {
        }

        public override TaskStatus OnUpdate()
        {
            if (_unit.Value == null) {
                return TaskStatus.Failure;
            }
            
            if (m_Overlap2DColliders == null) {
                m_Overlap2DColliders = new Collider2D[m_MaxCollisionCount];
            }
            var count = Physics2D.OverlapCircleNonAlloc(transform.position, _unit.Value.DetectRadius, m_Overlap2DColliders, _unit.Value.detectLayer);
            // if(count == 0 || (count == 1 && m_Overlap2DColliders[0].gameObject == gameObject))
            //     return TaskStatus.Failure;
                
            _unit.Value.AIDetectAround(m_Overlap2DColliders);
            Array.Fill(m_Overlap2DColliders, null);
            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            m_IgnoreLayerMask = 1 << LayerMask.NameToLayer("Ignore Raycast");
        }

        public override void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (Owner == null || _unit.Value == null) {
                return;
            }
            var oldColor = UnityEditor.Handles.color;
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawWireDisc(Owner.transform.position, Owner.transform.forward, _unit.Value.DetectRadius);
            UnityEditor.Handles.color = oldColor;
#endif
        }
    }
}