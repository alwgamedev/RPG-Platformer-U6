using UnityEngine;

namespace RPGPlatformer.Core
{
    public static class CurveTools
    {

        /// <summary>
        /// C1 path through data points = (p[i], v[i]), where the curve will pass through 
        /// each point p[i] with tangent direction v[i]. Each path segment is traversed in equal time
        /// (so it ignores the CurvePoint.lengthUnits property), which would cause
        /// the curve texture to stretch along each segment based on its length (when the line renderer is 
        /// in distribute per segment mode).
        /// </summary>
        public static Vector3 C1ConcatenatedPath(CurveGuidePoint[] dataPoints, float t)
        {
            var n = dataPoints.Length - 1;//number of curve segments
            var dt = 1 / (float)n;//time per segment
            var i = (int)(t / dt);
            if (i >= n)
            {
                return dataPoints[n].point;
            }

            var s =  (t - i * dt) / (dataPoints[i + 1].virtualSegments * dt);
            return PathWithTangents(s, dataPoints[i].point, dataPoints[i].velocity,
                dataPoints[i + 1].point, dataPoints[i + 1].velocity);
        }

        /// <summary>
        /// Returns position at time t along the degree three parametric curve from p to q with initial velocity v
        /// and terminal velocity w (parametrized over [0,1]).
        /// </summary>
        public static Vector3 PathWithTangents(float t, Vector3 p, Vector3 v, Vector3 q, Vector3 w = default)
        {
            var d = q - p;

            return p + t * v + t * t * (3 * d - 2 * v - w) + t * t * t * (-2 * d + v + w);
        }
    }
}