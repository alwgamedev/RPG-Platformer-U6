using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public enum TargetSource
    {
        transform, point
    }

    //these just store data, but having them be MB allows them to be animatable
    //and add as many as you want (if they are non-mb, then I have to serialize an array of them
    //in curve guide, and the individual array elements are not animatable -- only a lone struct field would be)
    public class CurveIKEffect : MonoBehaviour
    {
        public string description;
        public TargetSource targetSource;
        public int ikIterations = 1;
        public float ikStrength = 1;
        public float ikToleranceSqrd = .04f;
        public VisualCurveGuidePoint startPoint;
        public VisualCurveGuidePoint endPoint;

        //you will use one or the other depending on the chosen TargetSource
        [SerializeField] Transform ikTargetTransform;
        [SerializeField] Vector3 ikTargetPoint;

        int startIndex = 0;
        int endIndex = -1;

        public int StartIndex() => startIndex;

        public int EndIndex() => endIndex;

        public Vector3 TargetPosition()
        {
            if (targetSource == TargetSource.transform)
            {
                return ikTargetTransform.position;
            }

            return ikTargetPoint;
        }

        public void SetTarget(Transform target)
        {
            ikTargetTransform = target;
            targetSource = TargetSource.transform;
        }

        public void SetTarget(Vector3 target)
        {
            ikTargetPoint = target;
            targetSource = TargetSource.point;
        }

        //has changed in the sense of "transform.hasChanged"
        //(not has changed as in "got replaced by a different object")
        public bool TargetHasChanged()
        {
            if (targetSource == TargetSource.point)
            {
                return false;
            }

            return ikTargetTransform && ikTargetTransform.hasChanged;
        }

        public bool CanRunIK()
        {
            return targetSource == TargetSource.point 
                || (targetSource == TargetSource.transform && ikTargetTransform);
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
    }
}