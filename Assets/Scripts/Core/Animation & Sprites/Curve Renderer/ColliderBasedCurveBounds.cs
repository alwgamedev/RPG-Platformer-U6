using Cinemachine.Editor;
using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public class ColliderBasedCurveBounds : CurveBounds
    {
        [SerializeField] Collider2D prohibitedZone;
        [SerializeField] float buffer;
        //[SerializeField] float resetAvoidanceDistanceFactorSqrd = 2.25f;
        //[SerializeField] AvoidanceSide defaultAvoidanceSide;

        Vector2 center;
        //float d;
        float r;
        float r2;
        Vector2 p;
        //Vector2 v;
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

            //avoidanceSide = defaultAvoidanceSide;
        }

        protected override bool EnforceBoundsIteration(VisualCurveGuidePoint[] guidePoints)
        {
            if (guidePoints == null || !prohibitedZone)
                return false;

            outOfBounds = false;

            center = prohibitedZone.bounds.center;
            //v = endPoint.Point() - center;//startPoint.Point();

            //if (avoidanceSide == AvoidanceSide.undetermined)
            //{
            //    avoidanceSide = AvoidanceSide.left;
            //    //avoidanceSide = Vector2.Dot(endPoint.Point() - center, v) > 0 ?
            //    //    AvoidanceSide.right : AvoidanceSide.left;
            //}

            for (int i = startIndex; i <= endIndex; i++)
            {
                p = guidePoints[i].Point();

                if (Vector2.SqrMagnitude(p - center) < r2)
                {
                    outOfBounds = true;
                    //if (IsOnWrongSide(v, p - center, avoidanceSide))
                    //{
                    //    p = center 
                    //        + (Vector2)PhysicsTools.ReflectAcrossPerpendicularHyperplane(v.CCWPerp(), p - center);
                    //}
                    p = center + r * (p - center).normalized;
                    guidePoints[i].SetPoint(p);
                }
            }

            return outOfBounds;
        }

        //protected override bool AvoidanceSideCanBeReset(VisualCurveGuidePoint[] guidePoints)
        //{
        //    for (int i = startIndex; i <= endIndex; i++)
        //    {
        //        if (Vector2.SqrMagnitude(guidePoints[i].Point() - center) < resetAvoidanceDistanceFactorSqrd * r2)
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}

        //returns whether point q is on the wrong side of oriented line spanned by u
        //(i.e. if avoidance side is right and q is on the left side or vice versa)
        //private bool IsOnWrongSide(Vector2 u, Vector2 q, AvoidanceSide avoidanceSide)
        //{
        //    if (avoidanceSide == AvoidanceSide.right && Vector2.Dot(u, q) < 0)
        //    {
        //        return true;
        //    }

        //    if (avoidanceSide == AvoidanceSide.left && Vector2.Dot(u, q) > 0)
        //    {
        //        return true;
        //    }

        //    return false;
        //}
    }
}