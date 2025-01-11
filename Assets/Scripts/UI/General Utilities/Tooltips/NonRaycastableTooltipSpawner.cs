using UnityEngine;

namespace RPGPlatformer.UI
{
    public abstract class NonRaycastableTooltipSpawner : TooltipSpawner
    {
        protected virtual void OnMouseEnter()
        {
            OnPointerEnter(null);
        }

        protected virtual void OnMouseExit()
        {
            OnPointerExit(null);
        }
    }
}