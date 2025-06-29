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

        public CursorManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                BuildCursorLookup();
                if (SettingsManager.Instance && SettingsManager.Instance.IAMIsConfigured)
                {
                    OnIAMConfigure();
                }
                SettingsManager.IAMConfigured += OnIAMConfigure;//still subscribe to get settings updates
            }
            else 
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (Instance != this) return;

            var player = GlobalGameTools.Instance.Player;
            player.OnChannelStarted += () =>
            {
                if (!player.HasStoredAction)
                //don't use aiming cursor just because you're waiting for an animation event
                {
                    EquipAnimatedCursor(focusingRedCrosshairs);
                }
            };
            player.OnChannelEnded += () =>
            {
                if (animatedCursorEquipped)
                {
                    EquipIGOHoverCursor(true);
                    //if no hovered igo this will equip default cursor.
                    //properly accounts for mouse down
                }
            };
            player.OnPowerUpStarted += () => EquipAnimatedCursor(blinkingYellowCrosshairs);
            player.OnMaximumPowerAchieved += () => EquipAnimatedCursor(blinkingGreenCrosshairs);

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
            if (Instance == this)
            {
                Instance = null;
            }

            InteractableGameObject.HoveredIGOChanged -= EquipIGOHoverCursor;
            SettingsManager.IAMConfigured -= OnIAMConfigure;
            OnUpdate = null;
        }
    }
}