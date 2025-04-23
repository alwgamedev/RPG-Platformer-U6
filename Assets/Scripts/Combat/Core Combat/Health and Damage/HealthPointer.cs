using UnityEngine;

namespace RPGPlatformer.Combat
{
    //attach to colliders that should be included when targeting a health
    //(e.g. for pillbug or earthworm where colliders are disconnected from the game object
    //containing the health component)
    public class HealthPointer : MonoBehaviour, IHealthPointer
    {
        [SerializeField] Health health;

        public IHealth HealthComponent()
        {
            return health;
        }
    }
}