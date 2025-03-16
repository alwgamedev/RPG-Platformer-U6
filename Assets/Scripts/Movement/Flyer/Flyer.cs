using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class Flyer : AdvancedMover
    {
        [SerializeField] float flightAcceleration;
        [SerializeField] float flightSpeed;
        [SerializeField] Vector2 takeOffForce;

        protected bool flying;

        public override bool Running
        {
            set
            {
                if (!flying)
                {
                    base.Running = value;
                }
            }
        }
    }
}