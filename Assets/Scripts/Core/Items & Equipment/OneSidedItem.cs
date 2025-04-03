using UnityEngine;
using RPGPlatformer.Movement;

namespace RPGPlatformer.Core
{
    public class OneSidedItem : MonoBehaviour
    {
        public HorizontalOrientation mySide;
        public Renderer itemRenderer;

        private void Awake()
        {
            Mover mover = GetComponentInParent<Mover>();
            if (mover)
            {
                mover.DirectionChanged += FlipSides;
            }
        }

        public void FlipSides(HorizontalOrientation moverOrientation)
        {
            if (itemRenderer)
            {
                itemRenderer.sortingOrder = (int)mySide * (int)moverOrientation * Mathf.Abs(itemRenderer.sortingOrder);
            }
        }
    }
}
