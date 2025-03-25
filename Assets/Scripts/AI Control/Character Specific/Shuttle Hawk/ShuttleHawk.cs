using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class ShuttleHawk : HybridFlyerPatroller
    {
        [SerializeField] float departureWaitTime = 30;
        [SerializeField] float inFlightMountGravity = 100;
        [SerializeField] float defaultMountGravity = 60;

        float waitingForDepartureEntryTime;
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

            if (readyForDeparture && Time.time - waitingForDepartureEntryTime > departureWaitTime)
            {
                Trigger(typeof(Patrol).Name);
                if (!playerHasMounted)
                {
                    SubscribeMountedHandler(false);
                };
                return;
            }

            PatrolBehavior();
        }

        public void ReadyForDeparture()
        {
            readyForDeparture = true;
            playerHasMounted = false;
            waitingForDepartureEntryTime = Time.time;

            SubscribeMountedHandler(true);
        }

        public void ShuttleDeparture(PatrolPath flightPath)
        {
            BeginPatrol(NavigationMode.pathForwards, flightPath);
            MovementController.BeginFlying();
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