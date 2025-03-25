using System;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using UnityEditor;

namespace RPGPlatformer.UI
{
    public enum CursorType
    {
        Default, Dialogue, Loot, EnterDoor, //OpenShop, Bank
    }

    public class CursorManager : MonoBehaviour
    {
        [SerializeField] Texture2D defaultCursor;
        [SerializeField] Texture2D defaultCursorClicked;
        [SerializeField] Texture2D dialogueCursor;
        [SerializeField] Texture2D dialogueCursorClicked;
        [SerializeField] Texture2D lootCursor;
        [SerializeField] Texture2D lootCursorClicked;
        [SerializeField] Texture2D enterDoorCursor;//maybe also for portals
        [SerializeField] Texture2D enterDoorCursorClicked;
        [SerializeField] AnimatedCursorSO focusingRedCrosshairs;
        [SerializeField] AnimatedCursorSO blinkingGreenCrosshairs;
        [SerializeField] AnimatedCursorSO blinkingYellowCrosshairs;

        Action OnUpdate;
        Dictionary<CursorType, (Texture2D, Texture2D, Vector2)> CursorLookup = new();
        bool animatedCursorEquipped;
        CursorType currentCursorType;

        private void Awake()
        {
            BuildCursorLookup();
        }

        private void Start()
        {
            ICombatController playerCombatController = GameObject.Find("Player").GetComponent<ICombatController>();
            playerCombatController.OnChannelStarted += () => EquipAnimatedCursor(focusingRedCrosshairs);
            playerCombatController.OnChannelEnded += () =>
            {
                if (animatedCursorEquipped)
                {
                    EquipCursor(CursorType.Default, false, true);
                }
            };
            playerCombatController.OnPowerUpStarted += () => EquipAnimatedCursor(blinkingYellowCrosshairs);
            playerCombatController.OnMaximumPowerAchieved += () => EquipAnimatedCursor(blinkingGreenCrosshairs);

            InteractableGameObject.HoveredIGOChanged += EquipIGOHoverCursor;

            EquipCursor(CursorType.Default, false, true);
        }

        private void Update()
        {
            OnUpdate?.Invoke();
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.MouseDown)
            {
                EquipCursor(currentCursorType, true);
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                EquipCursor(currentCursorType);
            }
        }

        private void EquipIGOHoverCursor()
        {
            InteractableGameObject igo = InteractableGameObject.HoveredIGO;
            if (igo)
            {
                EquipCursor(igo.CursorType);
            }
            else
            {
                EquipCursor(CursorType.Default);
            }
        }

        private void EquipCursor(CursorType cursorType, bool clicked = false, 
            bool allowOverrideAnimatedCursor = false)
        {
            if (animatedCursorEquipped && !allowOverrideAnimatedCursor) return;

            if (CursorLookup.TryGetValue(cursorType, out var cursorData) && cursorData.Item1 != null)
            {
                currentCursorType = cursorType;
                var texture = clicked && cursorData.Item2 != null ? cursorData.Item2 : cursorData.Item1;
                EquipStaticCursor(texture, cursorData.Item3);
            }
        }

        private void EquipStaticCursor(Texture2D cursorTexture, Vector2 hotspot = default)
        {
            if (cursorTexture == null) return;

            OnUpdate = null;
            animatedCursorEquipped = false;
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.ForceSoftware);
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
                    Cursor.SetCursor(animatedCursor.CurrentTexture, animatedCursor.hotspot, CursorMode.ForceSoftware);
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
            CursorLookup.Add(CursorType.Default, (defaultCursor, defaultCursorClicked, default));
            CursorLookup.Add(CursorType.Dialogue, (dialogueCursor, dialogueCursorClicked, default));
            CursorLookup.Add(CursorType.Loot, (lootCursor, lootCursorClicked, default));
            CursorLookup.Add(CursorType.EnterDoor, (enterDoorCursor, enterDoorCursorClicked, default));
        }

        private void OnDestroy()
        {
            InteractableGameObject.HoveredIGOChanged -= EquipIGOHoverCursor;
        }
    }
}