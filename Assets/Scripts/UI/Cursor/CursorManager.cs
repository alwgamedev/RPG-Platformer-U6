using System;
using UnityEngine;
using RPGPlatformer.Combat;

namespace RPGPlatformer.UI
{
    public class CursorManager : MonoBehaviour
    {
        [SerializeField] Texture2D defaultCursor;
        [SerializeField] Texture2D greenCrosshairs;
        [SerializeField] Texture2D yellowCrosshairs;
        [SerializeField] AnimatedCursorSO focusingRedCrosshairs;//could just load these from resources
        [SerializeField] AnimatedCursorSO blinkingGreenCrosshairs;
        [SerializeField] AnimatedCursorSO blinkingYellowCrosshairs;

        Action OnUpdate;

        private void OnEnable()
        {
            ICombatController playerCombatController = GameObject.Find("Player").GetComponent<ICombatController>();
            playerCombatController.OnChannelStarted += () => EquipAnimatedCursor(focusingRedCrosshairs);
            playerCombatController.OnChannelEnded += DefaultCursor;
            playerCombatController.OnPowerUpStarted += () => EquipAnimatedCursor(blinkingYellowCrosshairs);
            playerCombatController.OnPowerUpEnded += DefaultCursor;
            playerCombatController.OnMaximumPowerAchieved += () => EquipAnimatedCursor(blinkingGreenCrosshairs);
        }

        private void Start()
        {
            EquipStaticCursor(defaultCursor);
        }

        private void Update()
        {
            OnUpdate?.Invoke();
        }

        void EquipStaticCursor(Texture2D cursorTexture, Vector2 hotspot)
        {
            if (cursorTexture == null) return;
            OnUpdate = null;
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.ForceSoftware);
        }
        void EquipStaticCursor(Texture2D cursorTexture)
        {
            EquipStaticCursor(cursorTexture, Vector2.zero);
        }

        void EquipAnimatedCursor(AnimatedCursor animatedCursor)
        {
            if (animatedCursor == null || animatedCursor.elements.Length == 0) return;
            animatedCursor.Reset();
            OnUpdate = () =>
            {
                if (animatedCursor.MoveNext())
                {
                    Cursor.SetCursor(animatedCursor.CurrentTexture, animatedCursor.hotspot, CursorMode.ForceSoftware);
                }
            };
        }

        void EquipAnimatedCursor(AnimatedCursorSO animatedCursorSO)
        {
            EquipAnimatedCursor(animatedCursorSO.animation);
        }

        public void DefaultCursor()
        {
            EquipStaticCursor(defaultCursor);
        }
    }
}