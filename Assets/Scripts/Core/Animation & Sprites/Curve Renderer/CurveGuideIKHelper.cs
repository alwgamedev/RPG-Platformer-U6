using UnityEngine;
using RPGPlatformer.Movement;

namespace RPGPlatformer.Core
{
    public static class CurveGuideIKHelper
    {
        public static void FABRIK(VisualCurveGuidePoint[] guides, int startIndex, int endIndex, 
            Vector3[] unitRays, float[] lengths, float totalLength, Vector3 target, 
            int iterations, float strength, float toleranceSqrd)
        {
            if (iterations == 0 || strength == 0 || startIndex >= endIndex) return;

            //var m = guides.Length - 1;
            var first = guides[startIndex];
            var last = guides[endIndex];
            var anchor = first.Point();
            Vector3 v;

            target = Vector3.LerpUnclamped(last.Point(), target, strength);
            var d = target - first.Point();
            var dist = d.magnitude;

            //for (int i = startIndex; i < endIndex; i++)
            //{
            //    v = guides[i + 1].Point() - guides[i].Point();
            //    l = v.magnitude;
            //    length += l;
            //    unitRays[i] = v / l;
            //    lengths[i] = l;
            //}

            if (dist > totalLength)
            {
                target = first.Point() + totalLength * (d / dist);
            }

            for (int i = 0; i < iterations; i++)
            {
                Forward();
                Backward();

                if (Vector3.SqrMagnitude(target - last.Point()) < toleranceSqrd) break;
            }

            void Forward()
            {
                last.SetPoint(target);

                for (int i = endIndex + 1; i < guides.Length; i++)
                {
                    guides[i].SetPoint(guides[i - 1].Point() + lengths[i - 1] * unitRays[i - 1]);
                }

                for (int i = endIndex; i > startIndex; i--)
                {
                    d = (guides[i - 1].Point() - guides[i].Point()).normalized;
                    guides[i - 1].SetPoint(guides[i].Point() + lengths[i - 1] * d);

                    v = (guides[i].Point() - guides[i - 1].Point()).normalized;
                    guides[i].SetTangentDir(PhysicsTools.FromToRotation(unitRays[i - 1], v,
                        guides[i].TangentDir(), true));
                    unitRays[i - 1] = v;
                }

                //we never have to set the ones before first pos, because first position will be the same at the end
                //of the algorithm
            }

            void Backward()
            {
                first.SetPoint(anchor);

                for (int i = startIndex; i < endIndex; i++)
                {
                    d = (guides[i + 1].Point() - guides[i].Point()).normalized;
                    guides[i + 1].SetPoint(guides[i].Point() + lengths[i] * d);

                    v = (guides[i + 1].Point() - guides[i].Point()).normalized;
                    guides[i + 1].SetTangentDir(PhysicsTools.FromToRotation(unitRays[i], v,
                        guides[i + 1].TangentDir(), true));
                    unitRays[i] = v;
                }

                for (int i = endIndex + 1; i < guides.Length; i++)
                {
                    guides[i].SetPoint(guides[i - 1].Point() + lengths[i - 1] * unitRays[i - 1]);
                }
            }
        }
    }
}