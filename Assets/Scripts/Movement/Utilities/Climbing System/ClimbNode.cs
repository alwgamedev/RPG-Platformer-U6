using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class ClimbNode : MonoBehaviour
    {
        public ClimbNode Higher { get; private set; }
        public ClimbNode Lower {  get; private set; }

        public void SetAdjacentNodes(ClimbNode higher, ClimbNode lower)
        {

        }
    }
}