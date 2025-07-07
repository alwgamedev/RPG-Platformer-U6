using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using RPGPlatformer.Core;
using System;

namespace RPGPlatformer.Effects
{
    [RequireComponent(typeof(MaterialManager))]
    public class Highlighter : MonoBehaviour
    {
        [SerializeField] bool matHasThicknessParameter = true;
        [SerializeField] float minThickness;
        [SerializeField] float maxThickness = .02f;
        [SerializeField] float minIntensity;//almost always 0
        [SerializeField] float maxIntensity = 5;
        [SerializeField] AnimationCurve thicknessCurve
            = new(new Keyframe[] { new(0, 0), new(1, 1) });
        [SerializeField] AnimationCurve intensityCurve 
            = new(new Keyframe[] { new(0, 0), new(1, 1)});
        [SerializeField] float tweenTime = 1;//seconds to go from min to max highlight
        [SerializeField] float easeInTimeScale = 1;
        [SerializeField] float easeOutTimeScale = - 0.5f;
        
        const float defaultHighlightFlashRestDuration = .1f;

        protected MaterialManager materialManager;
        
        float thicknessRange;
        float intensityRange;
        float tweenRate;//tween progress per second = timeScale / tweenTime
        float tweenProgress;//from 0-1
        bool tweening;

        public bool Tweening => tweening;
        public float HighlightPercentage => tweenProgress;//0-1 corresponding to minIntensity -> maxIntensity
        public bool HighlightActive => HighlightPercentage > 0;

        public event Action HighlightTweenComplete;

        protected virtual void Awake()
        {
            materialManager = GetComponent<MaterialManager>();
            thicknessRange = maxThickness - minThickness;
            intensityRange = maxIntensity - minIntensity;
            tweenRate = intensityRange / tweenTime;
        }

        protected virtual void Start()
        {
            InstantMinHighlight();
        }

        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.K))
            //{
            //    testing = !testing;
            //    EnableHighlight(testing);
            //}

            if (tweening)
            {
                UpdateTween();
            }
        }

        public void InstantMaxHighlight()
        {
            EndTween();
            SetIntensity(maxIntensity);
            tweenProgress = 1;
        }

        public void InstantMinHighlight()
        {
            EndTween();
            SetIntensity(minIntensity);
            tweenProgress = 0;
        }

        public void EnableHighlight(bool val)
        {
            BeginTween(val ? easeInTimeScale : easeOutTimeScale);
        }

        public async Task HighlightFlash(CancellationToken token,
            float restDuration = defaultHighlightFlashRestDuration)
        {
            EnableHighlight(true);
            await MiscTools.DelayGameTime((easeInTimeScale * (1 - tweenProgress) / tweenRate) + restDuration, token);
            EnableHighlight(false);
        }

        //note that this automatically replaces any ongoing tween, which is nice
        //however the "Down" half of a highlight flash could come in after a delay and override your new tween,
        //so you should give the highlight flash an appropriate cancellation token if you want to prevent this
        private void BeginTween(float timeScale)
        {
            if (timeScale == 0) return;

            tweenRate = timeScale / tweenTime;
            tweening = true;
            //and tweenProgress is assumed to already be accurate
        }

        private void UpdateTween()
        {
            tweenProgress += tweenRate * Time.deltaTime;

            if (tweenProgress > 1)
            {
                tweenProgress = 1;
                SetThickness(maxThickness);
                SetIntensity(maxIntensity);
                EndTween();
                return;
            }

            if (tweenProgress < 0)
            {
                tweenProgress = 0;
                SetThickness(minThickness);
                SetIntensity(minIntensity);
                EndTween();
                return;
            }

            SetThickness(Thickness(tweenProgress));
            SetIntensity(Intensity(tweenProgress));
        }

        private void EndTween()
        {
            tweening = false;
            HighlightTweenComplete?.Invoke();
        }

        private float Thickness(float tweenProgress)
        {
            return minThickness + thicknessCurve.Evaluate(tweenProgress) * thicknessRange;
        }

        private float Intensity(float tweenProgress)
        {
            return minIntensity + intensityCurve.Evaluate(tweenProgress) * intensityRange;
        }

        private void SetThickness(float val)
        {
            if (matHasThicknessParameter)
            {
                materialManager.SetFloat("_Thickness", val);
            }
        }

        protected virtual void SetIntensity(float val)
        {
            materialManager.SetFloat("_BloomIntensityMultiplier", val);
        }

        private void OnDestroy()
        {
            HighlightTweenComplete = null;
        }
    }
}
