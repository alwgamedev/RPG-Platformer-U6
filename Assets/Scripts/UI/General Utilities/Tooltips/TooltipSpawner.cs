using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using RPGPlatformer.SceneManagement;
using RPGPlatformer.Core;
using System.Threading;
using UnityEngine.InputSystem;

namespace RPGPlatformer.UI
{
    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    public abstract class TooltipSpawner : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPausable
    {
        [SerializeField] protected GameObject tooltipPrefab;
        [SerializeField] protected float spawnDelay;
        [SerializeField] protected float clearDelay;

        protected GameObject activeTooltip;
        protected Canvas targetCanvas;

        Action PointerEnter;
        Action PointerExit;
        Action TooltipClear;
        Action Destroyed;

        public GameObject ActiveTooltip => activeTooltip;

        public event Action TooltipSpawned;

        protected virtual void Awake()
        {
            targetCanvas = GetComponentInParent<Canvas>();
            if (!targetCanvas)
            {
                targetCanvas = GameObject.FindWithTag("Game UI Canvas").GetComponent<Canvas>();
            }

            if (spawnDelay > 0)
            {
                PointerEnter += async () => await DelayedSpawn(GlobalGameTools.Instance.TokenSource.Token);
            }

            TooltipClear = clearDelay > 0 ? 
                async () => await ClearTooltipDelayed(GlobalGameTools.Instance.TokenSource.Token) 
                : ClearTooltipImmediate;

            if (SettingsManager.Instance && SettingsManager.Instance.IAMIsConfigured)
            {
                OnIAMConfigure();
            }
            SettingsManager.IAMConfigured += OnIAMConfigure;//still subscribe to get settings updates
        }

        protected virtual void Start()
        {
            if (TryGetComponent(out RightClickMenuSpawner rcms))
            {
                rcms.MenuSpawned += ClearTooltipImmediate;
            }
        }

        protected void OnIAMConfigure()
        {
            var iam = SettingsManager.Instance.IAM;
            iam.InputAction(InputActionsManager.leftClickActionName).started += ClearOnMouseDown;
            iam.InputAction(InputActionsManager.rightClickActionName).started += ClearOnMouseDown;
        }

        public void Pause()
        {
            ClearTooltipImmediate();
        }

        public void Unpause() { }

        public virtual bool CanCreateTooltip()
        {
            if(TryGetComponent(out RightClickMenuSpawner rcms) && rcms && rcms.ActiveMenu)
            {
                return false;
            }
            return true;
        }

        public abstract void ConfigureTooltip(GameObject tooltip);

        public virtual void ClearTooltip()
        {
            TooltipClear?.Invoke();
        }

        public virtual void ClearTooltipImmediate()
        {
            if (activeTooltip)
            {
                Destroy(activeTooltip);
            }
            activeTooltip = null;
        }

        private async Task ClearTooltipDelayed(CancellationToken token)
        {
            if (!activeTooltip) return;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            try
            {
                Destroyed += cts.Cancel;
                await MiscTools.DelayGameTime(clearDelay, token);
                ClearTooltipImmediate();
            }
            catch (TaskCanceledException)
            {
                return;
            }
            finally
            {
                Destroyed -= cts.Cancel;
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            PointerEnter?.Invoke();

            if (spawnDelay == 0)
            {
                Spawn();
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            PointerExit?.Invoke();
            ClearTooltip();
        }

        private void RepositionTooltip()
        {
            activeTooltip.GetComponent<RectTransform>()
                .RepositionToFitInArea(targetCanvas.GetComponent<RectTransform>());
        }

        private GameObject InstantiateTooltipPrefab()
        {
            return Instantiate(tooltipPrefab, GetPosition(), Quaternion.identity, targetCanvas.transform);
        }

        private void Spawn()
        {
            if (!activeTooltip && CanCreateTooltip())
            {
                activeTooltip = InstantiateTooltipPrefab();
                if (activeTooltip)
                {
                    ConfigureTooltip(activeTooltip);
                    RepositionTooltip();
                    TooltipSpawned?.Invoke();
                }
            }
        }

        private async Task DelayedSpawn(CancellationToken token)
        {
            using var cts =
                CancellationTokenSource.CreateLinkedTokenSource(token);
            try
            {
                Destroyed += cts.Cancel;
                PointerExit += cts.Cancel;

                await MiscTools.DelayGameTime(spawnDelay, cts.Token);

                Spawn();
            }
            catch (TaskCanceledException)
            {
                return;
            }
            finally
            {
                PointerExit -= cts.Cancel;
                Destroyed -= cts.Cancel;
            }
        }

        protected virtual Vector3 GetPosition()
        {
            var p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            p.z = targetCanvas.transform.position.z;
            return p;
        }

        protected void ClearOnMouseDown(InputAction.CallbackContext ctx)
        {
            ClearTooltipImmediate();
        }

        private void OnDisable()
        {
            ClearTooltipImmediate();
        }

        protected virtual void OnDestroy()
        {
            Destroyed?.Invoke();

            PointerEnter = null;
            PointerExit = null;
            TooltipClear = null;
            TooltipSpawned = null;
            Destroyed = null;
            SettingsManager.IAMConfigured -= OnIAMConfigure;
        }
    }
}