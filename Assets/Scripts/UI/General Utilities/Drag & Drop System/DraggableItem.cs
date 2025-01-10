using RPGPlatformer.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPGPlatformer.UI
{
    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggableItem<T> : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
        where T : class
    {
        IDragSource<T> source;
        Canvas parentCanvas;
        CanvasGroup canvasGroup;
        bool canDrag = true;//mainly so that dragging gets cancelled when you pause

        private void Awake()
        {
            source = GetComponentInParent<IDragSource<T>>();
            parentCanvas = GetComponentInParent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void CancelDrag()
        {
            PointerEventData ed = new(EventSystem.current);
            OnEndDrag(ed);
            //cancels any ongoing drog by returning it to its source
            //(it will still call OnEndDrag again when you actually release the pointer)
        }

        public void DisableDragging()
        {
            canDrag = false;
            CancelDrag();
        }

        public void ReenableDragging()
        {
            canDrag = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (source == null || !eventData.IsLeftMouseButtonEvent()) return;

            canvasGroup.blocksRaycasts = false;//or else the drop event doesn't register? (test it out)
            transform.SetParent(parentCanvas.transform, true);
            Debug.Log("beginning drag");
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!canDrag || !eventData.IsLeftMouseButtonEvent()) return;
            transform.position = Camera.main.ScreenToWorldPoint((Vector3)eventData.position + parentCanvas.transform.position.z * Vector3.forward);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if(!eventData.IsLeftMouseButtonEvent() && canDrag) return;

            if (source != null)
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
            if(target == null) return;
            else if (source != null && source is IDragDropSlot<T> sourceSlot && target is IDragDropSlot<T> targetSlot)
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