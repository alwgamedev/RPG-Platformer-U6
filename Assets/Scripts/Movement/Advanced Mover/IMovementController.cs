using UnityEngine;

namespace RPGPlatformer.Movement
{
    public interface IMovementController : IMounter
    {
        public bool Moving { get; }
        public bool Jumping { get; }
        public bool Freefalling { get; }
        public Vector2 RelativeVelocity { get; }
        //public Rigidbody2D Rigidbody { get; }
        public HorizontalOrientation CurrentOrientation { get; }
        public IMover Mover { get; }

        public void MoveTowards(Vector2 point);
        public void MoveAwayFrom(Vector2 point);
        public void FaceTarget(Transform target);
        public void FaceTarget(Vector3 target);
        public void SoftStop();
        public void HardStop(bool maintainVerticalVelocity = true);
        public void OnDeath();
        public void OnRevival();
    }
}