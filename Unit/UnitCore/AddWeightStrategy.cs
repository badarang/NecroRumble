
using MoreMountains.Feedbacks;
using MoreMountains.Feel;
using UnityEngine;

public partial class Unit
{
    
    public abstract class AddWeightBehavior {
        public abstract  void AddWeight(Unit unit_, Collider2D[] detectThings_);
    }

    public class BaseAddAttackWeightBehavior_U : AddWeightBehavior{
        public override void AddWeight(Unit unit_, Collider2D[] detectThings_)
        {
            GameObject prevAttackTarget = unit_.AttackTarget;
            unit_.AttackTarget = null;
            foreach (var detectThing in detectThings_)
            {
                if(detectThing == null) break;
                if(detectThing == unit_._collider) continue;

                float dist = unit_._collider.Distance(detectThing).distance;
                if(dist < 0) dist = 0;

                if(IsExistInLayerMask(detectThing.gameObject.layer, unit_.targetLayerMask)){
                    float aggroWeight = 0;
                    Unit enemy = detectThing.GetComponent<Unit>();
                    if(enemy.IsDead || enemy.IsReviving || enemy.GetRemainAggroNum() <= 0) continue; 

                    // // 나를 치는 유닛 우선 타겟팅
                    // if(enemy.AttackTarget == unit_.gameObject){
                    //     unit_._attackWeightList.Clear();
                    //     unit_.AddAttackWeightOne(enemy.gameObject, float.MaxValue);
                    //     break;
                    // }

                    // Add distance score
                    if(prevAttackTarget == enemy.gameObject && dist <= unit_.instanceStats.FinalAttackRange){
                        aggroWeight += 100;
                    }
                    else{
                        aggroWeight += (1 - dist / unit_.DetectRadius) * 100;
                    }
                    // Add inertia bonus
                    if(prevAttackTarget == enemy.gameObject){
                        aggroWeight += (100 * g_aggroDistByInertia_u / unit_.DetectRadius) * unit_._aggroInertiaWeight;
                    }
                    // Add commander bonus
                    if(enemy.IsCommander){
                        aggroWeight += (100 * g_aggroDistByCommander_u / unit_.DetectRadius) * unit_._aggroCommanderWeight;
                    }
                    // Add rangeDealer bonus
                    if(enemy._isRangedDealer){
                        aggroWeight += (100 * g_aggroDistByRangedDealer_u / unit_.DetectRadius) * unit_._aggroRangedDealerWeight;
                    }
                    
                    unit_.AddAttackWeightOne(detectThing.gameObject, aggroWeight);
                }
            }
        }
    }
    public class BaseAddMoveWeightBehavior : AddWeightBehavior{
        public override void AddWeight(Unit unit_, Collider2D[] detectThings_)
        {
            
            foreach (var thing in detectThings_)
            {
                if(thing == null) break;
                if(thing == unit_._collider) continue;

                float dist = unit_._collider.Distance(thing).distance;
                if(dist < 0) dist = 0;

                if(thing.gameObject.layer == unit_.gameObject.layer){
                    if(dist <= unit_._avoidRadius){
                        // avoid friend
                        unit_.AddAvoidWeight(thing.transform.position, dist, unit_._avoidRadius, unit_._avoidMaxWeight, unit_._avoidNoiseRange, 75);
                    }
                }
                else if(thing.gameObject.layer == Layers.Obstacle){
                    float obstacleRadius = 0.8f;
                    if(dist <= obstacleRadius){
                        // avoid obstacle
                        unit_.AddAvoidWeight(thing.transform.position, dist, obstacleRadius, 1.5f, 30, 60);
                    }
                }
                else if(IsExistInLayerMask(thing.gameObject.layer, unit_.targetLayerMask)){ 
                    if(dist <= unit_._avoidRadius){
                        unit_.AddAvoidWeight(thing.transform.position, dist, unit_._avoidRadius, 1f, unit_._avoidNoiseRange, 75);
                        // unit_.AddAvoidWeight(thing.transform.position, dist, unit_._avoidRadius, unit_._avoidMaxWeight, unit_._avoidNoiseRange, 75);
                    }
                // else if(unit_.IsExistInLayerMask(thing.gameObject.layer, unit_.targetLayerMask)){ 
                //     if(dist <= unit_.Info.AttackRange){
                //         // back in attack radius
                //         unit_.AddWeightTargetInnerAttackRange(-unit_.AIPathUpdate(), dist, unit_._targetWeightExponent, unit_._targetMaxWeight);
                //     }
                // }
            }
        }
    }
    }
    public class BaseAddAttackWeightBehavior_H : AddWeightBehavior{
        public override void AddWeight(Unit unit_, Collider2D[] detectThings_)
        {
            // Add aggroWeight
            GameObject prevAttackTarget = unit_.AttackTarget;
            unit_.AttackTarget = null;
            
            foreach (var detectThing in detectThings_)
            {
                if(detectThing == null) break;
                if(detectThing == unit_._collider) continue;

                float dist = unit_._collider.Distance(detectThing).distance;
                if(dist < 0) dist = 0;

                if(IsExistInLayerMask(detectThing.gameObject.layer, unit_.targetLayerMask)){
                    float aggroWeight = 0;
                    if(detectThing.gameObject.layer == Layers.Player){
                        // Add distance score
                        if(prevAttackTarget == detectThing.gameObject && dist <= unit_.instanceStats.FinalAttackRange){
                            aggroWeight += 100;
                        }
                        else{
                            aggroWeight += (1 - dist / unit_.DetectRadius) * 100;
                        }
                        // Add inertia bonus
                        if(prevAttackTarget == detectThing.gameObject){
                            aggroWeight += (100 * g_aggroDistByInertia_h / unit_.DetectRadius) * unit_._aggroInertiaWeight;
                        }
                        // Add commander bonus
                        aggroWeight += (100 * g_aggroDistByCommander_h / unit_.DetectRadius) * unit_._aggroCommanderWeight;
                        // Add presence bonus
                        aggroWeight += (100 * g_aggroDistByPresence_h * g_aggroRemainByPlayerPresence_h / unit_.DetectRadius) * unit_._aggroPresenceWeight_h;
                    }
                    else{
                        Unit enemy = detectThing.GetComponent<Unit>();
                        if(enemy.IsDead || enemy.IsReviving || enemy.GetRemainAggroNum() <= 0) continue; 

                        // Add distance score
                        if(prevAttackTarget == enemy.gameObject && dist <= unit_.instanceStats.FinalAttackRange){
                            aggroWeight += 100;
                        }
                        else{
                            aggroWeight += (1 - dist / unit_.DetectRadius) * 100;
                        }
                        // Add inertia bonus
                        if(prevAttackTarget == enemy.gameObject){
                            aggroWeight += (100 * g_aggroDistByInertia_h / unit_.DetectRadius) * unit_._aggroInertiaWeight;
                        }
                        // Add commander bonus
                        if(enemy.IsCommander){
                            aggroWeight += (100 * g_aggroDistByCommander_h / unit_.DetectRadius) * unit_._aggroCommanderWeight;
                        }
                        // Add ranged dealer bonus
                        if(enemy._isRangedDealer){
                            aggroWeight += (100 * g_aggroDistByRangedDealer_h / unit_.DetectRadius) * unit_._aggroRangedDealerWeight;
                        }
                        // Add presence bonus
                        aggroWeight += (100 * g_aggroDistByPresence_h * enemy.GetRemainAggroNum() / unit_.DetectRadius) * unit_._aggroPresenceWeight_h;
                    }
                    unit_.AddAttackWeightOne(detectThing.gameObject, aggroWeight);
                }
            }
        }
    }
    
    public class PriestAddAttackWeightBehavior_H : AddWeightBehavior{
         public override void AddWeight(Unit unit_, Collider2D[] detectThings_)
        {
            // Add aggroWeight
            GameObject prevAttackTarget = unit_.AttackTarget;
            unit_.AttackTarget = null;
            
            foreach (var detectThing in detectThings_)
            {
                if(detectThing == null) break;
                if(detectThing == unit_._collider) continue;

                float dist = unit_._collider.Distance(detectThing).distance;
                if(dist < 0) dist = 0;

                if(IsExistInLayerMask(detectThing.gameObject.layer, unit_.targetLayerMask)){
                    float aggroWeight = 0;
                    if(detectThing.gameObject.layer != Layers.Player){
                        Unit enemy = detectThing.GetComponent<Unit>();
                        if(enemy.IsDead || enemy.IsReviving || enemy.GetRemainAggroNum() <= 0 || enemy.instanceStats.FinalMaxHp == enemy.CurrentHp) continue; 

                        // Add distance score
                        if(prevAttackTarget == enemy.gameObject && dist <= unit_.instanceStats.FinalAttackRange){
                            aggroWeight += 100;
                        }
                        else{
                            aggroWeight += (1 - dist / unit_.DetectRadius) * 100;
                        }
                        // Add HP bonus
                        aggroWeight += (100 * g_aggroDistByHP_h / unit_.DetectRadius) * ((enemy.instanceStats.FinalMaxHp - enemy.CurrentHp) / enemy.instanceStats.FinalMaxHp);
                        // Add commander bonus
                        if(enemy.IsCommander){
                            aggroWeight += (100 * g_aggroDistByCommander_h / unit_.DetectRadius) * unit_._aggroCommanderWeight;
                        }
                        // Add ranged dealer bonus
                        if(enemy._isRangedDealer){
                            aggroWeight += (100 * g_aggroDistByRangedDealer_h / unit_.DetectRadius) * unit_._aggroRangedDealerWeight;
                        }
                        // Add presence bonus
                        aggroWeight += (100 * g_aggroDistByPresence_h * enemy.GetRemainAggroNum() / unit_.DetectRadius) * unit_._aggroPresenceWeight_h;
                    }
                    unit_.AddAttackWeightOne(detectThing.gameObject, aggroWeight);
                }
            }
        }
    }
    public class PriestAddMoveWeightBehavior_H : AddWeightBehavior{
        public override void AddWeight(Unit unit_, Collider2D[] detectThings_)
        {
            
            foreach (var thing in detectThings_)
            {
                if(thing == null) break;
                if(thing == unit_._collider) continue;

                float dist = unit_._collider.Distance(thing).distance;
                if(dist < 0) dist = 0;

                if(thing.gameObject.layer == unit_.gameObject.layer){
                    if(dist <= unit_._avoidRadius){
                        // avoid friend
                        unit_.AddAvoidWeight(thing.transform.position, dist, unit_._avoidRadius, unit_._avoidMaxWeight, unit_._avoidNoiseRange, 75);
                    }
                }
                else if(thing.gameObject.layer == Layers.Obstacle){
                    float obstacleRadius = 0.8f;
                    if(dist <= obstacleRadius){
                        // avoid obstacle
                        unit_.AddAvoidWeight(thing.transform.position, dist, obstacleRadius, 1.5f, 30, 60);
                    }
                }
                else if(!IsExistInLayerMask(thing.gameObject.layer, unit_.targetLayerMask)){ 
                    if(dist <= unit_.instanceStats.FinalAttackRange / 2){
                        // back in attack radius
                        // unit_.AddWeightTargetInnerAttackRange(-unit_.AIPathUpdate(), dist, unit_._targetWeightExponent, unit_._targetMaxWeight * 0.8f);
                        unit_.AddWeightTargetInnerAttackRange(-unit_.AIPathUpdate(), dist, unit_._targetWeightExponent, unit_._targetMaxWeight);
                    }
                }
            }
        }
    }
    public class ArcherManAddMoveWeightBehavior_U : AddWeightBehavior{
        public override void AddWeight(Unit unit_, Collider2D[] detectThings_)
        {
            
            foreach (var thing in detectThings_)
            {
                if(thing == null) break;
                if(thing == unit_._collider) continue;

                float dist = unit_._collider.Distance(thing).distance;
                if(dist < 0) dist = 0;

                if(thing.gameObject.layer == unit_.gameObject.layer){
                    if(dist <= unit_._avoidRadius){
                        // avoid friend
                        unit_.AddAvoidWeight(thing.transform.position, dist, unit_._avoidRadius, unit_._avoidMaxWeight, unit_._avoidNoiseRange, 75);
                    }
                }
                else if(thing.gameObject.layer == Layers.Obstacle){
                    float obstacleRadius = 0.8f;
                    if(dist <= obstacleRadius){
                        // avoid obstacle
                        unit_.AddAvoidWeight(thing.transform.position, dist, obstacleRadius, 1.5f, 30, 60);
                    }
                }
                else if(IsExistInLayerMask(thing.gameObject.layer, unit_.targetLayerMask)){ 
                    if(dist <= unit_.instanceStats.FinalAttackRange){
                        // back in attack radius
                        unit_.AddWeightTargetInnerAttackRange(-unit_.AIPathUpdate(), dist, unit_._targetWeightExponent, unit_._targetMaxWeight);
                    }
                }
            }
        }
    }
    
    public class AssassinAttackWeightBehavior_U : AddWeightBehavior{
        public override void AddWeight(Unit unit_, Collider2D[] detectThings_)
        {
            GameObject prevAttackTarget = unit_.AttackTarget;
            unit_.AttackTarget = null;
            foreach (var detectThing in detectThings_)
            {
                if(detectThing == null) break;
                if(detectThing == unit_._collider) continue;

                float dist = unit_._collider.Distance(detectThing).distance;
                if(dist < 0) dist = 0;

                if(IsExistInLayerMask(detectThing.gameObject.layer, unit_.targetLayerMask)){
                    float aggroWeight = 0;
                    Unit enemy = detectThing.GetComponent<Unit>();
                    if(enemy.IsDead || enemy.IsReviving || enemy.GetRemainAggroNum() <= 0) continue; 

                    // // 나를 치는 유닛 우선 타겟팅
                    // if(enemy.AttackTarget == unit_.gameObject){
                    //     unit_._attackWeightList.Clear();
                    //     unit_.AddAttackWeightOne(enemy.gameObject, float.MaxValue);
                    //     break;
                    // }

                    // Add distance score
                    if(prevAttackTarget == enemy.gameObject && dist <= unit_.instanceStats.FinalAttackRange){
                        aggroWeight += 100;
                    }
                    else{
                        aggroWeight += (1 - dist / unit_.DetectRadius) * 100;
                    }
                    // Add inertia bonus
                    if(prevAttackTarget == enemy.gameObject){
                        aggroWeight += (100 * g_aggroDistByInertia_u / unit_.DetectRadius) * unit_._aggroInertiaWeight;
                    }
                    // Add commander bonus
                    if(enemy.IsCommander){
                        aggroWeight += (100 * g_aggroDistByCommander_u / unit_.DetectRadius) * unit_._aggroCommanderWeight;
                    }
                    // Add rangeDealer bonus
                    if(enemy._isRangedDealer){
                        aggroWeight += (100 * g_aggroDistByRangedDealer_u / unit_.DetectRadius) * 1f; //TODO: 인간 암살자는 SO 수치를 사용하고 언데드는 하드코딩. 급해서 ㅈㅅ
                    }
                    
                    unit_.AddAttackWeightOne(detectThing.gameObject, aggroWeight);
                }
            }
        }
    }
}