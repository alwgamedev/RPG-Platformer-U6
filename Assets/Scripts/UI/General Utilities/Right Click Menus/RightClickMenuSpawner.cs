using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using RPGPlatformer.Core;
using RPGPlatformer.SceneManagement;

namespace RPGPlatformer.UI
{
    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    //works both on UI and on game objects with colliders
    public abstract class RightClickMenuSpawner : MonoBehaviour, /*IPointerEnterHandler, 
        IPointerExitHandler,*/ /*IPointerUpHandler,*/ IPointerDownHandler, IPausable
    {
        [SerializeField] protected GameObject menuPrefab;
        [SerializeField] protected bool disableWhenPlayerIsDead = true;

        //protected bool raycastable;
        //protected bool pointerOverThis;
        protected bool justSpawnedMenu;
        //protected bool justClearedMenuOnMouseDown;
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
        }

        protected virtual void OnEnable()
        {
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
            if (activeMenu != null)
            {
                Destroy(activeMenu);
                activeMenu = null;
            }
        }

        //public void OnPointerUp(PointerEventData eventData)
        //{
        //    if(justClearedMenuOnMouseDown)
        //    {
        //        justClearedMenuOnMouseDown = false;
        //        return;
        //    }
        //    if (eventData.IsRightMouseButtonEvent())
        //    {
        //        //Debug.Log("opening new rcm");
        //        OpenMenu();
        //    }
        //}

        public void OnPointerDown(PointerEventData eventData)
        {
            if (activeMenu != null)
            {
                //Debug.Log("pointer down but there's already an rcm. clearing menu");
                ClearMenu();
            }
            else if (eventData.IsRightMouseButtonEvent())
            {
                //Debug.Log("opening new rcm");
                OpenMenu();
            }
        }

        //public void OnPointerEnter(PointerEventData eventData)
        //{
        //    pointerOverThis = true;
        //}

        //public void OnPointerExit(PointerEventData eventData)
        //{
        //    pointerOverThis = false;
        //}

        //public void OnMouseDown()
        //{
        //    if (activeMenu != null)
        //    {
        //        Debug.Log("pointer down but there's already an rcm. clearing menu");
        //        ClearMenu();
        //    }
        //    else if (Input.GetKeyDown(KeyCode.Mouse1))
        //    {
        //        Debug.Log("opening new rcm");
        //        OpenMenu();
        //    }
        //}

        //public void OnMouseEnter()
        //{
        //    OnPointerDown(null);
        //}

        //public void OnMouseExit()
        //{
        //    OnPointerExit(null);
        //}

        protected virtual void OnGUI()
        {
            //if (Event.current.type == EventType.MouseDown && activeMenu != null)
            //{
            //    //we need to do this so that rcm buttons can be clicked without closing button
            //    //(we could also just destroy activeMenu after a delay instead... but then I worry about 
            //    //weirdness when you spam click something, so this just feels like the correct, albeit
            //    //not most performant, approach)
            //    GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            //    if (currentSelected && currentSelected.transform.IsChildOf(activeMenu.transform)) return;
            //    //if (igo != null && igo.MouseOver) return;//why do we need this one?

            //    ClearMenu();
            //    justClearedMenuOnMouseDown = true;
            //}
            /*else*/
            if (Event.current.type == EventType.MouseUp /*&& !pointerOverThis*/)
            {
                if(justSpawnedMenu)
                {
                    justSpawnedMenu = false;
                    return;
                }
                else if(activeMenu)
                {
                    GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
                    if (currentSelected && currentSelected.transform.IsChildOf(activeMenu.transform)) return;
                    if (igo != null && igo.MouseOver) return;
                    ClearMenu();
                }

                //justClearedMenuOnMouseDown = false;
                //because we want justClearedMenu to reset every time pointer comes back up
                //even if it's not over this object
            }
            //if (activeMenu && Event.current.type == EventType.MouseUp && !pointerOverThis)
            //{
            //    GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            //    if (currentSelected && currentSelected.transform.IsChildOf(activeMenu.transform)) return;
            //    if (igo != null && igo.MouseOver) return;

            //    ClearMenu();
            //}
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