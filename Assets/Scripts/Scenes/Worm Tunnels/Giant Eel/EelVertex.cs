using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    public class EelVertex : MonoBehaviour
    {
        public float wiggleMax;
        public float wiggleTime;//time to go from 0 to wiggleMax
        
        int wiggleDirection = 1;//1 = up, -1 = down
        float wiggleTimer;//oscilates between -1 & 1
        VisualCurveGuidePoint vcgp;//will be childed to EelVertex, so that EelVertex.pos can be the neutral position

        public float WiggleTimer => wiggleTimer;

        private void Awake()
        {
            vcgp = GetComponentInChildren<VisualCurveGuidePoint>();
        }

        public void InitializeWiggle(int direction, float time)
        {
            wiggleDirection = direction;
            wiggleTimer = time;
        }

        public void UpdateWiggle(Vector2 leadBodyPosition, float leadWiggleTimer, HorizontalOrientation o, float dt)
        {
            var u = (leadBodyPosition - (Vector2)transform.position).normalized;
            var v = ((int)o) * u.CCWPerp();
            var z = 1 / Mathf.Sqrt(1 + leadWiggleTimer * leadWiggleTimer);
            //because sin'(x) = sin(x + pi/2), slope of the eel's curve at vertex[i]
            //should be vertex[i - 1].leadWiggletimer,
            //so tangent vector should be (z, leadWiggleTimer * z) (in the basis u,v, with z as above)

            if (wiggleTimer * wiggleDirection > wiggleTime)
            {
                wiggleDirection *= -1;
            }

            wiggleTimer += wiggleDirection * dt;
            vcgp.SetPoint((Vector2)transform.position + wiggleTimer * wiggleMax * v);
            vcgp.SetTangentDir(z * u + leadWiggleTimer * z * v);
        }
    }
}