using UnityEngine;
using RPGPlatformer.Movement;

namespace RPGPlatformer.Core
{
    public class OneSidedItem : MonoBehaviour
    {
        public HorizontalOrientation mySide;
        public SpriteRenderer itemSprite;

        private void Awake()
        {
            Mover mover = GetComponentInParent<Mover>();
            if (mover)
            {
                mover.UpdatedXScale += FlipSides;
            }
        }

        public void FlipSides(HorizontalOrientation moverOrientation)
        {
            if (itemSprite)
            {
                itemSprite.sortingOrder = (int)mySide * (int)moverOrientation * Mathf.Abs(itemSprite.sortingOrder);
            }
        }
    }
}
