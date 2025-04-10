using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer
{
    [ExecuteAlways]
    public class VisualCurveGuide : MonoBehaviour
    {
        [SerializeField] bool drawGizmos;
        [SerializeField] VisualCurveGuidePoint[] guides;
        [SerializeField] CurveGuideIKSettings ikSettings;

        public bool ikEnabled;

        int ikStartIndex;
        int ikEndIndex;

        CurveRenderer curveRenderer;
        Vector3[] unitRays;
        float[] lengths;//storage for lengths in the IK algorithm

        public CurveGuideIKSettings IKSettings
        {
            get => ikSettings;
            set
            {
                ikSettings = value;
                RecomputeIKEndpts();
            }
        }

        private void Awake()
        {
            RecomputeIKEndpts();
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (!Application.isPlaying)
            {
                CheckForUpdates();
            }
        }
#endif

        //doing this in fixed update fixes the issue with tearing (I think caused by colliders)
        private void FixedUpdate()
        {
            CheckForUpdates();
        }

        private void OnValidate()
        {
            RecomputeIKEndpts();
        }

        private void RecomputeIKEndpts()
        {
            if (guides == null)
            {
                ikStartIndex = 0;
                ikEndIndex = -1;
                return;
            }

            ikStartIndex = 0;
            ikEndIndex = guides.Length - 1;

            for (int i = 0; i < guides.Length; i++)
            {
                if (guides[i] == ikSettings.startPoint)
                {
                    ikStartIndex = i;
                }
                if (guides[i] == ikSettings.endPoint)
                {
                    ikEndIndex = i;
                }
            }
        }

        private void CheckForUpdates()
        {
            if (guides == null) return;

            if (ikSettings.TargetHasChanged())
            {
                UpdateRendererGuidePoints();
                return;
            }

            foreach (var guide in guides)
            {
                if (guide && guide.HasChanged())
                {
                    UpdateRendererGuidePoints();
                    break;
                }
            }
        }

        private void UpdateRendererGuidePoints()
        {
            if (unitRays == null || unitRays.Length != guides.Length - 1)
            {
                unitRays = new Vector3[guides.Length - 1];
            }
            if (lengths == null || lengths.Length != guides.Length - 1)
            {
                lengths = new float[guides.Length - 1];
            }

            if (ikEnabled && ikSettings.CanRunIK())
            {
                CurveGuideIKHelper.FABRIK(guides, ikStartIndex, ikEndIndex, unitRays, lengths,
                ikSettings.TargetPosition(), ikSettings.iterations, ikSettings.strength, ikSettings.toleranceSqrd);
            }

            if (curveRenderer == null)
            {
                curveRenderer = GetComponent<CurveRenderer>();
            }
            if (curveRenderer != null)
            {
                curveRenderer.HandleGuidePointChanges(guides);
            }
            //let's see if this triggers curve renderer's validate, or we should call draw ourselves
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos || guides == null) return;

            int i = 0;
            while (i < guides.Length)
            {
                if (!guides[i] || !guides[i].Active())
                {
                    i++; 
                    continue;
                }

                guides[i].DrawGizmo();

                int j = i + 1;

                //find the next active node
                while (j < guides.Length)
                {
                    if (!guides[j] || !guides[j].Active())
                    {
                        j++;
                        continue;
                    }

                    Debug.DrawLine(guides[i].Point(), guides[j].Point(), Color.green);
                    break;
                }

                i = j;
            }
        }
    }
}