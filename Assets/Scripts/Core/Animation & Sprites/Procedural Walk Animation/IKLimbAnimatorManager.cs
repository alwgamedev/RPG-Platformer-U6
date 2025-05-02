using UnityEngine;

namespace RPGPlatformer.Core
{
    public class IKLimbAnimatorManager : MonoBehaviour
    {
        [SerializeField] Rigidbody2D body;
        [SerializeField] IKLimbAnimator[] limbs;
        [SerializeField] float speedLerpRate;

        float smoothedSpeed;

        private void Update()
        {
            smoothedSpeed = Mathf.Lerp(smoothedSpeed, body.linearVelocity.magnitude, Time.deltaTime * speedLerpRate);
            if (limbs != null)
            {
                foreach (var l in limbs)
                {
                    if (l && !l.paused)
                    {
                        l.UpdateTimer(smoothedSpeed);
                    }
                }
            }
        }
    }
}