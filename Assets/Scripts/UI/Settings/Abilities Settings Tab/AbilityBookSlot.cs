using RPGPlatformer.Combat;
using System;
using UnityEngine.EventSystems;

namespace RPGPlatformer.UI
{
    public class AbilityBookSlot : AbilityBarSlot, IPointerEnterHandler
    {
        public event Action PointerEnter;

        public override bool AllowReplacementIfCantSwap => false;

        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerEnter?.Invoke();
        }

        public override bool CanPlace(AbilityBarItem item, IDragSource<AbilityBarItem> origin = null)
        {
            return false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            PointerEnter = null;
        }
    }
}