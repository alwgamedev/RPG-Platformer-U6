using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using RPGPlatformer.SceneManagement;
using RPGPlatformer.Core;
using System.Threading;

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
        //protected bool mouseOverSpawner;

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
                targetCanvas = GameObject.Find("Game UI Canvas").GetComponent<Canvas>();
            }

            if (spawnDelay > 0)
            {
                PointerEnter += async () => await DelayedSpawn(GlobalGameTools.Instance.TokenSource.Token);
            }

            TooltipClear = clearDelay > 0 ? 
                async () => await ClearTooltipDelayed(GlobalGameTools.Instance.TokenSource.Token) 
                : ClearTooltipImmediate;
        }

        protected virtual void Start()
        {
            if (TryGetComponent(out RightClickMenuSpawner rcms))
            {
                rcms.MenuSpawned += ClearTooltipImmediate;
            }
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

        //private void OnMouseEnter()
        //{
        //    OnPointerEnter(null);
        //}

        //private void OnMouseExit()
        //{
        //    OnPointerExit(null);
        //}

        //NOTE: The tooltip should have its pivot in UPPER RIGHT CORNER.
        private void RepositionTooltip()
        {
            activeTooltip.GetComponent<RectTransform>().RepositionToFitInArea(targetCanvas.GetComponent<RectTransform>());
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
            return Camera.main.ScreenToWorldPoint((Vector2)Input.mousePosition) 
                + targetCanvas.transform.position.z * Vector3.forward;
        }

        protected virtual void OnGUI()
        {
            if (activeTooltip && Event.current.type == EventType.MouseDown)
            {
                ClearTooltipImmediate();
            }
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
        }
    }
}