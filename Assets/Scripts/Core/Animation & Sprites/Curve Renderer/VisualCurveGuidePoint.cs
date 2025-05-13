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

        public Vector2 Point() => transform.position;

        public Vector2 TangentDir() => tangentPuller.position - transform.position;

        public void SetPoint(Vector2 p)
        {
            transform.position = p;
        }

        public void SetTangentDir(Vector2 v)
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