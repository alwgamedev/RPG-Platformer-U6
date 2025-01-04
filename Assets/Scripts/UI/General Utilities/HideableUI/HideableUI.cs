using System;
using UnityEngine;
using RPGPlatformer.SceneManagement;


namespace RPGPlatformer.UI
{

    [RequireComponent(typeof(CanvasGroup))]
    public class HideableUI : MonoBehaviour, IPausable
    {
        public enum PauseOptions
        {
            hide, show, ignore
        }

        public enum UnpauseOptions
        {
            hide, show, ignore, returnToPreviousState
        }

        [SerializeField] protected bool showOnEnable = true;
        [SerializeField] protected bool useShowOnEnable = true;
        [SerializeField] protected bool interactableWhenShown;
        [SerializeField] protected bool blockRaycastsWhenShown;
        [SerializeField] protected PauseOptions pauseAction = PauseOptions.ignore;
        [SerializeField] protected UnpauseOptions unpauseAction = UnpauseOptions.ignore;

        protected Action PauseAction;
        protected Action UnpauseAction;

        protected bool visibleBeforePause;

        public CanvasGroup CanvasGroup { get; protected set; }
        public bool Visible { get; protected set; }

        public event Action OnShow;
        public event Action OnHide;

        protected virtual void Awake()
        {
            CanvasGroup = GetComponent<CanvasGroup>();

            ConfigurePauseAction();
            ConfigureUnpauseAction();
        }

        protected virtual void OnEnable()
        {
            if (useShowOnEnable)
            {
                if (showOnEnable)
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }
        }

        public virtual void Show()
        {
            CanvasGroup.alpha = 1;
            CanvasGroup.interactable = interactableWhenShown;
            CanvasGroup.blocksRaycasts = blockRaycastsWhenShown;
            Visible = true;
            OnShow?.Invoke();
        }

        public virtual void Hide()
        {
            CanvasGroup.alpha = 0;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            Visible = false;
            OnHide?.Invoke();
        }

        public virtual void SetInteractable(bool val)
        {
            CanvasGroup.interactable = val;
        }

        public virtual void Pause()
        {
            visibleBeforePause = Visible;

            PauseAction?.Invoke();
        }

        public virtual void Unpause()
        {
            UnpauseAction?.Invoke();
        }

        protected virtual void ConfigurePauseAction()
        {
            if (pauseAction == PauseOptions.hide)
            {
                PauseAction = Hide;
            }
            else if (pauseAction == PauseOptions.show)
            {
                PauseAction = Show;
            }
            else
            {
                PauseAction = null;
            }
        }

        protected virtual void ConfigureUnpauseAction()
        {
            if (unpauseAction == UnpauseOptions.hide)
            {
                UnpauseAction = Hide;
            }
            else if (unpauseAction == UnpauseOptions.show)
            {
                UnpauseAction = Show;
            }
            else if (unpauseAction == UnpauseOptions.returnToPreviousState)
            {
                UnpauseAction = ReturnToPreviousState;
            }
        }

        protected void ReturnToPreviousState()
        {
            if (visibleBeforePause)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        protected virtual void OnDestroy()
        {
            OnShow = null;
            OnHide = null;
            PauseAction = null;
            UnpauseAction = null;
        }
    }
}