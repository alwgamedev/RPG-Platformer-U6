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

        protected GameObject activeMenu;
        protected Canvas targetCanvas;
        protected IInteractableGameObject igo;

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
                GlobalGameTools.Instance.OnPlayerDeath += ClearMenu;
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
            if (activeMenu)
            {
                foreach (Button button in GetComponentsInChildren<Button>())
                {
                    button.onClick.RemoveAllListeners();
                }
                Destroy(activeMenu);
                activeMenu = null;
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
            if (activeMenu && Event.current.type == EventType.MouseUp)
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

        private void OnMouseOver()
        {
            if (GlobalGameTools.PlayerIsDead && disableWhenPlayerIsDead) return;

            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                OpenMenu();
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
                RepositionMenu();
            }
        }

        //NOTE: The menu prefab should have its pivot set in the UPPER LEFT CORNER
        private void RepositionMenu()
        {
            activeMenu.GetComponent<RectTransform>().RepositionToFitInArea(targetCanvas.GetComponent<RectTransform>());
        }

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
    }
}