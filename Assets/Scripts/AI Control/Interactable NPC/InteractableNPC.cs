using System;
using System.Collections.Generic;
using RPGPlatformer.Core;
using RPGPlatformer.Dialogue;
using RPGPlatformer.UI;
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
                        TriggerDialogue(dialogueTrigger, 0);
                    }
                );
                InteractionOptions.Add(primaryAction);
            }
        }

        //NOTE: trigger is assumed to be a component on the NPC, so they have the same lifetime
        protected virtual void TriggerDialogue(DialogueTrigger trigger, int index)
        {
            trigger.TriggerDialogue(index);
            trigger.DialogueBeganSuccessfully += DialogueBeganHandler;

            void DialogueBeganHandler(bool success)
            {
                if (!success)
                {
                    trigger.DialogueBeganSuccessfully -= DialogueBeganHandler;
                    return;
                }

                OnUpdate = SendNotificationIfPlayerOutOfRange;
                PlayerOutOfRange += trigger.RequestCancelDialogue;
                trigger.DialogueEnded += DialogueEndedHandler;
                trigger.DialogueBeganSuccessfully -= DialogueBeganHandler;
            }

            void DialogueEndedHandler()
            {
                OnUpdate = null;
                PlayerOutOfRange -= trigger.RequestCancelDialogue;
                trigger.DialogueEnded -= DialogueEndedHandler;
            }
        }

        protected override void OnPlayerOutOfRange()
        {
            GameLog.Log($"You are too far away to interact with {DisplayName}.");
            OnUpdate = null;
        }

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