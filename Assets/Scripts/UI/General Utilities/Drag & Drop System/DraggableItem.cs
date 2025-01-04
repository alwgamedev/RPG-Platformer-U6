using UnityEngine;
using UnityEngine.EventSystems;

namespace RPGPlatformer.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggableItem<T> : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler where T : class
    {
        IDragSource<T> source;
        Canvas parentCanvas;
        CanvasGroup canvasGroup;

        private void Awake()
        {
            source = GetComponentInParent<IDragSource<T>>();
            parentCanvas = GetComponentInParent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        //TO-DO: Where do we set the new source??

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (source == null || !eventData.IsLeftMouseButtonEvent()) return;

            canvasGroup.blocksRaycasts = false;//or else the drop event doesn't register? (test it out)
            transform.SetParent(parentCanvas.transform, true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!eventData.IsLeftMouseButtonEvent()) return;
            transform.position = Camera.main.ScreenToWorldPoint((Vector3)eventData.position + parentCanvas.transform.position.z * Vector3.forward);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if(!eventData.IsLeftMouseButtonEvent()) return;
            if (source != null && source.Transform)
            {
                transform.SetParent(source.Transform);
                transform.localPosition = Vector3.zero;
            }
            canvasGroup.blocksRaycasts = true;

            IDropTarget<T> target;
            if (!EventSystem.current.IsPointerOverGameObject())//"IsPointerOverGameObject()" really means pointer is over UI (not just any game object)
            {
                target = parentCanvas.GetComponent<IDropTarget<T>>();
            }
            else
            {
                target = GetDropTarget(eventData);
            }

            DropItemOntoTarget(target);

            source?.DragComplete();
            target?.DropComplete();
        }

        private IDropTarget<T> GetDropTarget(PointerEventData eventData)
        {
            if (eventData.pointerEnter)
            {
                return eventData.pointerEnter.GetComponentInParent<IDropTarget<T>>();
            }
            return null;
        }

        private void DropItemOntoTarget(IDropTarget<T> target)
        {
            if (target == null || target == source)
            {
                return;
            }

            if (source != null && source is IDragDropSlot<T> sourceSlot && target is IDragDropSlot<T> targetSlot)
            {
                Swap(sourceSlot, targetSlot);
            }
            else if(target.AllowReplacementIfCantSwap)
            {
                ReplaceTargetItem(target);
            }
        }

        private void Swap(IDragDropSlot<T> source, IDragDropSlot<T> target)
        {
            T sourceItem = source.Contents();
            T targetItem = target.Contents();

            if (!source.CanPlace(targetItem) || !target.CanPlace(sourceItem)) return;

            source.RemoveItem();
            target.RemoveItem();
            source.PlaceItem(targetItem);
            target.PlaceItem(sourceItem);
        }

        private void ReplaceTargetItem(IDropTarget<T> target)
        {
            T sourceItem = source.Contents();

            if(!target.CanPlace(sourceItem)) return;

            source.RemoveItem();
            target.PlaceItem(sourceItem);
        }
    }
}