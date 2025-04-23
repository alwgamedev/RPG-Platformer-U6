using RPGPlatformer.Combat;
using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class PillBugCombatPatroller : GenericCombatPatroller<PillBugMovementController, AICombatController>
    {
        [SerializeField] float rollPursuitRange = 4;
        [SerializeField] float uncurlPursuitRange = .75f;

        protected override void Pursue(float distanceSquared)
        {
            if (!MovementController.Curled && distanceSquared > rollPursuitRange * rollPursuitRange)
            {
                MovementController.SetCurled(true);
            }
            else if (MovementController.Curled && distanceSquared < uncurlPursuitRange * uncurlPursuitRange)
            {
                MovementController.SetCurled(false);
            }

            base.Pursue(distanceSquared);
        }
    }
}
