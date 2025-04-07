using UnityEngine;
using RPGPlatformer.Movement;

namespace RPGPlatformer.Core
{
    public static class CurveGuideIKHelper
    {
        public static void FABRIK(VisualCurveGuidePoint[] guides, Vector3[] unitRays, float[] lengths, Vector3 target, 
            int iterations, float strength, float toleranceSqrd)
        {
            if (iterations == 0 || strength == 0) return;

            var m = guides.Length - 1;
            var first = guides[0];
            var last = guides[m];

            target = Vector3.LerpUnclamped(last.Point(), target, strength);
            var a = target - first.Point();
            var dist = a.magnitude;

            var anchor = first.Point();
            float length = 0;
            Vector3 v;
            float l;

            for (int i = 0; i < m; i++)
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

                for (int i = m; i > 0; i--)
                {
                    a = (guides[i - 1].Point() - guides[i].Point()).normalized;
                    guides[i - 1].SetPoint(guides[i].Point() + lengths[i - 1] * a);

                    v = (guides[i].Point() - guides[i - 1].Point()).normalized;
                    guides[i - 1].SetTangentDir(PhysicsTools.FromToRotation(unitRays[i - 1], v,
                        guides[i - 1].TangentDir(), true));
                    unitRays[i - 1] = v;

                    //the last tangent doesn't get rotated? maybe rethink this

                }
            }

            void Backward()
            {
                first.SetPoint(anchor);

                for (int i = 0; i < m; i++)
                {
                    a = (guides[i + 1].Point() - guides[i].Point()).normalized;
                    guides[i + 1].SetPoint(guides[i].Point() + lengths[i] * a);

                    v = (guides[i + 1].Point() - guides[i].Point()).normalized;
                    guides[i].SetTangentDir(PhysicsTools.FromToRotation(unitRays[i], v,
                        guides[i].TangentDir(), true));
                    unitRays[i] = v;
                }
            }
        }
    }
}