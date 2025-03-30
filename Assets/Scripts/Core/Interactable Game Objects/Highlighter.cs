using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGPlatformer.Core
{
    [RequireComponent(typeof(MaterialManager))]
    public class Highlighter : MonoBehaviour
    {
        [SerializeField] float minHighlightIntensity;//almost always 0
        [SerializeField] float maxHighlightIntensity = 5;
        [SerializeField] float tweenTime = 1;//seconds to go from min to max highlight

        MaterialManager materialManager;
        float intensityRange;
        float tweenRate;
        int tweenTimeScale;
        bool tweening;

        bool testing;

        private void Awake()
        {
            materialManager = GetComponent<MaterialManager>();
            intensityRange = maxHighlightIntensity - minHighlightIntensity;
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
            SetIntensity(maxHighlightIntensity);
        }

        public void InstantMinHighlight()
        {
            EndTween();
            SetIntensity(minHighlightIntensity);
        }

        public void EnableHighlight(bool val)
        {
            BeginTween(val ? 1 : -1);
        }

        public async Task HighlightFlash(float duration, CancellationToken token)
        {
            EnableHighlight(true);
            await MiscTools.DelayGameTime(duration, token);
            EnableHighlight(false);
        }

        private bool CanBeginTween(int timeScale)
        {
            if (timeScale == 0)
            {
                return false;
            }

            var val = materialManager.GetFloat("_BloomIntensityMultiplier");
            if (timeScale > 0 && val >= maxHighlightIntensity)
            {
                return false;
            }
            if (timeScale < 0 && val <= minHighlightIntensity)
            {
                return false;
            }

            return true;
        }

        //note that this automatically replaces any ongoing tween, which is a nice simple way to do it
        //however if you start a new tween that replaces the "Up" half of a highlight flash,
        //the "Down" half can come back in after a delay and replace your new tween,
        //so you should give the highlight flash an appropriate cancellation token if you want to prevent this
        private void BeginTween(int timeScale)
        {
            if (!CanBeginTween(timeScale)) return;

            tweenTimeScale = timeScale;
            tweening = true;
        }

        private void UpdateTween()
        {
            var val = GetIntensity();
            val += tweenTimeScale * tweenRate * Time.deltaTime;

            if ((tweenTimeScale > 0 && val > maxHighlightIntensity)
                || (tweenTimeScale < 0 && val < minHighlightIntensity))
            {
                val = Mathf.Clamp(val, minHighlightIntensity, maxHighlightIntensity);
                EndTween();
            }

            SetIntensity(val);
        }

        private void EndTween()
        {
            tweening = false;
        }

        private float GetIntensity()
        {
            return materialManager.GetFloat("_BloomIntensityMultiplier");
        }

        private void SetIntensity(float val)
        {
            materialManager.SetFloat("_BloomIntensityMultiplier", val);
        }

        //void Start()
        //{
        //    //ac = GetComponent<AnimationControl>();
        //    //ResetHighlight();
        //}

        //public void EnableHighlight(bool val)
        //{
        //    ac.SetFloat("animSpeed", val ? 1 : -1);
        //    var time = ac.CurrentStateInfo("Base Layer").normalizedTime;
        //    time = Mathf.Clamp(time, 0.01f, .99f);
        //    ac.PlayAnimationState("Highlight", "Base Layer", time);
        //}

        //public void ResetHighlight()
        //{
        //    ac.SetFloat("animSpeed", -1);
        //    ac.PlayAnimationState("Highlight", "Base Layer", 0);
        //}

        //public async Task HighlightFlash(float duration)
        //{
        //    EnableHighlight(true);
        //    await MiscTools.DelayGameTime(duration, GlobalGameTools.Instance.TokenSource.Token);
        //    EnableHighlight(false);
        //}
    }
}
