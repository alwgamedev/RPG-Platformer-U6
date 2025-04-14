using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using RPGPlatformer.Core;
using RPGPlatformer.SceneManagement;

namespace RPGPlatformer.UI
{
    enum TargetCanvas
    {
        parent, gameUI, child
    }

    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    //works both on UI and on game objects with colliders
    public abstract class RightClickMenuSpawner : MonoBehaviour, IPointerDownHandler, IPausable
    {
        [SerializeField] TargetCanvas targetCanvasSource;
        [SerializeField] protected GameObject menuPrefab;
        [SerializeField] protected Button menuButtonPrefab;
        [SerializeField] protected bool disableWhenPlayerIsDead = true;

        protected bool justSpawnedMenu;
        protected GameObject activeMenu;
        protected Canvas targetCanvas;

        public GameObject ActiveMenu => activeMenu;

        public event Action MenuSpawned;

        protected virtual void Awake()
        {
            FindTargetCanvas();

            if (disableWhenPlayerIsDead)
            {
                GlobalGameTools.OnPlayerDeath += ClearMenu;
            }

            SettingsManager.IAMConfigured += OnIAMConfigure;
        }

        protected virtual void FindTargetCanvas()
        {
            switch(targetCanvasSource)
            {
                case TargetCanvas.parent:
                    targetCanvas = GetComponentInParent<Canvas>(); 
                    break;
                case TargetCanvas.gameUI:
                    var guc = GameObject.FindWithTag("Game UI Canvas");
                    if (guc)
                    {
                        targetCanvas = guc.GetComponent<Canvas>();
                    }
                    break;
                case TargetCanvas.child:
                    targetCanvas = GetComponentInChildren<Canvas>();
                    break;
            }
        }

        protected void OnIAMConfigure()
        {
            var iam = SettingsManager.Instance.IAM;
            //just in case we are already subscribed, unsubscribe first
            //(IAM will reconfigure every time input settings change
            //-- the input actions will be brand new ones, but just being safe here)
            iam.InputAction(InputActionsManager.leftClickActionName).canceled -= ClearOnMouseUp;
            iam.InputAction(InputActionsManager.rightClickActionName).canceled -= ClearOnMouseUp;
            iam.InputAction(InputActionsManager.leftClickActionName).canceled += ClearOnMouseUp;
            iam.InputAction(InputActionsManager.rightClickActionName).canceled += ClearOnMouseUp;
            //SettingsManager.IAMConfigured -= OnIAMConfigure;
        }

        public void Pause()
        {
            ClearMenu();
        }

        public void Unpause() { }

        public virtual bool CanCreateMenu()
        {
            return menuPrefab && menuButtonPrefab;
        }

        public abstract void ConfigureMenu(GameObject menu);

        public virtual void ClearMenu()
        {
            if (ActiveMenu != null)
            {
                Destroy(activeMenu);
                activeMenu = null;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (activeMenu != null)//note that this only registers when you click inside the spawner area
            {
                ClearMenu();
            }
            else if (eventData.IsRightMouseButtonEvent())
            {
                OpenMenu();
            }
        }

        protected void ClearOnMouseUp(InputAction.CallbackContext ctx)
        {
            if (justSpawnedMenu)//this allows you to mouse up without closing the menu immediately after you open it
            {
                justSpawnedMenu = false;
                return;
            }
            else if (activeMenu != null)
            {
                //don't close if you're interacting with the menu
                GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
                if (activeMenu && currentSelected && currentSelected.transform.IsChildOf(activeMenu.transform))
                    return;

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