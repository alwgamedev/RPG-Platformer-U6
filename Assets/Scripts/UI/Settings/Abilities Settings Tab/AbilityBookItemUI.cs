using System;
using UnityEngine.EventSystems;

namespace RPGPlatformer.UI
{
    public class AbilityBookItemUI : AbilityBarItemUI, IPointerEnterHandler
    {
        public event Action PointerEnter;

        protected override void Awake() { }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerEnter?.Invoke();
        }

        protected override void OnDestroy()
        {
            PointerEnter = null;
        }
    }
}