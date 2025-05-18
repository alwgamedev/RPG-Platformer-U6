using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class GenericClimbableObject<T> : MonoBehaviour where T : GenericClimbNode<T>
    {
        [SerializeField] protected T[] nodes;//in descending order (0 is highest)

        //Collider2D[] colliders;

        protected virtual void Awake()
        {
            Configure();
        }

        protected virtual void Configure()
        {
            //colliders = new Collider2D[nodes.Length];

            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].SetAdjacentNodes(i > 0 ? nodes[i - 1] : null,
                    i < nodes.Length - 1 ? nodes[i + 1] : null);
                //colliders[i] = nodes[i].GetComponent<Collider2D>();
            }
        }
    }
}