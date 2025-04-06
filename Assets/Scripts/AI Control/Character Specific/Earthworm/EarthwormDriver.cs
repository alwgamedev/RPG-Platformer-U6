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
        [SerializeField] Transform wormholeAnchor;//idea: not childed to worm, but he can move it wherever he needs it

        AICombatController combatController;
        EarthwormMovementController movementController;
        Transform currentBodyAnchorPoint;

        //body anchors should have fixed local positions (i.e. not attached to guide points)
        Vector3 BodyAnchorOffset => currentBodyAnchorPoint.position - transform.position;
        //AnchoredPosition: set transform.position equal to this to line up with wormhole correctly
        Vector3 AnchoredPosition => wormholeAnchor.position - BodyAnchorOffset;

        private void Awake()
        {
            combatController = GetComponent<AICombatController>();
            movementController = GetComponent<EarthwormMovementController>();
        }

        private void Start()
        {
            combatController.currentTarget = GlobalGameTools.Player.Combatant.Health;
            //so that combat controller will GetAimPosition & FaceAimPosition correctly during combat
        }


        //STATE BEHAVIORS

        public void AboveGroundBehavior()
        {
            //scan for target and trigger pursuit if out of range
            //(pursuit = retreat underground and move to new wormhole closer to player)
        }


        //COMBAT

        public void StartAttacking()
        {

        }

        public void StopAttacking()
        {

        }

        public void FacePlayer()
        {
            combatController.FaceAimPosition();
        }


        //EMERGE & RETREAT

        public async Task Retreat(CancellationToken token)
        {
            SetBodyAnchor(false);
            await MoveToAnchorPosition(token);
        }

        public async Task Emerge(CancellationToken token)
        {
            SetBodyAnchor(true);
            await MoveToAnchorPosition(token);
        }


        //POSITIONING

        public void SetWormholePosition(Vector3 position)
        {
            wormholeAnchor.position = position;
        }

        public void GoToWormhole()
        {
            movementController.GoTo(wormholeAnchor.position - BodyAnchorOffset);
        }

        public void SetBodyAnchor(bool aboveGround)
        {
            currentBodyAnchorPoint = aboveGround ? aboveGroundAnchor : underGroundAnchor;
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
                movementController.BeginMoveTowards(AnchoredPosition);
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
