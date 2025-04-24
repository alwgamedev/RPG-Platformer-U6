using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class ClimbableObject : MonoBehaviour
    {
        [SerializeField] ClimbNode[] nodes;//in descending order (0 is highest)

        private void Awake()
        {
            Configure();
        }

        private void Configure()
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].SetAdjacentNodes(i > 0 ? nodes[i - 1] : null,
                    i < nodes.Length - 1 ? nodes[i + 1] : null);
            }
        }
    }
}