using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class RangeLineRendererController : MonoBehaviour
{
    //범위에 들어오는 유닛에게 Outline 씌워주려고 했는데, 지금은 사용하지 않음.
    // private LineRenderer _lineRenderer;
    // // Start is called before the first frame update
    // void Start()
    // {
    //     _lineRenderer = GetComponent<LineRenderer>();
    // }
    //
    // // Update is called once per frame
    // void Update()
    // {
    //     if (_lineRenderer.enabled)
    //     {
    //         if (_lineRenderer.startWidth == 1.0f) //Grind
    //         {
    //             foreach (var unit in ManagerRoot.Unit.GetAllAliveUndeadUnits())
    //             {
    //                 if (unit.TryGetComponent(out FeedbackController feedback))
    //                 {
    //                     feedback.ChangeMaterialBasedOnIsSelected(false);
    //                 }
    //             }
    //             
    //             if (_lineRenderer.positionCount < 2) return;
    //             if (_lineRenderer.gameObject.TryGetComponent(out MeshCollider meshCollider_))
    //             {
    //                 Destroy(meshCollider_);
    //             }
    //
    //             _lineRenderer.useWorldSpace = false;
    //             MeshCollider meshCollider = _lineRenderer.gameObject.AddComponent<MeshCollider>();
    //             Mesh mesh = new Mesh();
    //             _lineRenderer.BakeMesh(mesh, Camera.main, false);
    //             meshCollider.sharedMesh = mesh;
    //             
    //             //check collision with undead
    //             Collider[] colliders = Physics.OverlapBox(meshCollider.bounds.center, meshCollider.bounds.extents, meshCollider.transform.rotation, Layers.UndeadUnit);
    //             if (colliders.Length > 0)
    //             {
    //                 Debug.Log("Hit Undead");
    //                 //거리가 제일 가까운 유닛을 찾는다.
    //                 float minDistance = float.MaxValue;
    //                 Unit targetUnit = null;
    //                 foreach (Collider collider in colliders)
    //                 {
    //                     float distance = Vector3.Distance(collider.transform.position, transform.position);
    //                     if (distance < minDistance)
    //                     {
    //                         minDistance = distance;
    //                         targetUnit = collider.GetComponent<Unit>();
    //                     }
    //                 }
    //                 if (targetUnit != null)
    //                 {
    //                     if (targetUnit.TryGetComponent(out FeedbackController feedback))
    //                     {
    //                         feedback.ChangeMaterialBasedOnIsSelected(true);
    //                     }
    //                 }
    //             }
    //         }
    //     }
    // }
}
