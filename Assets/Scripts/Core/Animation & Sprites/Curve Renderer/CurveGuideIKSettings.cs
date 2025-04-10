using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public enum IKMode
    {
        transform, point
    }

    [Serializable]
    public struct CurveGuideIKSettings
    {
        //public bool enabled;
        public IKMode mode;
        public int iterations;
        public float strength;
        public float toleranceSqrd;
        public VisualCurveGuidePoint startPoint;
        public VisualCurveGuidePoint endPoint;

        [SerializeField] Transform ikTarget;
        [SerializeField] Vector3 ikTargetPoint;

        public Vector3 TargetPosition()
        {
            if (mode == IKMode.transform)
            {
                return ikTarget.position;
            }

            return ikTargetPoint;
        }

        public void SetTarget(Transform target)
        {
            ikTarget = target;
            mode = IKMode.transform;
        }

        public void SetTarget(Vector3 target)
        {
            ikTargetPoint = target;
            mode = IKMode.point;
        }

        //has changed in the sense of "transform.hasChanged"
        public bool TargetHasChanged()
        {
            if (mode == IKMode.point)
            {
                return false;
            }

            return ikTarget && ikTarget.hasChanged;
        }

        public bool CanRunIK()
        {
            return mode == IKMode.point || (mode == IKMode.transform && ikTarget);
        }
    }
}