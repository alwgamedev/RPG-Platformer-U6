using RPGPlatformer.Core;
using System;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class PoolableAIPatroller : PoolableObject
    {
        [SerializeField] bool snapSpawnPositionToGround = true;
        [SerializeField] float spawnHeightBuffer = 0.05f;

        IAIPatrollerController controller;
        int groundLayer;

        private void Awake()
        {
            groundLayer = LayerMask.GetMask("Ground");
            controller = GetComponent<IAIPatrollerController>();
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

        private void CorrectSpawnPosition()
        {
            var o = controller.Patroller.AIMovementController.Mover.CenterPosition;
            var h = controller.Patroller.AIMovementController.Mover.Height + spawnHeightBuffer;
            var r = Physics2D.Raycast(o, -Vector2.up, Mathf.Infinity, groundLayer);

            if (!r) return;

            if (r.distance < h)//our spawn position is below ground
            {
                //if i remember correctly, this snaps to a vertex of the collider (for polygon collider)
                //which could be undesirable when there are few vertices nearby
                //we'll see what it does
                //to eradicate this issue you could just try to choose spawnMin and spawnMax points to be above 
                //ground max height so that we never spawn below ground
                Debug.Log("spawned under ground, moving to closest point on ground collider");
                transform.position = r.collider.ClosestPoint(r.point) + h * Vector2.up
                    + (Vector2)(transform.position - o);
            }
            else if (r && r.distance > h)
            {
                Debug.Log("spawned too high, moving down to ground");
                transform.position = r.point + h * Vector2.up + (Vector2)(transform.position - o);
            }
        }
    }
}