using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using RPGPlatformer.Core;
using RPGPlatformer.SceneManagement;

namespace RPGPlatformer.UI
{
    using static UITools;

    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    //works both on UI and on game objects with colliders
    public abstract class RightClickMenuSpawner : MonoBehaviour, IPointerClickHandler, IPausable
    {
        [SerializeField] protected GameObject menuPrefab;
        [SerializeField] protected bool disableWhenPlayerIsDead = true;

        //protected GameObject activeMenu;
        protected Canvas targetCanvas;
        protected IInteractableGameObject igo;

        public GameObject ActiveMenu { get; protected set; }

        public event Action MenuSpawned;

        protected virtual void Awake()
        {
            targetCanvas = GetComponentInParent<Canvas>();
            if (!targetCanvas)
            {
                targetCanvas = GameObject.Find("Game UI Canvas").GetComponent<Canvas>();
            }

            igo = GetComponent<IInteractableGameObject>();
        }

        protected virtual void OnEnable()
        {
            if(TryGetComponent(out TooltipSpawner tts))
            {
                tts.TooltipSpawned += ClearMenu;
            }

            if (disableWhenPlayerIsDead)
            {
                GlobalGameTools.OnPlayerDeath += ClearMenu;
            }
        }

        public void Pause()
        {
            ClearMenu();
        }

        public void Unpause() { }

        public abstract bool CanCreateMenu();

        public abstract void ConfigureMenu(GameObject menu);

        public virtual void ClearMenu()
        {
            if (ActiveMenu)
            {
                foreach (Button button in GetComponentsInChildren<Button>())
                {
                    button.onClick.RemoveAllListeners();
                }
                Destroy(ActiveMenu);
                ActiveMenu = null;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.IsRightMouseButtonEvent())
            {
                OpenMenu();
            }
        }

        protected virtual void OnGUI()
        {
            if (ActiveMenu && Event.current.type == EventType.MouseUp)
            {
                GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
                if (currentSelected && currentSelected.transform.IsChildOf(ActiveMenu.transform)) return;
                if (igo != null && igo.MouseOver) return;

                ClearMenu();
            }
        }
        protected virtual void CreateAndConfigureButton(GameObject menu, Button buttonPrefab, string buttonText, 
            Action onClick, bool closeOnClick = true)
        {
            Button button = Instantiate(buttonPrefab, menu.transform);
            button.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;
            button.onClick.AddListener(() => onClick?.Invoke());
            if (closeOnClick)
            {
                button.onClick.AddListener(ClearMenu);
            }
        }

        private void OpenMenu()
        {
            if (!ActiveMenu && CanCreateMenu())
            {
                ActiveMenu = InstantiateMenuPrefab();
            }
            if (ActiveMenu)
            {
                ConfigureMenu(ActiveMenu);
                var rectT = ActiveMenu.GetComponent<RectTransform>();
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectT); 
                rectT.RepositionToFitInArea(targetCanvas.GetComponent<RectTransform>());
                //RepositionMenu();
                MenuSpawned?.Invoke();
            }
        }

        //NOTE: The menu prefab should have its pivot set in the UPPER LEFT CORNER
        //private void RepositionMenu()
        //{
        //    ActiveMenu.GetComponent<RectTransform>().RepositionToFitInArea(targetCanvas.GetComponent<RectTransform>());
        //}

        private GameObject InstantiateMenuPrefab()
        {
            return Instantiate(menuPrefab, GetPosition(), Quaternion.identity, targetCanvas.transform);
        }

        private Vector3 GetPosition()
        {
            return Camera.main.ScreenToWorldPoint(Input.mousePosition) + targetCanvas.transform.position.z * Vector3.forward;
        }

        private void OnDisable()
        {
            ClearMenu();
        }

        protected virtual void OnDestroy()
        {
            MenuSpawned = null;
        }
    }
}