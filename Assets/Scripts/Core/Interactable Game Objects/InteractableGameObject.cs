using UnityEngine;
using RPGPlatformer.SceneManagement;
using RPGPlatformer.UI;
using System;


namespace RPGPlatformer.Core
{
    //Game objects with MouseOver, MouseExit, and MouseDown callbacks

    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    public abstract class InteractableGameObject : MonoBehaviour, IInteractableGameObject, IPausable
    {
        [SerializeField] protected string displayName;
        [SerializeField] protected string examineText;
        [SerializeField] protected CursorType cursorType = CursorType.Default;
        [SerializeField] protected float maxInteractableDistance = 1.5f;

        protected int homeLayer;
        protected Transform playerTransform;

        private static InteractableGameObject hoveredIGO;

        public string DisplayName => $"<b>{displayName}</b>";
        public virtual string ExamineText => examineText;
        public virtual CursorType CursorType => cursorType;
        public bool MouseOver { get; protected set; }

        public event Action PlayerOutOfRange;

        public static InteractableGameObject HoveredIGO
        {
            get => hoveredIGO;
            protected set
            {
                hoveredIGO = value;
                HoveredIGOChanged?.Invoke();
            }
        }
        public static bool MouseOverAnyIGO => HoveredIGO != null;

        public static event Action HoveredIGOChanged;

        protected virtual void Awake()
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            playerTransform = playerGO.transform;
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

        protected virtual void OnMouseEnter()
        {
            MouseOver = true;
            HoveredIGO = this;
        }

        protected virtual void OnMouseExit()
        {
            MouseOver = false;
            if (!MouseOverAny())
            {
                HoveredIGO = null;
            }
        }

        protected abstract void OnMouseDown();

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
                HoveredIGO = null;
            }

            PlayerOutOfRange = null;
        }
    }
}