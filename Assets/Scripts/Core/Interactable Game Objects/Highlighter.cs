using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGPlatformer.Core
{
    [RequireComponent(typeof(MaterialManager))]
    public class Highlighter : MonoBehaviour
    {
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

        MaterialManager materialManager;
        float thicknessRange;
        float intensityRange;
        float tweenRate;//tween progress per second = timeScale / tweenTime
        float tweenProgress;//from 0-1
        bool tweening;

        bool testing;

        private void Awake()
        {
            materialManager = GetComponent<MaterialManager>();
            thicknessRange = maxThickness - minThickness;
            intensityRange = maxIntensity - minIntensity;
            tweenRate = intensityRange / tweenTime;
        }

        private void Start()
        {
            InstantMinHighlight();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                testing = !testing;
                EnableHighlight(testing);
            }

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

        public async Task HighlightFlash(float duration, CancellationToken token)
        {
            EnableHighlight(true);
            await MiscTools.DelayGameTime(duration, token);
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
            materialManager.SetFloat("_Thickness", val);
        }

        private void SetIntensity(float val)
        {
            materialManager.SetFloat("_BloomIntensityMultiplier", val);
        }
    }
}
