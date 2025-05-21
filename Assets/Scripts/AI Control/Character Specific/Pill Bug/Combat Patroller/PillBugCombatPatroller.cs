using RPGPlatformer.Combat;
using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class PillBugCombatPatroller : GenericCombatPatroller<PillBugMovementController, AICombatController>
    {
        [SerializeField] float rollPursuitRange = 4;
        [SerializeField] float uncurlPursuitRange = .75f;

        protected override void Pursue(float distanceSquared, float tolerance)
        {
            if (!MovementController.Curled && !InRange(distanceSquared, rollPursuitRange, tolerance))
            {
                MovementController.SetCurled(true);
            }
            else if (MovementController.Curled && InRange(distanceSquared, uncurlPursuitRange, tolerance))
            {
                MovementController.SetCurled(false);
            }

            base.Pursue(distanceSquared, tolerance);
        }
    }
}
