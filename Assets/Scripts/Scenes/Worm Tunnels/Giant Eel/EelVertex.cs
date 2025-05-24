using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    public class EelVertex : MonoBehaviour
    {
        public float wiggleMax;
        public float wiggleSpeed;
        
        int wiggleDirection;//1 = up, -1 = down
        float wigglePosition;
        VisualCurveGuidePoint vcgp;//will be childed to EelVertex, so that EelVertex.pos can be the neutral position

        private void Awake()
        {
            vcgp = GetComponentInChildren<VisualCurveGuidePoint>();
        }

        public void InitializeWiggleDirection(int o)
        {
            wiggleDirection = o;
        }

        //u = body direction (normalized); wiggle will be perpendicular to u
        //actually u might be direction between two adjecent vertices (v[i-1] & v[i+1])
        public void UpdateWiggle(Vector2 u, HorizontalOrientation o, float dt)
        {
            if (wigglePosition * wiggleDirection > wiggleMax)
            {
                wiggleDirection *= -1;
            }

            wigglePosition += wiggleDirection * wiggleSpeed * dt;
            vcgp.SetPoint((Vector2)transform.position + ((int)o) * wigglePosition * u.CCWPerp());
            vcgp.SetTangentDir(-u);
            //we can either use vcg "tangent weight" to uniformly scale the tangents
            //or scale them individually
            //actually a good idea might be to scale tangent with or inversely to wigglePosition
            //(although this could be unnecessary overkill)
        }
    }
}