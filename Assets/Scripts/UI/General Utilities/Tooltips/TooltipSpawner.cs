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

        //protected GameObject activeTooltip;
        protected Canvas targetCanvas;
        //protected bool mouseOverSpawner;

        Action PointerEnter;
        Action PointerExit;

        public GameObject ActiveTooltip { get; protected set; }

        public event Action TooltipSpawned;

        protected virtual void Awake()
        {
            targetCanvas = GetComponentInParent<Canvas>();
            if (!targetCanvas)
            {
                targetCanvas = GameObject.Find("Game UI Canvas").GetComponent<Canvas>();
            }

            PointerEnter += async () => await DelayedSpawn();
        }

        protected virtual void OnEnable()
        {
            if (TryGetComponent(out RightClickMenuSpawner rcms))
            {
                rcms.MenuSpawned += ClearTooltip;
            }
        }

        public void Pause()
        {
            ClearTooltip();
        }

        public void Unpause() { }

        public virtual bool CanCreateTooltip()
        {
            if(TryGetComponent(out RightClickMenuSpawner rcms) && rcms.ActiveMenu)
            {
                return false;
            }
            return true;
        }

        public abstract void ConfigureTooltip(GameObject tooltip);

        public virtual void ClearTooltip()
        {
            if (ActiveTooltip)
            {
                Destroy(ActiveTooltip);
            }
            ActiveTooltip = null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(spawnDelay == 0)
            {
                Spawn();
                return;
            }

            PointerEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PointerExit?.Invoke();
            ClearTooltip();
        }

        //NOTE: The tooltip should have its pivot in UPPER RIGHT CORNER.
        private void RepositionTooltip()
        {
            ActiveTooltip.GetComponent<RectTransform>().RepositionToFitInArea(targetCanvas.GetComponent<RectTransform>());
        }

        private GameObject InstantiateTooltipPrefab()
        {
            return Instantiate(tooltipPrefab, GetPosition(), Quaternion.identity, targetCanvas.transform);
        }

        private void Spawn()
        {
            if (!ActiveTooltip && CanCreateTooltip())
            {
                ActiveTooltip = InstantiateTooltipPrefab();
                if (ActiveTooltip)
                {
                    ConfigureTooltip(ActiveTooltip);
                    RepositionTooltip();
                    TooltipSpawned?.Invoke();
                }
            }
        }

        private async Task DelayedSpawn()
        {
            using (var cts =
                CancellationTokenSource.CreateLinkedTokenSource(GlobalGameTools.Instance.TokenSource.Token))
            {
                try
                {
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
                }
            }
        }

        protected virtual Vector3 GetPosition()
        {
            return Camera.main.ScreenToWorldPoint(Input.mousePosition) + targetCanvas.transform.position.z * Vector3.forward;
        }

        protected virtual void OnGUI()
        {
            if (ActiveTooltip && Event.current.type == EventType.MouseUp)
            {
                ClearTooltip();
            }
        }

        private void OnDisable()
        {
            ClearTooltip();
        }

        protected virtual void OnDestroy()
        {
            PointerEnter = null;
            PointerExit = null;
            TooltipSpawned = null;
        }
    }
}