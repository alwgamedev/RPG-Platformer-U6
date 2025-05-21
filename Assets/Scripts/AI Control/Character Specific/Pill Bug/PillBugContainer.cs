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

        //Vector3[] bodyPieceOffsets;

        public PillBugMover Mover => mover;

        private void Awake()
        {
            cc = GetComponentInChildren<AICombatController>();
            mover = GetComponentInChildren<PillBugMover>(); 
            healthCanvas = GetComponentInChildren<CombatantHealthBarCanvas>();
        }

        private void Start()
        {
            cc.Combatant.DeathFinalized += () =>
            {
                if (gameObject && cc && cc.Combatant != null && cc.Combatant.DestroyOnFinalizeDeath)
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

        public void PositionHealthBarCanvas()
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