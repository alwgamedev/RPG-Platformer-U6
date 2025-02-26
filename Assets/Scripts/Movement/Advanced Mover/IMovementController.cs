using UnityEngine;

namespace RPGPlatformer.Movement
{
    public interface IMovementController
    {
        public bool Moving { get; }
        public Rigidbody2D Rigidbody { get; }
        public HorizontalOrientation CurrentOrientation { get; }
        public IMover Mover { get; }

        public void MoveTowards(Vector2 point);
        public void MoveAwayFrom(Vector2 point);
        public void FaceTowards(Transform target);
        public void FaceTarget(Vector2 target);
        public void OnDeath();
        public void OnRevival();
    }
}