using UnityEngine;
using RPGPlatformer.SceneManagement;
using RPGPlatformer.UI;
using System;
using UnityEngine.EventSystems;
using Unity.VisualScripting;


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
    {
        [SerializeField] protected string displayName;
        [SerializeField] protected string examineText;
        [SerializeField] protected CursorType cursorType = CursorType.Default;
        [SerializeField] protected float maxInteractableDistance = 1.5f;

        protected int homeLayer;
        protected Transform playerTransform;
        protected RightClickMenuSpawner rcm;

        private static InteractableGameObject hoveredIGO;

        public string DisplayName => $"<b>{displayName}</b>";
        public virtual string ExamineText => examineText;
        public virtual CursorType CursorType => cursorType;
        public bool MouseOver { get; protected set; }

        public event Action PlayerOutOfRange;

        public static InteractableGameObject HoveredIGO => hoveredIGO;

        //public static InteractableGameObject HoveredIGO
        //{
        //    get => hoveredIGO;
        //    protected set
        //    {
        //        hoveredIGO = value;
        //        HoveredIGOChanged?.Invoke();
        //    }
        //}
        public static bool MouseOverAnyIGO => HoveredIGO != null;

        public static event Action HoveredIGOChanged;
        public static event Action IGOClicked;

        protected virtual void Awake()
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            playerTransform = playerGO.transform;
            rcm = GetComponent<RightClickMenuSpawner>();
        }

        public virtual void Examine()
        {
            if(!string.IsNullOrEmpty(ExamineText))
            {
                GameLog.Log(ExamineText);
                //using the property ExamineText instead of field examineText
                //so that inheriting classes (like LootDrop) can have a forced examine text.
            }
        }

        //looks stupid, should just do in property HoveredIGO, but that property should be static,
        //so can't reference the non-static rcm in that property's set
        private void SetHoveredIGOToThis()
        {
            hoveredIGO = this;
            if(rcm == null || !rcm.ActiveMenu)
            {
                HoveredIGOChanged?.Invoke();
                //so that we don't get flickering cursor changes when we have RCM active
            }
        }

        private void SetHoveredIGOToNull()
        {
            hoveredIGO = null;
            HoveredIGOChanged?.Invoke();
        }


        //PLAYER IN RANGE TO INTERACT?

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

        protected virtual bool PlayerInRange()
        {
            if (playerTransform == null)
            {
                return false;
            }

            return Vector2.Distance(playerTransform.position, transform.position) < maxInteractableDistance;
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


        //MOUSE EVENT CALLBACKS

        //public virtual void OnMouseEnter()
        //{
        //    MouseOver = true;
        //    HoveredIGO = this;
        //}

        //public void OnMouseExit()
        //{
        //    MouseOver = false;
        //    if (!MouseOverAny())
        //    {
        //        HoveredIGO = null;
        //    }
        //}

        //public abstract void OnMouseDown();

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
        }

        private static bool MouseOverAny()
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Physics2D.OverlapCircleAll(mousePosition, 0.05f, LayerMask.GetMask("Interactable Game Elements")).Length > 0)
            {
                return true;
            }
            return false;
        }

        protected virtual void OnDestroy()
        {
            if (HoveredIGO == this)
            {
                SetHoveredIGOToNull();
            }

            PlayerOutOfRange = null;
        }
    }
}