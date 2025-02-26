using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using RPGPlatformer.Core;
using RPGPlatformer.SceneManagement;
using UnityEngine.InputSystem;

namespace RPGPlatformer.UI
{
    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    //works both on UI and on game objects with colliders
    public abstract class RightClickMenuSpawner : MonoBehaviour, IPointerDownHandler, IPausable
    {
        [SerializeField] protected GameObject menuPrefab;
        [SerializeField] protected bool disableWhenPlayerIsDead = true;

        protected bool justSpawnedMenu;
        protected GameObject activeMenu;
        protected Canvas targetCanvas;
        protected IInteractableGameObject igo;

        public GameObject ActiveMenu => activeMenu;

        public event Action MenuSpawned;

        protected virtual void Awake()
        {
            targetCanvas = GetComponentInParent<Canvas>();
            if (!targetCanvas)
            {
                targetCanvas = GameObject.Find("Game UI Canvas").GetComponent<Canvas>();
            }

            igo = GetComponent<IInteractableGameObject>();

            if (disableWhenPlayerIsDead)
            {
                GlobalGameTools.OnPlayerDeath += ClearMenu;
            }

            if (SettingsManager.Instance && SettingsManager.Instance.IAM.actionMap != null)
            {
                OnIAMConfigure();
            }
            SettingsManager.OnIAMConfigure += OnIAMConfigure;
            //subscribe in either case, so that we are linked up to the latest action map whenever it
            //gets rebuilt (e.g. due to input bindings change or something)
        }

        protected void OnIAMConfigure()
        {
            var iam = SettingsManager.Instance.IAM;
            iam.LeftClickAction.canceled += CancelOnMouseUp;
            iam.RightClickAction.canceled += CancelOnMouseUp;
            SettingsManager.OnIAMConfigure -= OnIAMConfigure;
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
            if (activeMenu != null)
            {
                Destroy(activeMenu);
                activeMenu = null;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (activeMenu != null)
            {
                ClearMenu();
            }
            else if (eventData.IsRightMouseButtonEvent())
            {
                OpenMenu();
            }
        }

        protected void CancelOnMouseUp(InputAction.CallbackContext ctx)
        {
            if (justSpawnedMenu)
            {
                justSpawnedMenu = false;
                return;
            }
            else if (activeMenu)
            {
                GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
                if (currentSelected && currentSelected.transform.IsChildOf(activeMenu.transform)) return;
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
            if (!activeMenu && CanCreateMenu())
            {
                activeMenu = InstantiateMenuPrefab();
            }
            if (activeMenu)
            {
                ConfigureMenu(activeMenu);
                var rectT = activeMenu.GetComponent<RectTransform>();
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectT); 
                rectT.RepositionToFitInArea(targetCanvas.GetComponent<RectTransform>());

                justSpawnedMenu = true;
                MenuSpawned?.Invoke();
            }
        }

        private GameObject InstantiateMenuPrefab()
        {
            return Instantiate(menuPrefab, GetPosition(), Quaternion.identity, targetCanvas.transform);
        }

        private Vector3 GetPosition()
        {
            var p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            p.z = targetCanvas.transform.position.z;
            return p;
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