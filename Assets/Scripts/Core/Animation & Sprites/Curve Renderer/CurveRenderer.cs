using UnityEngine;

namespace RPGPlatformer.Core
{
    [ExecuteAlways]
    public class CurveRenderer : MonoBehaviour
    {
        [Min(0)][SerializeField] int numPointsDrawn = 30;
        [SerializeField] CurvePoint[] pointData;
        //note: numPointsDrawn is independent of pointData.Length.
        //pointData determines the curve we want to draw and numPointsDrawn
        //is number of points drawn along that curve.

        LineRenderer lineRenderer;

        private void OnValidate()
        {
            RedrawCurve();
        }

        public void SetPoints(VisualCurvePoint[] guides)
        {
            if (guides == null) return;

            if (pointData == null || pointData.Length != guides.Length)
            {
                pointData = new CurvePoint[guides.Length];
            }

            for (int i = 0; i < pointData.Length; i++)
            {
                if (guides[i] == null || guides[i].Data == null) continue;

                var data = guides[i].Data;
                if (data?.Item1 == null || data?.Item2 == null) continue;

                var p = data.Item1.position;
                var v = data.Item2.position - p;
                pointData[i] = new CurvePoint(p, v);
            }
        }

        public void SetPoints(SerializableTuple<Transform>[] guides)
        {
            if (guides == null) return;

            if (pointData == null || pointData.Length != guides.Length)
            {
                pointData = new CurvePoint[guides.Length];
            }

            for (int i = 0; i < pointData.Length; i++)
            {
                if (guides[i] == null || guides[i].Item1 == null || guides[i].Item2 == null) continue;
                
                var p = guides[i].Item1.position;
                var v = guides[i].Item2.position - p;
                pointData[i] = new CurvePoint(p, v);
            }
        }

        public void RedrawCurve()
        {
            if (numPointsDrawn < 2 || pointData.Length < 2) return;

            //if pointData.Length < 2, then we will catch that here (no error)
            //var path = CurveTools.SmoothlyConcatenatedPath(pointData);
            //if (path == null) return;

            //allows us to run in edit mode where we don't know when components are being added/removed
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }

            lineRenderer.positionCount = numPointsDrawn;
            var dt = 1 / (float)(numPointsDrawn - 1);

            for (int i = 0; i < numPointsDrawn; i++)
            {
                
                lineRenderer.SetPosition(i, CurveTools.SmoothlyConcatenatedPath(pointData, i * dt));
            }
        }
    }
}