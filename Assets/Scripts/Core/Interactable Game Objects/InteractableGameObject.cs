using UnityEngine;
using RPGPlatformer.SceneManagement;
using RPGPlatformer.UI;
using System;
using UnityEngine.EventSystems;
using System.Collections.Generic;


namespace RPGPlatformer.Core
{
    //Game objects with MouseOver, MouseExit, and MouseDown callbacks

    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    public class InteractableGameObject : MonoBehaviour, IInteractableGameObject, IPausable, 
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    //why use pointer events instead of OnMouseOver etc.?
    //because pointer events are detected by trigger colliders, so we can expand the object's
    //interactable area by adding a trigger collider that won't affect its physics interactions
    //NOTE that this means we NEED to keep a Physics2D Raycaster on the main camera
    {
        [SerializeField] protected string displayName;
        [SerializeField] protected string examineText;
        [SerializeField] protected CursorType defaultCursorType = CursorType.Default;
        [SerializeField] protected float maxInteractableDistance = 1.5f;

        protected int homeLayer;
        protected RightClickMenuSpawner rcm;
        //protected Action leftClickAction;

        public string DisplayName => $"<b>{displayName}</b>";
        public virtual string ExamineText => examineText;
        public virtual CursorType CursorType { get; protected set; }
        public bool MouseOver { get; protected set; }

        public event Action PlayerOutOfRange;

        //HoveredIGO should only be set through the SetHoveredIGO methods
        //and keeping set private because we don't want any setting beyond what this base class does automatically
        public static IInteractableGameObject HoveredIGO { get; private set; }
        public static bool MouseOverAnyIGO => HoveredIGO != null;

        public static event Action HoveredIGOChanged;
        public static event Action IGOClicked;
        //^used so that player combat is cancelled appropriately when igo is clicked

        protected virtual void Awake()
        {
            rcm = GetComponent<RightClickMenuSpawner>();
            CursorType = defaultCursorType;
        }


        //INTERACTION

        //maybe should be (string, Func<bool>, Action) where the func<bool> will determine if the action can be called
        //e.g. you still want dialogue to show up in an RCM but the player is too far away to engage in dialogue
            //^but in that case the dialogue window would close immediately (workable but not ideal)
        //...for now we can just use the public "PlayerCanInteract" method for all interaction options
        public virtual IEnumerable<(string, Func<bool>, Action)> InteractionOptions()
        {
            if (!string.IsNullOrEmpty(ExamineText))
            {
                yield return ($"Examine {DisplayName}", () => true,  Examine);
            }
        }

        public virtual void Examine()
        {
            if (!string.IsNullOrEmpty(ExamineText))
            {
                GameLog.Log(ExamineText);
                //using the property ExamineText instead of field examineText
                //so that inheriting classes (like LootDrop) can override get to have a forced examine text
                //(that ignore the examineText field)
            }
        }


        //INTERACTABILITY

        public virtual bool PlayerCanInteract()
        {
            return !GlobalGameTools.PlayerIsDead && PlayerInRangeWithNotifications();
        }

        protected virtual bool PlayerInRangeWithNotifications()
        {
            if (!PlayerInRange())
            {
                OnPlayerOutOfRange();
                PlayerOutOfRange?.Invoke();
                return false;
            }
            return true;
        }

        protected virtual void SendNotificationIfPlayerOutOfRange()
        {
            PlayerInRangeWithNotifications();
        }

        protected virtual bool PlayerInRange()
        {
            var playerTransform = GlobalGameTools.PlayerTransform;

            if (playerTransform == null)
            {
                return false;
            }

            return Vector2.SqrMagnitude(playerTransform.position - transform.position) 
                < maxInteractableDistance * maxInteractableDistance;
        }

        protected virtual void OnPlayerOutOfRange() { }


        //PAUSE AND DISABLING

        public void Pause()
        {
            SetInteractable(false);
        }

        public void Unpause()
        {
            SetInteractable(true);
        }

        public void SetInteractable(bool val)
        {
            if (val)
            {
                gameObject.layer = homeLayer;

            }
            else
            {
                homeLayer = gameObject.layer;
                gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }

        //MANAGE HOVERED STATUS

        private void SetHoveredIGOToThis()
        {
            HoveredIGO = this;
            //so that we don't get flickering cursor changes when we have RCM active
            if (rcm == null || !rcm.ActiveMenu)
            {
                HoveredIGOChanged?.Invoke();
            }
        }

        private void SetHoveredIGOToNull()
        {
            HoveredIGO = null;
            HoveredIGOChanged?.Invoke();
        }

        private static bool MouseOverAny()
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return Physics2D.OverlapCircleAll(mousePosition, 0.05f, LayerMask.GetMask("Interactable Game Elements"))
                .Length != 0;
        }


        //MOUSE EVENT HANDLERS

        //NOTE: will not trigger on an inactive game object (even though trigger colliders still send events)
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            MouseOver = true;
            SetHoveredIGOToThis();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            MouseOver = false;
            if (!MouseOverAny())
            {
                SetHoveredIGOToNull();
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            IGOClicked.Invoke();

            if (eventData.IsLeftMouseButtonEvent() && PlayerCanInteract())
            {
                OnLeftClick();
            }
        }

        protected virtual void OnLeftClick() { }

        protected virtual void OnDestroy()
        {
            if (HoveredIGO == (IInteractableGameObject)this)
            {
                SetHoveredIGOToNull();
            }

            PlayerOutOfRange = null;
        }
    }
}