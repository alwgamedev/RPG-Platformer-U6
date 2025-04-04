using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer
{
    [ExecuteAlways]
    public class CurveVisualizer : MonoBehaviour
    {
        [SerializeField] bool drawGizmos;
        [SerializeField] VisualCurveGuidePoint[] guides;

        CurveRenderer curveRenderer;

        private void Update()
        {
            if (guides == null) return;

            foreach (var guide in guides)
            {
                if (guide && guide.HasChanged())
                {
                    RedrawCurve();
                    break;
                }
            }
        }

        private void RedrawCurve()
        {
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