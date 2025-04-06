using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using System.Threading;
using System.Threading.Tasks;
using System;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    [RequireComponent(typeof(AICombatController))]
    [RequireComponent(typeof(EarthwormMovementController))]
    public class EarthwormDriver : StateDriver
    {
        [SerializeField] Transform underGroundAnchor;
        [SerializeField] Transform aboveGroundAnchor;
        [SerializeField] Transform testWormhole;

        AICombatController combatController;
        EarthwormMovementController movementController;
        Transform currentWormholeAnchorPoint;
        Transform currentBodyAnchorPoint;

        //anchors should have fixed local positions (i.e. not attached to guide points)
        Vector3 BodyAnchorOffset => currentBodyAnchorPoint.position - transform.position;

        //public event Action OnDisabled;

        private void Awake()
        {
            combatController = GetComponent<AICombatController>();
            movementController = GetComponent<EarthwormMovementController>();

            currentBodyAnchorPoint = underGroundAnchor;
        }

        public void SetAnchor(bool aboveGround)
        {
            currentBodyAnchorPoint = aboveGround ? aboveGroundAnchor : underGroundAnchor;
        }

        public void GoToWormhole(Transform anchor)
        {
            currentWormholeAnchorPoint = anchor;
            movementController.GoTo(anchor.position - BodyAnchorOffset);
        }

        public async Task MoveToAnchorPosition(CancellationToken token)
        {
            TaskCompletionSource<object> tcs = new();
            using var reg = token.Register(Cancel);

            void Complete()
            {
                tcs.TrySetResult(null);
            }

            void Cancel()
            {
                tcs.TrySetCanceled();
            }

            try
            {
                movementController.DestinationReached += Complete;
                combatController.OnDeath += Cancel;
                movementController.BeginMoveTowards(currentWormholeAnchorPoint.position - BodyAnchorOffset);
                await tcs.Task;
            }
            catch
            {
                return;
            }
            finally
            {
                movementController.DestinationReached -= Complete;
                combatController.OnDeath -= Cancel;
            }
        }
    }
}
