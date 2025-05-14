using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public enum CurveIKTargetSource
    {
        transform, point
    }

    //these just store data, but having them be MB allows them to be animatable
    //and add as many as you want (if they are non-mb, then I have to serialize an array of them
    //in curve guide, and the individual array elements are not animatable -- only a lone struct field would be)
    public class CurveIKEffect : MonoBehaviour
    {
        public string description;//just to make it easier to identify which ik effect is which in the inspector
        public CurveIKTargetSource targetSource;
        //public bool dontRotateTangents;
        public int ikIterations = 1;
        public float ikStrength = 1;
        public float ikToleranceSqrd = .04f;
        public VisualCurveGuidePoint startPoint;
        public VisualCurveGuidePoint endPoint;

        //you will use one or the other depending on the chosen TargetSource
        [SerializeField] Transform ikTargetTransform;
        [SerializeField] Vector2 ikTargetPoint;

        int startIndex = 0;
        int endIndex = -1;

        //public Transform IKTargetTransform => ikTargetTransform;

        public int StartIndex() => startIndex;

        public int EndIndex() => endIndex;

        public Vector2 TargetPosition()
        {
            if (targetSource == CurveIKTargetSource.transform)
            {
                return ikTargetTransform.position;
            }

            return ikTargetPoint;
        }

        public void SetTarget(Transform target)
        {
            ikTargetTransform = target;
            targetSource = CurveIKTargetSource.transform;
        }

        public void SetTarget(Vector2 target)
        {
            ikTargetPoint = target;
            targetSource = CurveIKTargetSource.point;
        }

        public void SetTargetPosition(Vector2 p)
        {
            if (targetSource == CurveIKTargetSource.transform)
            {
                ikTargetTransform.position = p;
            }
            else
            {
                ikTargetPoint = p;
            }
        }

        //has changed in the sense of "transform.hasChanged"
        //(not has changed as in "got replaced by a different object")
        public bool TargetHasChanged()
        {
            if (targetSource == CurveIKTargetSource.point)
            {
                return false;
            }

            return ikTargetTransform && ikTargetTransform.hasChanged;
        }

        public bool CanRunIK()
        {
            return targetSource == CurveIKTargetSource.point 
                || (targetSource == CurveIKTargetSource.transform && ikTargetTransform);
        }

        public void RecomputeEndptIndices(VisualCurveGuidePoint[] guides)
        {
            if (guides == null)
            {
                startIndex = 0;
                endIndex = -1;
                return;
            }

            startIndex = 0;
            endIndex = guides.Length - 1;

            for (int i = 0; i < guides.Length; i++)
            {
                if (guides[i] == startPoint)
                {
                    startIndex = i;
                }
                if (guides[i] == endPoint)
                {
                    endIndex = i;
                }
            }
        }

        public async Task LerpBetweenTransforms(Transform tr0, Transform tr1, float T, CancellationToken token,
            float lerpStrength = 1)
        {
            Vector2 p0() => tr0.transform.position;
            Vector2 p1() => tr1.transform.position;
            await LerpBetweenPositions(p0, p1, T, token, lerpStrength);
        }

        public async Task LerpTowardsTransform(Transform tr, float T, CancellationToken token,
            float lerpStrength = 1)
        {
            Vector2 p0() => endPoint.Point();
            Vector2 p1() => tr.position;
            await LerpBetweenPositions(p0, p1, T, token, lerpStrength);
        }

        //T = time to complete
        public async Task LerpBetweenPositions(Vector2 p0, Vector2 p1, float T, CancellationToken token,
            float lerpStrength = 1)
        {
            Vector2 q0() => p0;
            Vector2 q1() => p1;
            await LerpBetweenPositions(q0, q1, T, token, lerpStrength);
        }

        public async Task LerpTowardsPosition(Vector2 p1, float T, CancellationToken token, 
            float lerpStrength = 1)
        {
            Vector2 q0() => endPoint.Point();
            Vector2 q1() => p1;
            await LerpBetweenPositions(q0, q1, T, token, lerpStrength);
        }

        public async Task LerpBetweenPositions(Func<Vector2> p, Func<Vector2> q, float T,
            CancellationToken token, float lerpStrength = 1)
        {
            void UpdatePosition(float s)
            {
                SetTargetPosition(Vector2.Lerp(p(), q(), s * lerpStrength));
            }

            float t = 0;

            while (t < T)
            {
                UpdatePosition(t);
                await Task.Yield();
                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                t += Time.deltaTime;
            }

            UpdatePosition(1);
        }
    }
}