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
        VisualCurveGuide curveGuide;
        Transform currentBodyAnchorPoint;

        //body anchors should have fixed local positions (i.e. not attached to guide points)
        Vector3 BodyAnchorOffset => currentBodyAnchorPoint.position - transform.position;
        //AnchoredPosition: set transform.position equal to this to line up with wormhole correctly
        Vector3 AnchoredPosition => wormholeAnchor.position - BodyAnchorOffset;

        private void Awake()
        {
            combatController = GetComponent<AICombatController>();
            movementController = GetComponent<EarthwormMovementController>();
            curveGuide = GetComponent<VisualCurveGuide>();
        }

        private void Start()
        {
            combatController.currentTarget = GlobalGameTools.Player.Combatant.Health;
            curveGuide.ikTarget = GlobalGameTools.PlayerTransform;
            curveGuide.ikEnabled = false;
            //so that combat controller will GetAimPosition & FaceAimPosition correctly during combat
        }


        //SETTINGS

        //think I will hook these up to animation events
        public void EnableIK()
        {
            curveGuide.ikEnabled = true;
        }

        public void DisableIK()
        {
            curveGuide.ikEnabled = false;
        }


        //STATE BEHAVIORS

        public void AboveGroundBehavior()
        {
            //A. scan for target and trigger pursuit(*) if out of range
            //B. run above ground timer and escape after certain time (or if health falls too low
                //and having completed certain number of "phases" (emergences) yet
            //then player has to go find the new wormhole location...
            //so there should be some indication at least of which direction the worm is going;
            //would be awesome if we can make a shader to produce a rumble on the surface of the ground
            //that indicates the worm's tunneling direction
                //a) create a shader thate creates semi-random, scalable bumps and ridges along (UV) edge of a sprite
                //b) add settings to apply the shader only over a certain world position area (which we can "animate"
                //to indicate tunneling)

            //(*) pursuit = retreat underground and move to new wormhole closer to player
            //maybe only pursue if player is out of a range for say 1.5sec
            //(and/or if player is a certain threshold outside attack range)
            //(don't want to trigger it while player is running away from an attack)
        }

        public void DormantBehavior()
        {
            //if not dead, wait for player to trigger 
        }

        public void PursuitBehavior()
        {
            //pick wormhole location closer to player
            //emerge at new wormhole
        }


        //COMBAT

        //TO-DO: randomize attack speed & give it custom (possibly randomized) ability cycle
        //+ figure out what to do with collision during slam attack
        //(ideally disable collider but apply force to player (sending them up and backwards) and briely stun them

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
