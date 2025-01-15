using System;
using UnityEngine.EventSystems;

namespace RPGPlatformer.UI
{
    public class AbilityBookSlot : AbilityBarSlot, IPointerEnterHandler
    {
        public event Action PointerEnter;

        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerEnter?.Invoke();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            PointerEnter = null;
        }
    }
}