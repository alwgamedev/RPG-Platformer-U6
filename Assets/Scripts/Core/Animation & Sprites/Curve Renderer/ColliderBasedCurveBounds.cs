using Cinemachine.Editor;
using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public class ColliderBasedCurveBounds : CurveBounds
    {
        [SerializeField] Collider2D prohibitedZone;
        [SerializeField] float buffer;
        //[SerializeField] VisualCurveGuidePoint startPoint;
        //[SerializeField] VisualCurveGuidePoint endPoint;
        
        //int startIndex;
        //int endIndex;

        //Vector2 max;
        Vector2 center;
        //Vector2 min;
        float r;
        float r2;
        Vector2 p;
        Vector2 d;
        bool outOfBounds;

        private void Start()
        {
            if (prohibitedZone)
            {
                r = Mathf.Max(Vector2.Distance(prohibitedZone.bounds.max, prohibitedZone.bounds.center),
                Vector2.Distance(prohibitedZone.bounds.min, prohibitedZone.bounds.center));
                r += buffer;
                r2 = r * r;

            }
        }

        protected override bool EnforceBoundsIteration(VisualCurveGuidePoint[] guidePoints)
        {
            if (guidePoints == null || !prohibitedZone)
                return false;

            outOfBounds = false;

            //max = prohibitedZone.bounds.max + buffer * Vector3.one;
            //min = prohibitedZone.bounds.min - buffer * Vector3.one;
            center = prohibitedZone.bounds.center;

            for (int i = startIndex; i <= endIndex; i++)
            {
                p = guidePoints[i].Point();
                if (Vector2.SqrMagnitude(p - center) < r2)
                {
                    //d = center + r * (p - center).normalized - p;
                    p = center + r * (p - center).normalized;
                    outOfBounds = true;
                    guidePoints[i].SetPoint(p);
                    //outOfBounds = true;
                    //d.x = d.x < center.x ? min.x - p.x : max.x - p.x;
                    //d.y = d.y < center.y ? min.y - p.y : max.y - p.y;
                    //for (int j = startIndex; j <= i; j++)
                    //{
                    //    guidePoints[j].SetPoint(guidePoints[j].Point() + d);
                    //}
                }
            }

            return outOfBounds;
        }

        //private bool LessThan(Vector2 a, Vector2 b)
        //{
        //    return a.x < b.x && a.y < b.y;
        //}
    }
}