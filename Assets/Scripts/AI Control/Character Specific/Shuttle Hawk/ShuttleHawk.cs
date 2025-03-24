using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class ShuttleHawk : HybridFlyerPatroller
    {
        [SerializeField] float departureWaitTime = 30;

        float waitingForDepartureEntryTime;
        bool readyForDeparture;
        bool playerHasMounted;

        public void PrepareForDeparture()
        {
            Trigger(typeof(AwaitingDeparture).Name);
            playerHasMounted = false;
            readyForDeparture = false;
        }

        public void HeadToDeparturePoint(Transform departurePoint)
        {
            BeginPatrol(NavigationMode.singleDestination, (Vector2)departurePoint.position);
        }

        public void AwaitingDepartureBehavior()
        {
            if (readyForDeparture && playerHasMounted)
            {
                Debug.Log("take off!");
                //return;
            }

            if (readyForDeparture && Time.time - waitingForDepartureEntryTime > departureWaitTime)
            {
                TriggerPatrol();
                return;
            }

            PatrolBehavior();
        }

        public void ReadyForDeparture()
        {
            Debug.Log("readying for departure");
            readyForDeparture = true;
            playerHasMounted = false;
            waitingForDepartureEntryTime = Time.time;
        }
    }
}