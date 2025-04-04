using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer
{
    [ExecuteAlways]
    public class CurveVisualizer : MonoBehaviour
    {
        [SerializeField] VisualCurvePoint[] guides;

        CurveRenderer curveRenderer;

        private void OnEnable()
        {
            VisualCurvePoint.CurvePointMoved -= RedrawCurve;//just in case we are already subbed
            VisualCurvePoint.CurvePointMoved += RedrawCurve;
        }

        private void OnDrawGizmos()
        {
            if (guides == null) return;

            for (int i = 0; i < guides.Length; i++)
            {
                if (guides[i] == null || guides[i].Data == null) continue;
                var guide = guides[i].Data;

                //TO-DO: do this in OnDrawGizmos, or wherever you're supposed to do it
                if (guide?.Item1 != null)
                {
                    if (guide.Item2 != null)
                    {
                        Debug.DrawLine(guide.Item1.position, guide.Item2.position, Color.yellow);
                    }

                    if (i < guides.Length - 1 && guides[i + 1] != null && guides[i + 1].Data?.Item1 != null)
                    {
                        Debug.DrawLine(guide.Item1.position, guides[i + 1].Data.Item1.position, Color.green);
                    }
                }
            }
        }

        private void OnValidate()
        {
            RedrawCurve();
        }

        private void RedrawCurve()
        {
            if (curveRenderer != null)
            {
                UpdateCurveRenderer();
                curveRenderer.RedrawCurve();
            }
        }

        private void UpdateCurveRenderer()
        {
            if (curveRenderer == null)
            {
                curveRenderer = GetComponent<CurveRenderer>();
            }
            if (curveRenderer != null)
            {
                curveRenderer.SetPoints(guides);
                //curveRenderer.RedrawCurve();
            }
            //let's see if this triggers curve renderer's validate, or we should call draw ourselves
        }

        private void OnDisable()
        {
            VisualCurvePoint.CurvePointMoved -= RedrawCurve;
        }
    }
}