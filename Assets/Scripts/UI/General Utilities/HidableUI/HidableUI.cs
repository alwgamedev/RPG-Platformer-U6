﻿using System;
using UnityEngine;
using RPGPlatformer.SceneManagement;
using System.Threading.Tasks;
using System.Threading;


namespace RPGPlatformer.UI
{

    [RequireComponent(typeof(CanvasGroup))]
    public class HidableUI : MonoBehaviour, IPausable
    {
        public enum PauseOptions
        {
            hide, show, ignore
        }

        public enum UnpauseOptions
        {
            hide, show, ignore, returnToPreviousState
        }

        public enum FadeState
        {
            none, fadeShow, fadeHide
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

        protected FadeState fadeState = FadeState.none;

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

        //This isn't actually a great idea -- if other scripts (e.g. on child game objects)
        //are subscribed to OnShow, then they might not have done Awake/Enable yet when Show is called
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

        public virtual async Task FadeShow(float time, CancellationToken token)
        {
            if (fadeState == FadeState.fadeShow || Visible) return;

            fadeState = FadeState.fadeShow;

            while (CanvasGroup.alpha < 1)
            {
                await Task.Yield();
                if (fadeState != FadeState.fadeShow) return;
                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                CanvasGroup.alpha += Time.deltaTime / time;
            }

            fadeState = FadeState.none;
            Show();
        }

        public virtual async Task FadeHide(float time, CancellationToken token)
        {
            if (fadeState == FadeState.fadeHide || !Visible) return;

            fadeState = FadeState.fadeHide;

            while (CanvasGroup.alpha > 0)
            {
                await Task.Yield();
                if (fadeState != FadeState.fadeHide) return;
                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                CanvasGroup.alpha -= Time.deltaTime / time;
            }

            fadeState = FadeState.none;
            Hide();
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