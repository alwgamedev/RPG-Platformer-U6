using UnityEngine;

namespace RPGPlatformer.Inventory
{
    public class EquippableItemGO : MonoBehaviour
    {
        [SerializeField] Transform projectileAnchor;

        public Transform ProjectileAnchor => projectileAnchor;
    }
}