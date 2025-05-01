using UnityEngine;

namespace RPGPlatformer.Movement
{
    public interface IAIMovementController : IMovementController
    {
        public Transform CurrentTarget { get; set; }
        public Vector3 MoveInput { get; }

        //public void SetMoveInput(Vector3 moveInput);
        public bool DropOffAhead(HorizontalOrientation direction, out float distance);
    }
}