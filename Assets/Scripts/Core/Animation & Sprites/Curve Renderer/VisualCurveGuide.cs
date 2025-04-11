using RPGPlatformer.Core;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.VisualScripting;

namespace RPGPlatformer
{
    [ExecuteAlways]
    public class VisualCurveGuide : MonoBehaviour
    {
        [SerializeField] bool drawGizmos;
        [SerializeField] VisualCurveGuidePoint[] guides;

        public bool ikEnabled;
        public CurveIKEffect[] ikEffects;

        CurveRenderer curveRenderer;
        Vector3[] unitRays;
        float[] lengths;//storage for lengths in the IK algorithm
        float totalLength;

        private void Awake()
        {
            ReconfigureIKEffects();
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
            ReconfigureIKEffects();
        }

        public void DisableAllIK()
        {
            if (ikEffects != null)
            {
                foreach (var e in ikEffects)
                {
                    if (e != null)
                    {
                        e.enabled = false;
                    }
                }

                ikEnabled = false;
            }
        }

        //should call this whenever you add new ik effects
        public void ReconfigureIKEffects()
        {
            if (ikEffects == null || guides == null) return;

            foreach (var effect in ikEffects)
            {
                effect?.RecomputeEndptIndices(guides);
            }
        }

        private void CheckForUpdates()
        {
            if (guides == null) return;

            if (ikEffects != null)
            {
                foreach (var effect in ikEffects)
                {
                    if (effect != null && effect.TargetHasChanged())
                    {
                        UpdateRendererGuidePoints();
                        return;
                    }
                }
            }

            foreach (var guide in guides)
            {
                if (guide && guide.HasChanged())
                {
                    UpdateRendererGuidePoints();
                    return;
                }
            }
        }

        private void UpdateRendererGuidePoints()
        {
            if (ikEnabled && ikEffects != null)
            {
                RecomputeRaysAndLengths();

                foreach (var e in ikEffects)
                {
                    if (e != null && e.enabled && e.CanRunIK())
                    {
                        CurveGuideIKHelper.FABRIK(guides, e.StartIndex(), e.EndIndex(), unitRays, lengths, totalLength,
                        e.TargetPosition(), e.ikIterations, e.ikStrength, e.ikToleranceSqrd);
                    }
                }
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

        private void RecomputeRaysAndLengths()
        {
            if (lengths == null || lengths.Length != guides.Length - 1)
            {
                lengths = new float[guides.Length - 1];
            }
            if (unitRays == null || unitRays.Length != guides.Length - 1)
            {
                unitRays = new Vector3[guides.Length - 1];
            }

            Vector3 v;
            float l;

            totalLength = 0;

            for (int i = 0; i < guides.Length - 1; i++)
            {
                v = guides[i + 1].Point() - guides[i].Point();
                l = v.magnitude;
                totalLength += l;
                unitRays[i] = v / l;
                lengths[i] = l;
            }
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