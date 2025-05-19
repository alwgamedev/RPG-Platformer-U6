using RPGPlatformer.Core;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace RPGPlatformer
{
    [ExecuteAlways]
    public class VisualCurveGuide : MonoBehaviour
    {
        [SerializeField] bool drawGizmos;
        [SerializeField] bool physicsDependent = true;
        [SerializeField] VisualCurveGuidePoint[] guides;
        [HideInInspector][SerializeField] float _lengthScale = 1;

        VisualCurveGuidePoint[] _guides;
        VisualCurveGuidePoint[] Guides
        {
            get
            {
                if (_guides == null)
                {
                    _guides = guides;
                }

                return _guides;
            }
        }

        public bool ikEnabled;
        public CurveIKEffect[] ikEffects;

        public bool enforceBounds;
        public CurveBounds bounds;

        public float lengthScale = 1;

        CurveRenderer curveRenderer;
        Vector2[] unitRays;//direction of g[i + 1] - g[i]
        Vector2[] unitRays2;//direction of g[i + 2] - g[i]
        float[] lengths;//storage for lengths in the IK algorithm
        float totalLength;

        bool effectAppliedThisFrame;

        private void Awake()
        {
            //lengthScale = 1;
            //_lengthScale = lengthScale;
            ReconfigureIKEffects();
            ReconfigureBounds();
        }

        private void Update()
        {
            if (lengthScale != _lengthScale)
            {
                UpdateLengthScale();
            }

            CheckForUpdates();
        }

        public void CallUpdate()
        {
            Update();
        }

        //need to do this in fixed update if you want colliders to move with IK
        private void FixedUpdate()
        {
            if (physicsDependent && ikEnabled)
            {
                Update();
            }
        }

        private void OnValidate()
        {
            ReconfigureIKEffects();
            ReconfigureBounds();
            //SetLengthScale(lengthScale);
        }

        public void SetGuidePoints(VisualCurveGuidePoint[] guides)
        {
            if (guides == null)
                return;

            int n = guides.Length;
            _guides = new VisualCurveGuidePoint[n];
            Array.Copy(guides, _guides, n);
            ReconfigureIKEffects();
            ReconfigureBounds();
        }

        public void UpdateLengthScale()
        {
            if (lengthScale <= 0)
            {
                lengthScale = _lengthScale;
                return;
            }

            float r = lengthScale / _lengthScale;

            for (int i = 1; i < guides.Length; i++)
            {
                Guides[i].SetPoint(Guides[0].Point() + r * (Guides[i].Point() - Guides[0].Point()));
                Guides[i].SetTangentDir(r * Guides[i].TangentDir());
            }

            _lengthScale = lengthScale;
        }

        //l = goal length scale, T = time to complete
        public async Task LerpLengthScale(float l, float T, CancellationToken token, Func<bool> canContinue = null)
        {
            float r = (l - lengthScale) / T;
            float t = 0;

            while (t < T && (canContinue?.Invoke() ?? true))
            {
                await Task.Yield();
                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                t += Time.deltaTime;
                lengthScale += r * Time.deltaTime;
            }
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
        private void ReconfigureIKEffects()
        {
            if (ikEffects == null || Guides == null) return;

            foreach (var effect in ikEffects)
            {
                effect?.RecomputeEndptIndices(Guides);
            }
        }

        private void ReconfigureBounds()
        {
            if (Guides == null || !bounds) return;

            bounds.Configure(Guides);
        }

        private void CheckForUpdates()
        {
            if (Guides == null) return;

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

            foreach (var guide in Guides)
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
            effectAppliedThisFrame = false;

            if (ikEnabled && ikEffects != null)
            {
                effectAppliedThisFrame = true;
                RecomputeRaysAndLengths();

                foreach (var e in ikEffects)
                {
                    if (e != null && e.enabled && e.CanRunIK())
                    {
                        CurveGuideIKHelper.FABRIK(Guides, e.StartIndex(), e.EndIndex(), 
                            /*unitRays, unityRays2,*/ lengths, totalLength,
                            e.TargetPosition(), e.ikIterations, e.ikStrength, e.ikToleranceSqrd);
                    }
                }
            }

            if (enforceBounds && bounds != null)
            {
                if (!effectAppliedThisFrame)
                {
                    effectAppliedThisFrame = true;
                    RecomputeRaysAndLengths();
                }

                bounds.EnforceBounds(Guides, lengths);
            }

            if (effectAppliedThisFrame)
            {
                CurveGuideIKHelper.RotateTangents(Guides, unitRays, unitRays2);
            }

            if (curveRenderer == null)
            {
                curveRenderer = GetComponent<CurveRenderer>();
            }
            if (curveRenderer != null)
            {
                curveRenderer.HandleGuidePointChanges(Guides);
            }
            //let's see if this triggers curve renderer's validate, or we should call draw ourselves
        }

        private void RecomputeRaysAndLengths()
        {
            if (lengths == null || lengths.Length != Guides.Length - 1)
            {
                lengths = new float[Guides.Length - 1];
            }
            if (unitRays == null || unitRays.Length != Guides.Length - 1)
            {
                unitRays = new Vector2[Guides.Length - 1];
            }

            if (unitRays2 == null || unitRays2.Length != Guides.Length - 2)
            {
                unitRays2 = new Vector2[Math.Max(Guides.Length - 2, 0)];
            }

            Vector3 v;
            float l;

            totalLength = 0;

            for (int i = 0; i < Guides.Length - 1; i++)
            {
                v = Guides[i + 1].Point() - Guides[i].Point();
                l = v.magnitude;
                totalLength += l;
                unitRays[i] = v / l;
                lengths[i] = l;
            }

            for (int i = 0; i < Guides.Length - 2; i++)
            {
                unitRays2[i] = (Guides[i + 2].Point() - Guides[i].Point()).normalized;
            }
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos || Guides == null) return;

            int i = 0;
            while (i < Guides.Length)
            {
                if (!Guides[i] || !Guides[i].Active())
                {
                    i++; 
                    continue;
                }

                Guides[i].DrawGizmo();

                int j = i + 1;

                //find the next active node
                while (j < Guides.Length)
                {
                    if (!Guides[j] || !Guides[j].Active())
                    {
                        j++;
                        continue;
                    }

                    Debug.DrawLine(Guides[i].Point(), Guides[j].Point(), Color.green);
                    break;
                }

                i = j;
            }
        }
    }
}