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
            //subscribe in either case, so that we are linked up to the latest action map whenever it
            //gets rebuilt (e.g. due to input bindings change or something)
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
            iam.InputAction(InputActionsManager.leftClickActionName).canceled += CancelOnMouseUp;
            iam.InputAction(InputActionsManager.rightClickActionName).canceled += CancelOnMouseUp;
            SettingsManager.IAMConfigured -= OnIAMConfigure;
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