using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class ClimbableObject : GenericClimbableObject<ClimbNode>
    {
        //[SerializeField] ClimbNode[] nodes;//in descending order (0 is highest)

        //Collider2D[] colliders;

        //private void Awake()
        //{
        //    Configure();
        //}

        //private void Configure()
        //{
        //    colliders = new Collider2D[nodes.Length];

        //    for (int i = 0; i < nodes.Length; i++)
        //    {
        //        nodes[i].SetAdjacentNodes(i > 0 ? nodes[i - 1] : null,
        //            i < nodes.Length - 1 ? nodes[i + 1] : null);
        //        colliders[i] = nodes[i].GetComponent<Collider2D>();
        //    }
        //}
    }
}