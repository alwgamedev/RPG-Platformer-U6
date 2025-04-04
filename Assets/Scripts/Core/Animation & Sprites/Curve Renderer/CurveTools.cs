using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public static class CurveTools
    {
        /// <summary>
        /// Path through data points = (p[i], v[i]), where the curve will pass through 
        /// each point p[i] with tangent direction v[i]. You can leave the last tangent direction zero
        /// if you want it to be configured automatically.
        /// </summary>
        public static Func<float, Vector3> SmoothlyConcatenatedPath(CurvePoint[] dataPoints)
        {
            if (dataPoints == null || dataPoints.Length < 2)
            {
                return null;
            }

            var n = dataPoints.Length - 1;//number of curve segments
            var dt = 1 / (float)n;

            Vector3 R(float t)
            {
                var i = (int)(t / dt);
                if (i >= n)
                {
                    return dataPoints[n].point;
                }
                if (i < 0)
                {
                    return dataPoints[0].point;
                }

                var s = (t - i * dt) / dt;
                return PathWithTangents(dataPoints[i].point, dataPoints[i].velocity,
                    dataPoints[i + 1].point, dataPoints[i + 1].velocity)(s);
            }

            return R;
        }

        /// <summary>
        /// Returns the degree three curve from p to q with initial velocity v
        /// and terminal velocity w (parametrized over [0,1]). If w = 0, it will use the first pulling point
        /// r = p + v and set w = q - r.
        /// </summary>
        public static Func<float, Vector3> PathWithTangents(Vector3 p, Vector3 v, Vector3 q, Vector3 w = default)
        {
            var d = q - p;

            if (w == Vector3.zero)
            {
                w = d - v;
            }

            Vector3 R(float t)
            {
                return p + t * v + t * t * (3 * d - 2 * v - w) + t * t * t * (-2 * d + v + w);
            }

            return R;
        }
    }
}