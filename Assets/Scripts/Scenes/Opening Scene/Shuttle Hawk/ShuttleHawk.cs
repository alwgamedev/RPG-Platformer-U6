using RPGPlatformer.Core;
using RPGPlatformer.Movement;
using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class ShuttleHawk : HybridFlyerPatroller
    {
        [SerializeField] float departureWaitTime = 30;
        [SerializeField] float inFlightMountGravity = 100;
        [SerializeField] float defaultMountGravity = 60;
        [SerializeField] float departureVerificationTime = 1;
        [SerializeField] float dismountVerificationTime = 5;

        float mountVerificationTimer = 0;
        bool readyForDeparture;
        bool playerIsMounted;

        MountableEntity[] mounts;

        protected override void Awake()
        {
            base.Awake();

            mounts = GetComponentsInChildren<MountableEntity>();
        }

        private void Start()
        {
            SetMountGravity(defaultMountGravity);

            MovementController.OnFlightEntry += () => SetMountGravity(inFlightMountGravity);
            MovementController.OnFlightExit += () => SetMountGravity(defaultMountGravity);
        }

        public void SetMountGravity(float gravity)
        {
            foreach (var mount in mounts)
            {
                mount.SetLocalGravity(gravity);
            }
        }

        public void PrepareForDeparture()
        {
            Trigger(typeof(AwaitingDeparture).Name);
            playerIsMounted = false;
            readyForDeparture = false;
        }

        public void HeadToDeparturePoint(Transform departurePoint)
        {
            BeginPatrol(NavigationMode.singleDestination, departurePoint.position);
        }

        public void AwaitingDepartureBehavior()
        {
            if (readyForDeparture && playerIsMounted)
            {
                mountVerificationTimer += Time.deltaTime;
                if (mountVerificationTimer > departureVerificationTime)
                {
                    //SubscribeMountedHandlers(false);//keep mounted handlers subscribed for shuttling!
                    mountVerificationTimer = 0;
                    Trigger(typeof(Shuttling).Name);
                }
                return;
            }

            PatrolBehavior();
        }

        public void ReadyForDeparture()
        {
            readyForDeparture = true;
            playerIsMounted = false;
            mountVerificationTimer = 0;

            BeginPatrol(NavigationMode.timedRest, departureWaitTime);
            SubscribeMountedHandlers(true);
        }

        public void DepartureTimeOut()
        {
            Trigger(typeof(Patrol).Name);
            if (!playerIsMounted)
            {
                SubscribeMountedHandlers(false);
            }
        }

        public void BeginFlightPath(LinkedListNode<PatrolPathWayPoint> startPoint, bool forwards = true)
        {
            BeginPatrol(forwards ? NavigationMode.pathForwards : NavigationMode.pathBackwards, startPoint);
            MovementController.BeginFlying();
        }

        public void ShuttlingBehavior()
        {
            if (IsFollowingFlightPath() && !MovementController.Flying)
            {
                MovementController.BeginFlying();
                //just in case e.g. player is naughty and blocks him while taking off
            }
            //check mode=pathforwards because he is still in Shuttling state while resting at final destination
            if (!playerIsMounted && PatrolNavigator.CurrentMode == NavigationMode.pathForwards)
            {
                mountVerificationTimer += Time.deltaTime;
                if (mountVerificationTimer > dismountVerificationTime)
                {
                    SubscribeMountedHandlers(false);
                    Trigger(typeof(ReturningToNest).Name);
                    return;
                }
            }

            PatrolBehavior();
        }

        private bool IsFollowingFlightPath()
        {
            return PatrolNavigator.CurrentMode == NavigationMode.pathForwards 
                || PatrolNavigator.CurrentMode == NavigationMode.pathBackwards;
        }

        public void ShuttleDestionationReached()
        {
            SubscribeMountedHandlers(false);
            BeginPatrol(NavigationMode.timedRest, departureWaitTime);
        }

        public void ReturningToNestBehavior()
        {
            if (IsFollowingFlightPath() && !MovementController.Flying)
            {
                MovementController.BeginFlying();
            }

            PatrolBehavior();
        }

        public void ReadyToReturnToNest()
        {
            Trigger(typeof(ReturningToNest).Name);
        }

        public void OnReturnedToNest()
        {
            Trigger(typeof(Patrol).Name);
        }

        private void MountedHandler(IMounter mounter)
        {
            if (mounter.transform == GlobalGameTools.Instance.PlayerTransform && !playerIsMounted)
            {
                playerIsMounted = true;
                mountVerificationTimer = 0;
                //SubscribeMountedHandler(false);
            }
        }

        private void DismountedHandler(IMounter mounter)
        {
            if (mounter.transform == GlobalGameTools.Instance.PlayerTransform && playerIsMounted)
            {
                playerIsMounted = false;
                mountVerificationTimer = 0;
            }
        }

        private void SubscribeMountedHandlers(bool val)
        {
            foreach (var mount in mounts)
            {
                if (val)
                {
                    mount.EnableTriggerStay(true);
                    mount.Mounted += MountedHandler;
                    mount.Dismounted += DismountedHandler;
                    mount.MountStay += MountedHandler;
                }
                else
                {
                    mount.EnableTriggerStay(false);
                    mount.Mounted -= MountedHandler;
                    mount.Dismounted -= DismountedHandler;
                    mount.MountStay -= MountedHandler;
                }
            }
        }
    }
}