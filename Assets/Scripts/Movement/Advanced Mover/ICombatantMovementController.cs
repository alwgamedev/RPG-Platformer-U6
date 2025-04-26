using UnityEngine;

namespace RPGPlatformer.Movement
{
    public interface ICombatantMovementController
    {
        public bool Moving { get; }
        public HorizontalOrientation CurrentOrientation { get; }

        public void FaceTarget(Transform target);

        public void FaceTarget(Vector3 target);

        public void SetRunning(bool val);

        public void OnDeath();

        public void OnRevival();
    }
}