using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class AIMovementController : AdvancedMovementController
    {
        [SerializeField] bool dontMoveOverDropOffs;
        [SerializeField] float maxPermissibleDropOffHeightFactor = 2;

        protected override void GroundedMoveAction(float input)
            //I am doing the drop off check here (rather than at the point where MoveInput is set)
            //so that orientation is accurate (without having to do an unecessary SetOrientation every time we
            //set MoveInput)
        {
            SetOrientation(input);

            if (dontMoveOverDropOffs && mover.DropOffInFront(maxPermissibleDropOffHeightFactor)) 
                return;
            //{
            //    //MoveInput = 0;//we may not want to do this because it triggers an event
            //    ////but for now it's harmless
            //    ////(just returning would have the same effect)
            //    return;
            //}

            mover.MoveGrounded();
        }
    }
}