using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer
{
    [ExecuteAlways]
    public class VisualCurveGuide : MonoBehaviour
    {
        [SerializeField] bool drawGizmos;
        [SerializeField] VisualCurveGuidePoint[] guides;
        [SerializeField] int ikIterations;
        [SerializeField] float ikStrength = 0;
        [SerializeField] float ikToleranceSqrd = 0.01f;

        public Transform ikTarget;

        CurveRenderer curveRenderer;
        Vector3[] unitRays;
        float[] lengths;//storage for lengths in the IK algorithm

        private void Update()
        {
            if (guides == null) return;

            if (ikTarget && ikTarget.hasChanged)
            {
                UpdateRendererGuidePoints();
                return;
            }

            foreach (var guide in guides)
            {
                if (guide && guide.HasChanged())
                {
                    UpdateRendererGuidePoints();
                    break;
                }
            }
        }

        private void UpdateRendererGuidePoints()
        {
            if (ikStrength != 0 && ikTarget)
            {
                if (unitRays == null || unitRays.Length != guides.Length - 1)
                {
                    unitRays = new Vector3[guides.Length - 1];
                }
                if (lengths == null || lengths.Length != guides.Length - 1)
                {
                    lengths = new float[guides.Length - 1];
                }

                CurveGuideIKHelper.FABRIK(guides, unitRays, lengths, ikTarget.position, 
                    ikIterations, ikStrength, ikToleranceSqrd);
            }

            if (curveRenderer == null)
            {
                curveRenderer = GetComponent<CurveRenderer>();
            }
            if (curveRenderer != null)
            {
                curveRenderer.HandleGuidePointChanges(guides);
            }
            //let's see if this triggers curve renderer's validate, or we should call draw ourselves
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos || guides == null) return;

            int i = 0;
            while (i < guides.Length)
            {
                if (!guides[i] || !guides[i].Active())
                {
                    i++; 
                    continue;
                }

                guides[i].DrawGizmo();

                int j = i + 1;

                //find the next active node
                while (j < guides.Length)
                {
                    if (!guides[j] || !guides[j].Active())
                    {
                        j++;
                        continue;
                    }

                    Debug.DrawLine(guides[i].Point(), guides[j].Point(), Color.green);
                    break;
                }

                i = j;
            }
        }
    }
}