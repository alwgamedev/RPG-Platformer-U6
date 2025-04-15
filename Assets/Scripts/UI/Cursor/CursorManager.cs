using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.UI
{
    public enum CursorType
    {
        Default, Dialogue, Loot, EnterDoor, Inspect//OpenShop, Bank
    }

    public class CursorManager : MonoBehaviour
    {
        //[SerializeField] Texture2D defaultCursor;
        //[SerializeField] Texture2D defaultCursorClicked;
        //[SerializeField] Texture2D dialogueCursor;
        //[SerializeField] Texture2D dialogueCursorClicked;
        //[SerializeField] Texture2D lootCursor;
        //[SerializeField] Texture2D lootCursorClicked;
        //[SerializeField] Texture2D enterDoorCursor;//maybe also for portals
        //[SerializeField] Texture2D enterDoorCursorClicked;
        [SerializeField] CursorData defaultCursor;
        [SerializeField] CursorData dialogueCursor;
        [SerializeField] CursorData lootCursor;
        [SerializeField] CursorData enterDoorCursor;//maybe also for portals
        [SerializeField] CursorData inspectCursor;
        [SerializeField] AnimatedCursorSO focusingRedCrosshairs;
        [SerializeField] AnimatedCursorSO blinkingGreenCrosshairs;
        [SerializeField] AnimatedCursorSO blinkingYellowCrosshairs;

        Action OnUpdate;
        Dictionary<CursorType, CursorData> CursorLookup = new();
        bool animatedCursorEquipped;
        CursorType currentCursorType;

        private void Awake()
        {
            BuildCursorLookup();

            SettingsManager.IAMConfigured += OnIAMConfigure;
        }

        private void Start()
        {
            ICombatController playerCombatController = GameObject.Find("Player").GetComponent<ICombatController>();
            playerCombatController.OnChannelStarted += () =>
            {
                EquipAnimatedCursor(focusingRedCrosshairs);
            };
            playerCombatController.OnChannelEnded += () =>
            {
                if (animatedCursorEquipped)
                {
                    EquipIGOHoverCursor(true);
                    //if no hovered igo this will equip default cursor.
                    //properly accounts for mouse down
                }
            };
            playerCombatController.OnPowerUpStarted += () => EquipAnimatedCursor(blinkingYellowCrosshairs);
            playerCombatController.OnMaximumPowerAchieved += () => EquipAnimatedCursor(blinkingGreenCrosshairs);

            InteractableGameObject.HoveredIGOChanged += EquipIGOHoverCursor;

            EquipCursor(CursorType.Default);

        }

        private void Update()
        {
            OnUpdate?.Invoke();
        }

        private void OnIAMConfigure()
        {
            var iam = SettingsManager.Instance.IAM;
            iam.InputAction(InputActionsManager.leftClickActionName).started += ctx => HandleMouseClick(true);
            iam.InputAction(InputActionsManager.rightClickActionName).started += ctx => HandleMouseClick(true);
            iam.InputAction(InputActionsManager.leftClickActionName).canceled += ctx =>
            {
                if (!iam.HeldDown(InputActionsManager.rightClickActionName))
                {
                    HandleMouseClick(false);
                }
            };
            iam.InputAction(InputActionsManager.rightClickActionName).canceled += ctx =>
            {
                if (!iam.HeldDown(InputActionsManager.leftClickActionName))
                {
                    HandleMouseClick(false);
                }
            };
        }

        private void HandleMouseClick(bool mouseDown)
        {
            EquipCursor(currentCursorType, mouseDown);
        }

        private void EquipIGOHoverCursor()
        {
            EquipIGOHoverCursor(false);
        }

        private void EquipIGOHoverCursor(bool overrideAnimatedCursor = false)
        {
            var igo = InteractableGameObject.HoveredIGO; 
            var iam = SettingsManager.Instance.IAM;
            bool clicked = iam != null && (iam.HeldDown(InputActionsManager.leftClickActionName) 
                || iam.HeldDown(InputActionsManager.rightClickActionName));

            if (igo != null)
            {
                EquipCursor(igo.CursorType, clicked, overrideAnimatedCursor);
            }
            else
            {
                EquipCursor(CursorType.Default, clicked, overrideAnimatedCursor);
            }
        }

        private void EquipCursor(CursorType cursorType, bool clicked = false, 
            bool allowOverrideAnimatedCursor = false)
        {
            if (animatedCursorEquipped && !allowOverrideAnimatedCursor) return;

            if (CursorLookup.TryGetValue(cursorType, out var cursorData))
            {
                currentCursorType = cursorType;
                EquipStaticCursor(cursorData, clicked);
            }
        }

        private void EquipStaticCursor(CursorData data, bool clicked)
        {
            OnUpdate = null;
            animatedCursorEquipped = false;
            var tex = clicked ? data.ClickedTexture : data.Texture;
            Cursor.SetCursor(tex, data.Hotspot, CursorMode.ForceSoftware);
        }

        private void EquipAnimatedCursor(AnimatedCursor animatedCursor)
        {
            if (animatedCursor == null || animatedCursor.elements.Length == 0) return;

            animatedCursorEquipped = true;
            animatedCursor.Reset();
            OnUpdate = () =>
            {
                if (animatedCursor.MoveNext())
                {
                    Cursor.SetCursor(animatedCursor.CurrentTexture, animatedCursor.Hotspot, CursorMode.ForceSoftware);
                }
            };
        }

        private void EquipAnimatedCursor(AnimatedCursorSO animatedCursorSO)
        {
            EquipAnimatedCursor(animatedCursorSO.animation);
        }

        private void BuildCursorLookup()
        {
            CursorLookup.Clear();
            CursorLookup.Add(CursorType.Default, defaultCursor);
            CursorLookup.Add(CursorType.Dialogue, dialogueCursor);
            CursorLookup.Add(CursorType.Loot, lootCursor);
            CursorLookup.Add(CursorType.EnterDoor, enterDoorCursor);
            CursorLookup.Add(CursorType.Inspect, inspectCursor);
        }

        private void OnDestroy()
        {
            InteractableGameObject.HoveredIGOChanged -= EquipIGOHoverCursor;
            OnUpdate = null;
        }
    }
}