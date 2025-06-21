using UnityEngine;

namespace RPGPlatformer.Movement
{
    public interface IAIMovementController : IMovementController
    {
        public Transform CurrentTarget { get; set; }
        public Vector3 MoveInput { get; }
        public Transform LeftMovementBound { get; set; }
        public Transform RightMovementBound { get; set; }

        //public void SetMoveInput(Vector3 moveInput);
        public bool DropOffAhead(HorizontalOrientation direction, out float distance);

        public bool CanMove(Vector3 moveInput);
    }
}