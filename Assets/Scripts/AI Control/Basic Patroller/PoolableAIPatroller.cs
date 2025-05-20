using RPGPlatformer.Core;
using System;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class PoolableAIPatroller : PoolableObject
    {
        [SerializeField] protected bool snapSpawnPositionToGround = true;
        [SerializeField] protected Transform patrollerTransform;
        //for characters like pill bug or worm that don't have the patroller script on a child transform

        protected IAIPatrollerController controller;
        int groundLayer;

        protected virtual Vector3 Position => transform.position;

        protected virtual void Awake()
        {
            groundLayer = LayerMask.GetMask("Ground");
            controller = patrollerTransform ?
                patrollerTransform.GetComponent<IAIPatrollerController>()
                : GetComponent<IAIPatrollerController>();
        }

        public override void Configure(object parameters)
        {
            var p = parameters as PoolableAIPatrollerConfigurationParameters;

            if (p)
            {
                if (p.PatrolParameters)
                {
                    controller.DefaultPatrolParams = p.PatrolParameters.Content;
                }
                controller.Patroller.AIMovementController.LeftMovementBound = p.LeftMovementBound;
                controller.Patroller.AIMovementController.RightMovementBound = p.RightMovementBound;
            }

            if (controller is ICombatPatrollerController c)
            {
                c.CombatPatroller.CombatController.Combatant.DestroyOnFinalizeDeath = false;
                c.CombatPatroller.CombatController.Combatant.DeathFinalized += ReturnToPool;
                if (p)
                {
                    c.CombatPatroller.LeftAttackBound = p.LeftAttackBound;
                    c.CombatPatroller.RightAttackBound = p.RightAttackBound;
                }
            }
        }

        public override void BeforeSetActive()
        {
            if (snapSpawnPositionToGround)
            {
                CorrectSpawnPosition();
            }
        }

        public override void ResetPoolableObject()
        {
            if (controller is ICombatPatrollerController c)
            {
                if (c.CombatPatroller.CombatController.Combatant.Health.IsDead)
                {
                    c.CombatPatroller.CombatController.Combatant.Revive();
                }
            }
        }

        protected virtual void CorrectSpawnPosition()
        {
            //var o = controller.Patroller.AIMovementController.Mover.CenterPosition;
            //var h = 0.5f * controller.Patroller.AIMovementController.Mover.Height + spawnHeightBuffer;
            var r = Physics2D.Raycast(Position, -Vector2.up, Mathf.Infinity, groundLayer);

            if (r && r.distance > 0)
            {
                SetPosition(r.point);
                //as stupid as it looks, this is the only thing that works
                //(trying a more logical solution like setting position to ground pos + half height,
                //always starts him with a freefall (even if you use collider center etc.))
            }

            //if (!r) return;

            //if (r.distance <= 0)//our spawn position is below ground
            //{
            //    //if i remember correctly, this snaps to a vertex of the collider (for polygon collider)
            //    //which could be undesirable when there are few vertices nearby
            //    //we'll see what it does
            //    //to eradicate this issue you could just try to choose spawnMin and spawnMax points to be above 
            //    //ground max height so that we never spawn below ground
            //    Debug.Log("spawned under ground, moving to closest point on ground collider");
            //    transform.position = r.collider.ClosestPoint(r.point) + h * Vector2.up
            //        + (Vector2)(transform.position - o);
            //}
            //else if (r && r.distance > 0)
            //{
            //    Debug.DrawLine(o, r.point, Color.red, 5);
            //    Debug.Log($"pos before correction: {transform.position}");
            //    transform.position -= r.distance * Vector3.up;
            //    Debug.Log($"pos after: {transform.position}");
            //}
        }
    }
}