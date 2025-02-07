using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class DynamicFoliageController : MonoBehaviour
    {
        [Range(0, 1)][SerializeField] float externalInfluence = 0;
        [SerializeField] float easeInTime = .15f;
        [SerializeField] float easeOutTime = .15f;
        [Min(0)][SerializeField] float minSpeedToTrigger = 0.5f;

        public float EaseInTime => easeInTime;
        public float EaseOutTime => easeOutTime;
        public float MinSpeedToTrigger => minSpeedToTrigger;

        int externalInfluenceProperty = Shader.PropertyToID("_ExternalInfluence");

        public void SetFoliageInfluence(Material mat, float xVelocity)
        {
            mat.SetFloat(externalInfluenceProperty, xVelocity);
        }
    }
}
