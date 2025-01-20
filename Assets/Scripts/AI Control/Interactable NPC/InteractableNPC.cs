using System;
using System.Collections.Generic;
using RPGPlatformer.Core;
using RPGPlatformer.Dialogue;
using RPGPlatformer.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPGPlatformer.AIControl
{
    public class InteractableNPC : InteractableGameObject
    {
        protected Action OnUpdate;
        protected (string, Action) primaryAction = new();//i.e. left click action (if any)
        //Maybe this can be chosen from a set of pre-prepared static methods based on the IGO's CURSOR TYPE
        //(e.g. if cursor type dialogue, primaryAction = get component dialogueTrigger and trigger dialogue)

        public List<(string, Action)> InteractionOptions { get; protected set; } = new();
        //items are (rc menu text, action) exactly like inventory item
        //actions can be e.g. Talk To, View Shop, Pickpocket, etc.

        private void Start()
        {
            InitializeInteractionOptions();
        }

        private void Update()
        {
            OnUpdate?.Invoke();
        }

        protected virtual void InitializeInteractionOptions()
        {
            InteractionOptions = new();

            if (cursorType == CursorType.Dialogue && TryGetComponent(out DialogueTrigger dialogueTrigger))
            {
                primaryAction = ($"Talk to {displayName}", () =>
                    {
                        dialogueTrigger.TriggerDialogue(0);
                        OnUpdate = () => PlayerInRangeWithNotifications();
                    }
                );
                PlayerOutOfRange += dialogueTrigger.CancelDialogue;
                InteractionOptions.Add(primaryAction);
            }
        }

        protected override void OnPlayerOutOfRange()
        {
            GameLog.Log($"You are too far away to interact with {DisplayName}.");
            OnUpdate = null;
        }

        //public override void OnMouseDown()
        //{
        //    if (!Input.GetKeyDown(KeyCode.Mouse0)) return;
        //    if (GlobalGameTools.PlayerIsDead || !PlayerInRangeWithNotifications()) return;

        //    primaryAction.Item2?.Invoke();
        //}

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            if (!eventData.IsLeftMouseButtonEvent()) return;
            if (GlobalGameTools.PlayerIsDead || !PlayerInRangeWithNotifications()) return;

            primaryAction.Item2?.Invoke();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}