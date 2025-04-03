using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.Core
{
    [ExecuteAlways]
    public class BezierCurveRenderer : MonoBehaviour
    {
        [Min(0)] public int numPointsDrawn = 30;
        public Vector3[] pullPoints;
        //samplePoints.Length doesn't have to equal numPointsDrawn
        //samplePoints = points we want to pass through
        //numPointsDrawn = number of points drawn along the curve passing through those points

        LineRenderer lineRenderer;

        private void OnValidate()
        {
            RedrawCurve();
        }

        public void RedrawCurve()
        {
            if (numPointsDrawn < 2 || pullPoints == null || pullPoints.Length == 0) return;

            var path = PhysicsTools.BezierPath(pullPoints);
            if (path == null) return;

            //allows us to run in edit mode where we don't know when components are being added/removed
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }

            lineRenderer.positionCount = numPointsDrawn;
            var dt = 1 / (float)(numPointsDrawn - 1);

            for (int i = 0; i < numPointsDrawn; i++)
            {
                lineRenderer.SetPosition(i, path(i * dt));
            }
        }
    }
}