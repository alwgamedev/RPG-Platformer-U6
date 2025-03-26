using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class ShuttleHawk : HybridFlyerPatroller
    {
        [SerializeField] float departureWaitTime = 30;
        [SerializeField] float inFlightMountGravity = 100;
        [SerializeField] float defaultMountGravity = 60;

        bool readyForDeparture;
        bool playerHasMounted;

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
            playerHasMounted = false;
            readyForDeparture = false;
        }

        public void HeadToDeparturePoint(Transform departurePoint)
        {
            BeginPatrol(NavigationMode.singleDestination, departurePoint.position);
        }

        public void AwaitingDepartureBehavior()
        {
            if (readyForDeparture && playerHasMounted)
            {
                Trigger(typeof(Shuttling).Name);
                return;
            }

            PatrolBehavior();
        }

        public void ReadyForDeparture()
        {
            readyForDeparture = true;
            playerHasMounted = false;

            BeginPatrol(NavigationMode.timedRest, departureWaitTime);
            SubscribeMountedHandler(true);
        }

        public void DepartureTimeOut()
        {
            Trigger(typeof(Patrol).Name);
            if (!playerHasMounted)
            {
                SubscribeMountedHandler(false);
            }
        }

        public void BeginFlightPath(PatrolPath flightPath, bool forwards = true)
        {
            BeginPatrol(forwards ? NavigationMode.pathForwards : NavigationMode.pathBackwards, flightPath);
            MovementController.BeginFlying();
        }

        public void ShuttleDestionationReached()
        {
            BeginPatrol(NavigationMode.timedRest, departureWaitTime);
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
            if (mounter.CompareTag("Player"))
            {
                playerHasMounted = true;
                SubscribeMountedHandler(false);
            }
        }

        private void SubscribeMountedHandler(bool val)
        {
            foreach (var mount in mounts)
            {
                if (val)
                {
                    mount.EnableTriggerStay(true);
                    mount.Mounted += MountedHandler;
                    mount.MountStay += MountedHandler;
                }
                else
                {
                    mount.EnableTriggerStay(false);
                    mount.Mounted -= MountedHandler;
                    mount.MountStay -= MountedHandler;
                }
            }
        }
    }
}