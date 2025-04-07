using UnityEngine;

namespace RPGPlatformer.Core
{
    [ExecuteAlways]
    public class VisualCurveGuidePoint : MonoBehaviour
    {
        [SerializeField] Transform tangentPuller;

        bool wasActive;

        private void Awake()
        {
            wasActive = Active();
        }
        public bool Active() => gameObject.activeSelf && tangentPuller;

        public Vector3 Point() => transform.position;

        public Vector3 TangentDir() => tangentPuller.position - transform.position;

        public void SetPoint(Vector3 p)
        {
            transform.position = p;
        }

        public void SetTangentDir(Vector3 v)
        {
            tangentPuller.position = Point() + v;
        }

        public bool HasChanged()
        {
            if (wasActive != Active())
            {
                wasActive = Active();
                return true;
            }

            return Active() && (transform.hasChanged || tangentPuller.hasChanged);
        }

        public void DrawGizmo()
        {
            if (Active())
            {
                Debug.DrawLine(transform.position, tangentPuller.position, Color.yellow);
            }
        }
    }
}