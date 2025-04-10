using UnityEngine;
using RPGPlatformer.Movement;
using System.Net;

namespace RPGPlatformer.Core
{
    public static class CurveGuideIKHelper
    {
        public static void FABRIK(VisualCurveGuidePoint[] guides, int startIndex, int endIndex, 
            Vector3[] unitRays, float[] lengths, Vector3 target, 
            int iterations, float strength, float toleranceSqrd)
        {
            if (iterations == 0 || strength == 0 || startIndex >= endIndex) return;

            //var m = guides.Length - 1;
            var first = guides[startIndex];
            var last = guides[endIndex];

            target = Vector3.LerpUnclamped(last.Point(), target, strength);
            var a = target - first.Point();
            var dist = a.magnitude;

            var anchor = first.Point();
            float length = 0;
            Vector3 v;
            float l;

            for (int i = startIndex; i < endIndex; i++)
            {
                v = guides[i + 1].Point() - guides[i].Point();
                l = v.magnitude;
                length += l;
                unitRays[i] = v / l;
                lengths[i] = l;
            }

            if (dist > length)
            {
                target = first.Point() + length * (a / dist);
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
                    guides[i].SetPoint(guides[i - 1].Point() + unitRays[i - 1]);
                }

                for (int i = endIndex; i > startIndex; i--)
                {
                    a = (guides[i - 1].Point() - guides[i].Point()).normalized;
                    guides[i - 1].SetPoint(guides[i].Point() + lengths[i - 1] * a);

                    v = (guides[i].Point() - guides[i - 1].Point()).normalized;
                    guides[i].SetTangentDir(PhysicsTools.FromToRotation(unitRays[i - 1], v,
                        guides[i].TangentDir(), true));
                    unitRays[i - 1] = v;
                }

                //we never have to set the ones before first, because first position will be the same at the end
                //of the algorithm
                //for (int i = startIndex - 1; i >= 0; i--)
                //{
                //    guides[i].SetPoint(guides[i + 1].Point() - unitRays[i]);
                //}
            }

            void Backward()
            {
                first.SetPoint(anchor);

                for (int i = startIndex; i < endIndex; i++)
                {
                    a = (guides[i + 1].Point() - guides[i].Point()).normalized;
                    guides[i + 1].SetPoint(guides[i].Point() + lengths[i] * a);

                    v = (guides[i + 1].Point() - guides[i].Point()).normalized;
                    guides[i + 1].SetTangentDir(PhysicsTools.FromToRotation(unitRays[i], v,
                        guides[i + 1].TangentDir(), true));
                    unitRays[i] = v;
                }

                for (int i = endIndex + 1; i < guides.Length; i++)
                {
                    guides[i].SetPoint(guides[i - 1].Point() + unitRays[i - 1]);
                }
            }
        }
    }
}