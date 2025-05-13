using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class EvilRoot : MonoBehaviour
    {
        [SerializeField] Collider2D[] contactColliders;

        int playerLayer;

        private void Awake()
        {
            playerLayer = LayerMask.GetMask("Player");
        }

        //we may want to get rid of collision entirely, because it doesn't work well with the IK
        //(because you stretch the middle of the root out when pushed, while head stays glued to IK target;
        //maybe we could just make the root heavy enough that you can only push it a little,
        //or have it be much, much slower to lerp to IK target (to the point that you can push head away from IK target
        //and see it gradually come back))

        private void EnableCollisionWithPlayer(bool val)
        {
            if (val)
            {
                foreach (var c in contactColliders)
                {
                    c.excludeLayers = c.excludeLayers & ~playerLayer;
                }
            }
            else
            {
                foreach (var c in contactColliders)
                {
                    c.excludeLayers = c.excludeLayers | playerLayer;
                }
            }
        }
    }
}