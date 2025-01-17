using UnityEngine;
using UnityEngine.EventSystems;

namespace RPGPlatformer.UI
{
    //[RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggableItem<T> : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
        where T : class
    {
        [SerializeField] bool keepItemInSourceWhenReplacing;
            //i.e. if it cant swap and it goes to replace the target item, should the source remove its item
            //or keep a copy there? e.g. when dragging from ability book onto ability bar, we want to replace
            //the ability bar item, but also keep the item in the ability book
        [SerializeField] bool removeItemFromSourceOnFailedDrop;

        IDragSource<T> source;
        Canvas parentCanvas;
        CanvasGroup canvasGroup;
        bool canDrag = true;
        
        public bool CanDrag => canDrag && (source?.ItemCanBeDragged() ?? true);//mainly so that dragging gets cancelled when you pause
        public bool IsDragging { get; protected set; }

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
            if (IsDragging)
            {
                CancelDrag();
            }
        }

        public void ReenableDragging()
        {
            canDrag = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!CanDrag || !eventData.IsLeftMouseButtonEvent()) return;

            IsDragging = true;
            canvasGroup.blocksRaycasts = false;//or else the drop event doesn't register? (test it out)
            transform.SetParent(parentCanvas.transform, true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!CanDrag || !eventData.IsLeftMouseButtonEvent()) return;

            transform.position = Camera.main.ScreenToWorldPoint((Vector3)eventData.position
                + parentCanvas.transform.position.z * Vector3.forward);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if(!eventData.IsLeftMouseButtonEvent() && CanDrag) return;

            IsDragging = false;

            if (source != null)
            {
                transform.SetParent(source.DraggableParentTransform);
                transform.localPosition = Vector3.zero;
            }

            canvasGroup.blocksRaycasts = true;

            IDropTarget<T> target;
            if (!EventSystem.current.IsPointerOverGameObject())
                //"IsPointerOverGameObject()" really means pointer is over UI (not just any game object)
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
            if(target == null)
            {
                OnFailedDrop();
                return;
            }
            else if (source != null && source is IDragDropSlot<T> sourceSlot && target is IDragDropSlot<T> targetSlot)
            {
                if (!TrySwap(sourceSlot, targetSlot) && target.AllowReplacementIfCantSwap)
                {
                    TryReplaceTargetItem(target);
                }
            }
        }

        private bool TrySwap(IDragDropSlot<T> source, IDragDropSlot<T> target)
        {
            T sourceItem = source.Contents();
            T targetItem = target.Contents();

            if (!target.CanPlace(sourceItem, source))
            {
                OnFailedDrop();
                return false;
            }
            if (!source.CanPlace(targetItem, target))
            {
                return false;
            }

            source.RemoveItem();
            target.RemoveItem();
            source.PlaceItem(targetItem);
            target.PlaceItem(sourceItem);
            return true;
        }

        private bool TryReplaceTargetItem(IDropTarget<T> target)
        {
            T sourceItem = source.Contents();

            if(!target.CanPlace(sourceItem, source))
            {
                OnFailedDrop();
                return false;
            }

            if (!keepItemInSourceWhenReplacing)
                //in this case you may want to place a copy in the target instead... I think it'll be okay
            {
                source.RemoveItem();
            }
            target.PlaceItem(sourceItem);
            return true;
        }

        private void OnFailedDrop()
        {
            if (source != null && removeItemFromSourceOnFailedDrop)
            {
                source.RemoveItem();
            }
        }
    }
}