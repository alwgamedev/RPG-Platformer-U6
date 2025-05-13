using UnityEngine;
using RPGPlatformer.Movement;
using Unity.VisualScripting;
using Cinemachine.Editor;

namespace RPGPlatformer.Core
{
    public static class CurveGuideIKHelper
    {
        public static void FABRIK(VisualCurveGuidePoint[] guides, int startIndex, int endIndex,
            Vector3[] unitRays, Vector3[] unitRays2, float[] lengths, float totalLength, Vector3 target,
            int iterations, float strength, float toleranceSqrd, bool rotateTangents)
        {
            if (iterations == 0 || strength == 0 || startIndex >= endIndex) return;

            var first = guides[startIndex];
            var last = guides[endIndex];
            var originalFirst = first.Point();
            var originalLast = last.Point();
            Vector3 v;

            target = Vector3.LerpUnclamped(last.Point(), target, strength);
            var d = target - first.Point();
            var dist = d.magnitude;

            if (dist > totalLength)
            {
                target = first.Point() + totalLength * (d / dist);
            }

            //for (int i = startIndex + 1; i < endIndex; i++)
            //{
            //    v = guides[i].Point() - guides[i - 1].Point();
            //    d = guides[i + 1].Point() - guides[i].Point();
            //    guides[i].SetTangentDir(Components(guides[i].TangentDir(), v, d));
            //}

            for (int i = 0; i < iterations; i++)
            {
                Forward();
                Backward();

                if (Vector3.SqrMagnitude(target - last.Point()) < toleranceSqrd) break;
            }

            //shift all the guide points beyond last by the final displacement of last
            d = last.Point() - originalLast;
            for (int i = endIndex + 1; i < guides.Length; i++)
            {
                guides[i].SetPoint(guides[i].Point() + d);
            }

            //rotate each tangent vector by the amount its previous ray rotated by
            //(not a particularly good choice, but struggled to find a better one that works)
            if (rotateTangents)
            {
                for (int i = endIndex; i > startIndex; i--)
                {
                    if (i < endIndex)
                    {
                        v = (guides[i + 1].Point() - guides[i - 1].Point()).normalized;
                        guides[i].SetTangentDir(PhysicsTools.FromToRotation(unitRays2[i - 1], v, guides[i].TangentDir()));
                    }
                    else
                    {
                        v = (guides[i].Point() - guides[i - 1].Point()).normalized;
                        guides[i].SetTangentDir(PhysicsTools.FromToRotation(unitRays[i - 1], v, guides[i].TangentDir()));
                    }
                    //unitRays[i - 1] = v;

                    //if (i < endIndex)
                    //{
                    //    guides[i].SetTangentDir(PhysicsTools.FromToRotation(unitRays[i - 1], v, 
                    //        guides[i].TangentDir()));
                    //}
                    //if (i <= unitRays2.Length)
                    //{
                    //    v = (guides[i + 1].Point() - guides[i - 1].Point()).normalized;
                    //    guides[i].SetTangentDir(PhysicsTools.FromToRotation(unitRays2[i - 1], v, guides[i].TangentDir()));
                    //}
                }
            }

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

        //returns components of a in basis u, v, u x v
        //or if u,v are parallel it returns (Dot(a, u.normalized), 0, 0) (zero if u = 0)
        //public static Vector3 Components(Vector3 a, Vector3 u, Vector3 v)
        //{
        //    var w = Vector3.Cross(u, v);

        //    if (w == Vector3.zero)
        //    {
        //        return new Vector3(Vector3.Dot(a, u.normalized), 0, 0);
        //    }

        //    var d = 1 / Vector3.Dot(u, Vector3.Cross(v, w));//1 / det(u,v,w)
        //    var uu = d * new Vector3(v.y * w.z - v.z * w.y, u.z * w.y - u.y * w.z, u.y * v.z - u.z * v.y);
        //    var vv = d * new Vector3(v.z * w.x - v.x * w.z, u.x * w.z - u.z * w.x, u.z * v.x - u.x * v.z);
        //    var ww = d * new Vector3(v.x * w.y - v.y * w.x, u.y * w.x - u.x * w.y, u.x * v.y - u.y * v.x);
        //    //(rows of (u,v,w) inverse)

        //    return new Vector3(Vector3.Dot(a, uu), Vector3.Dot(a, vv), Vector3.Dot(a, ww));
        //}
    }


}