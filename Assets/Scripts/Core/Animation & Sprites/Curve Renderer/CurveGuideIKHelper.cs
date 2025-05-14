using UnityEngine;
using RPGPlatformer.Movement;

namespace RPGPlatformer.Core
{
    public static class CurveGuideIKHelper
    {
        //rn just for 2D (should work fine in 3D but sometimes the tangent vector rotation
        //was going crazy and stretching out to infinity -- this might have been because I had one thing 
        //in here which was Vector2 while everything else was Vector3 (I think maybe the 
        //target = Vector2.LerpUnclamped... was V2 instead of V3.Lerp)
        //anyway for 2D project this is fine
        public static void FABRIK(VisualCurveGuidePoint[] guides, int startIndex, int endIndex,
            /*Vector2[] unitRays, Vector2[] unitRays2,*/ float[] lengths, float totalLength, Vector2 target,
            int iterations, float strength, float toleranceSqrd/*, bool rotateTangents*/)
        {
            if (iterations == 0 || strength == 0 || startIndex >= endIndex) return;

            var first = guides[startIndex];
            var last = guides[endIndex];
            var originalFirst = first.Point();
            var originalLast = last.Point();
            //Vector2 v;

            target = Vector2.LerpUnclamped(last.Point(), target, strength);
            var d = target - first.Point();
            var dist = d.magnitude;

            if (dist > totalLength)
            {
                target = first.Point() + totalLength * (d / dist);
            }

            for (int i = 0; i < iterations; i++)
            {
                Forward();
                Backward();

                if (Vector2.SqrMagnitude(target - last.Point()) < toleranceSqrd) break;
            }

            //shift all the guide points beyond last by the final displacement of last
            d = last.Point() - originalLast;
            for (int i = endIndex + 1; i < guides.Length; i++)
            {
                guides[i].SetPoint(guides[i].Point() + d);
            }

            //rotate each tangent vector by the amount its previous ray rotated by
            //(not a particularly good choice, but struggled to find a better one that works)
            //if (rotateTangents)
            //{
            //    for (int i = endIndex; i > startIndex; i--)
            //    {
            //        if (i < endIndex)
            //        {
            //            v = (guides[i + 1].Point() - guides[i - 1].Point()).normalized;
            //            guides[i].SetTangentDir(PhysicsTools.FromToRotation(unitRays2[i - 1], v, guides[i].TangentDir()));
            //        }
            //        else
            //        {
            //            v = (guides[i].Point() - guides[i - 1].Point()).normalized;
            //            guides[i].SetTangentDir(PhysicsTools.FromToRotation(unitRays[i - 1], v, guides[i].TangentDir()));
            //        }
            //    }
            //}

            void Forward()
            {
                last.SetPoint(target);

                for (int i = endIndex; i > startIndex; i--)
                {
                    d = (guides[i - 1].Point() - guides[i].Point()).normalized;
                    guides[i - 1].SetPoint(guides[i].Point() + lengths[i - 1] * d);
                }
            }

            void Backward()
            {
                first.SetPoint(originalFirst);

                for (int i = startIndex; i < endIndex; i++)
                {
                    d = (guides[i + 1].Point() - guides[i].Point()).normalized;
                    guides[i + 1].SetPoint(guides[i].Point() + lengths[i] * d);
                }
            }
        }

        public static void RotateTangents(VisualCurveGuidePoint[] guides, Vector2[] oldUnitRays, Vector2[] oldUnitRays2)
        {
            Vector2 v;
            var n = guides.Length - 1;

            for (int i = n; i > 0; i--)
            {
                if (i < n)
                {
                    v = (guides[i + 1].Point() - guides[i - 1].Point()).normalized;
                    guides[i].SetTangentDir(PhysicsTools.FromToRotation(oldUnitRays2[i - 1], v, guides[i].TangentDir()));
                }
                else
                {
                    v = (guides[i].Point() - guides[i - 1].Point()).normalized;
                    guides[i].SetTangentDir(PhysicsTools.FromToRotation(oldUnitRays[i - 1], v, guides[i].TangentDir()));
                }
            }

            if (n > 0)
            {
                v = (guides[1].Point() - guides[0].Point()).normalized;
                guides[0].SetTangentDir(PhysicsTools.FromToRotation(oldUnitRays[0], v, guides[0].TangentDir()));
            }
        }
    }
    
}