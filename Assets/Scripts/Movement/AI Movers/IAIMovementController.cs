using UnityEngine;

namespace RPGPlatformer.Movement
{
    public interface IAIMovementController : IMovementController
    {
        public Transform CurrentTarget { get; set; }
        public Vector2 MoveInput { get; set; }

        public bool DropOffAhead(HorizontalOrientation direction, out float distance);
    }
}