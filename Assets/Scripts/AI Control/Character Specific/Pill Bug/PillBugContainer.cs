using RPGPlatformer.Combat;
using RPGPlatformer.Movement;
using RPGPlatformer.UI;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    //because you can't call methods on children in an animation event
    public class PillBugContainer : MonoBehaviour
    {
        AICombatController cc;
        PillBugMover mover;
        CombatantHealthBarCanvas healthCanvas;

        private void Start()
        {
            cc = GetComponentInChildren<AICombatController>();
            mover = GetComponentInChildren<PillBugMover>();
            healthCanvas = GetComponentInChildren<CombatantHealthBarCanvas>();

            cc.Combatant.DeathFinalized += () =>
            {
                if (gameObject)
                {
                    Destroy(gameObject);
                }
            };
        }

        private void Update()
        {
            PositionHealthBarCanvas();
        }

        public void ExecuteStoredAction()
        {
            cc.ExecuteStoredAction();
        }

        private void PositionHealthBarCanvas()
        {
            if (healthCanvas && mover && cc != null)
            {
                healthCanvas.transform.position = mover.Curled ?
                mover.Axle.transform.position
                : cc.transform.position;
            }
        }
    }
}