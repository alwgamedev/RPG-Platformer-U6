using UnityEngine;
using RPGPlatformer.SceneManagement;
using RPGPlatformer.UI;
using System;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using RPGPlatformer.Dialogue;
using RPGPlatformer.Saving;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Linq;


namespace RPGPlatformer.Core
{
    //Game objects with MouseOver, MouseExit, and MouseDown callbacks

    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    public class InteractableGameObject : MonoBehaviour, IInteractableGameObject, IPausable, 
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISavable, IOutOfBoundsHandler
    //why use pointer events instead of OnMouseOver etc.?
    //because pointer events are detected by trigger colliders, so we can expand the object's
    //interactable area by adding a trigger collider that won't affect its physics interactions
    //NOTE that this means we NEED to keep a Physics2D Raycaster on the main camera
    {
        [SerializeField] protected string displayName;
        [SerializeField] protected string examineText;
        [SerializeField] protected CursorType defaultCursorType = CursorType.Default;
        [SerializeField] protected float maxInteractableDistance = 1.5f;

        protected int igoLayer;
        protected int ignoreLayer;
        protected RightClickMenuSpawner rcm;
        protected Action OnUpdate;
        //protected Action leftClickAction;

        public string DisplayName => $"<b>{displayName}</b>";
        public virtual string ExamineText => examineText;
        public virtual CursorType CursorType { get; protected set; }
        public bool MouseOver { get; protected set; }
        public bool PlayerHasInteracted { get; protected set; }
        //^note this will only be triggered by left clicking the IGO
        //it doesn't make sense to single out right click actions that 
        //would trigger this (e.g. examine shouldn't, but others should)

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
            //if (rcm)
            //{
            //    rcm.MenuSpawned += SetHoveredIGOToNull;
            //}
            CursorType = defaultCursorType;
            igoLayer = LayerMask.NameToLayer("Interactable Game Object");
            ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
        }

        protected virtual void Update()
        {
            OnUpdate?.Invoke();
        }


        //INTERACTION FUNCTIONS

        //data is (Description, CanBeExecuted, Action)
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

        protected virtual void TriggerDialogue(IDialogueTrigger trigger, int index)
        {
            trigger.DialogueBeganSuccessfully += DialogueBeganHandler;

            void DialogueBeganHandler(bool success)
            {
                if (!success)
                {
                    trigger.DialogueBeganSuccessfully -= DialogueBeganHandler;
                    return;
                }

                OnUpdate = SendNotificationIfPlayerOutOfRange;
                PlayerOutOfRange += trigger.RequestCancelDialogue;
                trigger.DialogueEnded += DialogueEndedHandler;
                trigger.DialogueBeganSuccessfully -= DialogueBeganHandler;
            }

            void DialogueEndedHandler()
            {
                //when we have an IGO that can trigger different spawns (e.g. a dialogue, a pop-up, and a shop)
                //then we will need to be more careful about subbing/unsubbing...
                //or probably the action just can't be executed if other spawn is already active
                OnUpdate = null;
                PlayerOutOfRange -= trigger.RequestCancelDialogue;
                trigger.DialogueEnded -= DialogueEndedHandler;
            }

            trigger.TriggerDialogue(index);
        }


        //INTERACTABILITY

        public virtual bool PlayerCanInteract()
        {
            return !GlobalGameTools.Instance.PlayerIsDead && PlayerInRangeWithNotifications();
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

        //same thing but with VOID return type
        protected virtual void SendNotificationIfPlayerOutOfRange()
        {
            PlayerInRangeWithNotifications();
        }

        protected virtual bool PlayerInRange()
        {
            var playerTransform = GlobalGameTools.Instance.PlayerTransform;

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
                gameObject.layer = igoLayer;

            }
            else
            {
                //homeLayer = gameObject.layer;
                gameObject.layer = ignoreLayer;
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
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return false;
            }

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var colls = Physics2D.OverlapPointAll(mousePosition, LayerMask.GetMask("Interactable Game Object"));

            foreach (var c in colls)
            {
                if (c && (!HoveredIGO?.transform || !c.transform.IsChildOf(HoveredIGO.transform)) 
                    && c.TryGetComponent(out InteractableGameObject igo))
                {
                    igo.OnPointerEnter(null);
                }
                return true;
            }

            return false;
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
                if (!PlayerHasInteracted)
                {
                    PlayerHasInteracted = true;
                    OnFirstLeftClick();
                }
                OnLeftClick();
            }
        }

        protected virtual void OnFirstLeftClick() { }

        protected virtual void OnLeftClick()
        {
            Examine();
        }


        //ISAVABLE

        public JsonNode CaptureState()
        {
            return JsonSerializer.SerializeToNode(PlayerHasInteracted);
        }

        public void RestoreState(JsonNode jNode)
        {
            var b = JsonSerializer.Deserialize<bool>(jNode);
            PlayerHasInteracted = b;
        }


        //IOUTOFBOUNDSHANDLER

        public virtual void OnOutOfBounds()
        {
            if (gameObject)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (HoveredIGO == (IInteractableGameObject)this)
            {
                SetHoveredIGOToNull();
            }

            OnUpdate = null;
            PlayerOutOfRange = null;
        }
    }
}