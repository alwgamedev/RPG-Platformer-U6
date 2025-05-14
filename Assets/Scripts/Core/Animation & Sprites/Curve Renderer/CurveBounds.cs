using UnityEngine;

namespace RPGPlatformer.Core
{
    public abstract class CurveBounds : MonoBehaviour
    {
        [SerializeField] protected VisualCurveGuidePoint startPoint;
        [SerializeField] protected VisualCurveGuidePoint endPoint;
        [SerializeField] protected int iterations;

        //protected enum AvoidanceSide
        //{
        //    undetermined, right, left
        //}

        protected int startIndex;
        protected int endIndex;
        //protected AvoidanceSide avoidanceSide = AvoidanceSide.undetermined;


        //returns whether 
        public void EnforceBounds(VisualCurveGuidePoint[] guidePoints, float[] lengths)
        {
            for (int i = 0; i < iterations; i++)
            {
                if (!EnforceBoundsIteration(guidePoints))
                {
                    //if (i == 0)
                    //{
                    //    if (avoidanceSide != AvoidanceSide.undetermined && AvoidanceSideCanBeReset(guidePoints))
                    //    {
                    //        avoidanceSide = AvoidanceSide.undetermined;
                    //    }
                    //}
                    return;
                }

                CorrectLengths(guidePoints, lengths);
            }
        }

        //will return whether the algorithm did anything (i.e. whether any points were outside bounds)
        protected abstract bool EnforceBoundsIteration(VisualCurveGuidePoint[] guidePoints);

        //protected abstract bool AvoidanceSideCanBeReset(VisualCurveGuidePoint[] guidePoints);

        public void Configure(VisualCurveGuidePoint[] guidePoints)
        {
            if (guidePoints == null)
            {
                startIndex = 0;
                endIndex = -1;
                return;
            }

            startIndex = 0;
            endIndex = guidePoints.Length - 1;

            for (int i = 0; i < guidePoints.Length; i++)
            {
                if (guidePoints[i] == startPoint)
                {
                    startIndex = i;
                }
                if (guidePoints[i] == endPoint)
                {
                    endIndex = i;
                }
            }
        }

        protected void CorrectLengths(VisualCurveGuidePoint[] guidePoints, float[] lengths)
        {
            Vector2 v;

            for (int i = 1; i < guidePoints.Length; i++)
            {
                v = (guidePoints[i].Point() - guidePoints[i - 1].Point()).normalized;
                guidePoints[i].SetPoint(guidePoints[i - 1].Point() + lengths[i - 1] * v);
            }
        }
    }
}