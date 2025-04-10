using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    [RequireComponent(typeof(TriggerColliderMessenger))]
    public class EarthwormWormhole : MonoBehaviour
    {
        [SerializeField] ParticleSystem emergeParticles;

        TriggerColliderMessenger trigger;

        public TriggerColliderMessenger Trigger => trigger;
        public ParticleSystem EmergeParticles => emergeParticles;

        private void Awake()
        {
            trigger = GetComponent<TriggerColliderMessenger>();
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            emergeParticles.Stop();
        }

        public void PlayEmergeEffect()
        {
            emergeParticles.Play();
            //may also add rocks bouncing out
        }
    }
}