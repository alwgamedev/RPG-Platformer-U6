using UnityEngine;

namespace RPGPlatformer.Core
{
    public class IKLimbAnimatorManager : MonoBehaviour
    {
        [SerializeField] Rigidbody2D body;
        [SerializeField] IKLimbAnimator[] limbs;
        [SerializeField] float speedLerpRate;
        //[SerializeField] float speedThreshold;

        float smoothedSpeed;

        private void FixedUpdate()
        {
            smoothedSpeed = Mathf.Lerp(smoothedSpeed, body.linearVelocity.magnitude, Time.deltaTime * speedLerpRate);
            if (/*smoothedSpeed > speedThreshold &&*/ limbs != null)
            {
                foreach (var l in limbs)
                {
                    if (l)
                    {
                        l.UpdateTimer(smoothedSpeed);
                    }
                }
            }
        }
    }
}