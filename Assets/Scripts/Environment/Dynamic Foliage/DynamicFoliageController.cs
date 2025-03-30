using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class DynamicFoliageController : MonoBehaviour
    {
        //[Range(0, 1)][SerializeField] float externalInfluence = 0;
        [SerializeField] float easeInTime = .15f;
        [SerializeField] float easeOutTime = .15f;
        [Min(0)][SerializeField] float minSpeedToTrigger = 0.5f;

        public float EaseInTime => easeInTime;
        public float EaseOutTime => easeOutTime;
        public float MinSpeedToTrigger => minSpeedToTrigger;

        int influenceVelocityProperty = Shader.PropertyToID("_InfluenceVelocity");
        int influenceOrientationProperty = Shader.PropertyToID("_InfluenceOrientation");

        public void SetFoliageInfluenceVelocity(Material mat, Vector2 velocity)
        {
            mat.SetVector(influenceVelocityProperty, velocity);
        }

        public void SetFoliageInfluenceOrientation(Material mat, float orientation)
        {
            mat.SetFloat(influenceOrientationProperty, orientation);
        }
    }
}
