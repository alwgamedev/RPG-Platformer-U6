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
                if (gameObject && cc?.Combatant != null && cc.Combatant.DestroyOnFinalizeDeath)
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

        //public void RecordBodyPieceOffsets()
        //{
        //    if (bodyPieceOffsets == null)
        //    {
        //        bodyPieceOffsets = new Vector3[mover.NumBodyPieces];
        //    }

        //    Vector3 d;

        //    for (int i = 0; i < mover.NumBodyPieces; i++)
        //    {
        //       d = mover.BodyPieces[i].transform.position - transform.position;
        //       if (mover.CurrentOrientation == HorizontalOrientation.left)
        //       {
        //           d.x *= -1;
        //       }
        //    }
        //}

        //public void RestoreBodyPieceOffsets()
        //{
        //    foreach (var b in mover.BodyPieces)
        //    {
        //        b.SetKinematic();
        //    }

        //    for (int i = 0; i < bodyPieceOffsets.Length; i++)
        //    {
        //        var d = bodyPieceOffsets[i];
        //        d.x *= (int)mover.CurrentOrientation;
        //        mover.BodyPieces[i].transform.position = transform.position + d;
        //    }

        //    foreach (var b in mover.BodyPieces)
        //    {
        //        b.bodyType = RigidbodyType2D.Dynamic;
        //    }
        //}

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