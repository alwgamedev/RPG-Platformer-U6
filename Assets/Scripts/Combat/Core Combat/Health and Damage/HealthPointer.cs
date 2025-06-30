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
            if (!enabled) return null;
            //this is important for things like spider web barrier, where we need to be able to have
            //the health be unresponsive for some period (in that case, until the mother spider is killed)
            return health;
        }
    }
}