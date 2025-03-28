using System;
using System.Collections.Generic;
using RPGPlatformer.Core;
using RPGPlatformer.Dialogue;
using RPGPlatformer.UI;
using UnityEngine.EventSystems;

namespace RPGPlatformer.AIControl
{
    public class InteractableNPC : InteractableGameObject, IInteractableNPC
    {
        protected Action OnUpdate;
        //protected string primaryActionKey;//i.e. left click action (if any)
        //Maybe this can be chosen from a set of pre-prepared static methods based on the IGO's CURSOR TYPE
        //(e.g. if cursor type dialogue, primaryAction = get component dialogueTrigger and trigger dialogue)

        //in the future maybe we could make a serializable class holding the description and a UnityEvent
        //in order to set these in the inspector
        //protected Dictionary<string, Action> InteractionOptions = new();
        //items are (rc menu text, action) exactly like inventory item
        //actions can be e.g. Talk To, View Shop, Pickpocket, etc.

        //^^TO-DO: add RCM for IntNPC

        //private void Start()
        //{
        //    InitializeInteractionOptions();
        //}

        private void Update()
        {
            OnUpdate?.Invoke();
        }

        //public IEnumerable<(string, Action)> GetInteractionOptions()
        //{
        //    //so this is always first in order (will want for RCM)
        //    if (InteractionOptions.TryGetValue(primaryActionKey, out var a) && a != null)
        //    {
        //        yield return (primaryActionKey, a);
        //    }

        //    foreach (var entry in InteractionOptions)
        //    {
        //        if (entry.Key != primaryActionKey && entry.Value != null)
        //        {
        //            yield return (entry.Key, entry.Value);
        //        }
        //    }
        //}

        //public virtual void SetCursorTypeAndPrimaryAction(CursorType cursorType, 
        //    bool removePrimaryActionFromDict = true)
        //{
        //    this.defaultCursorType = cursorType;
        //    SetPrimaryAction(cursorType, removePrimaryActionFromDict);
        //}

        //protected virtual void SetPrimaryAction(CursorType cursorType, bool removePreviousFromDict = true)
        //{
        //    if (cursorType == CursorType.Default)
        //    {
        //        NoPrimaryAction(removePreviousFromDict);
        //    }
        //    if (cursorType == CursorType.Dialogue)
        //    {
        //        if (!TryGetComponent(out IDialogueTrigger dialogueTrigger))
        //        {
        //            NoPrimaryAction(removePreviousFromDict);
        //            return;
        //        }

        //        SetPrimaryAction($"Talk to {displayName}", () => { TriggerDialogue(dialogueTrigger, 0); });

        //        //ATM not using dialogue trigger enable/disable anywhere, so leaving this out
        //        //because it can get confusing when something else changes the cursor type, and 
        //        //you don't know whether you want to override the change or not.

        //        //dialogueTrigger.TriggerEnabled += TriggerEnabledHandler;

        //        ////in the future we may generalize this (e.g. have an interface for
        //        ////"IntNPC Action Source" or something stupid) -- for now dialogue trigger is the only primary action
        //        //void TriggerEnabledHandler(bool val)
        //        //{
        //        //    if (val && this.cursorType == CursorType.Dialogue)
        //        //    {
        //        //        SetCursorTypeAndPrimaryAction(CursorType.Dialogue);
        //        //    }
        //        //    else
        //        //    {
        //        //        SetCursorTypeAndPrimaryAction(CursorType.Default);
        //        //    }
        //        //}
        //    }
        //}

        //protected void SetPrimaryAction(string description, Action action, 
        //    bool removePreviousFromDict = true)
        //{
        //    if (description == null)
        //    {
        //        NoPrimaryAction(removePreviousFromDict);
        //        return;
        //    }
        //    if (primaryActionKey != null && removePreviousFromDict)
        //    {
        //        InteractionOptions.Remove(primaryActionKey);
        //    }

        //    primaryActionKey = description;
        //    InteractionOptions[primaryActionKey] = action;
        //}

        //protected void NoPrimaryAction(bool removePreviousFromDict = true)
        //{
        //    if (primaryActionKey == null) return;

        //    if (removePreviousFromDict)
        //    {
        //        InteractionOptions.Remove(primaryActionKey);
        //    }

        //    primaryActionKey = null;
        //}

        //protected virtual void InitializeInteractionOptions()
        //{
        //    InteractionOptions = new();

        //    SetPrimaryAction(defaultCursorType);
        //}

        //NOTE: trigger is assumed to be a component on the NPC, so they have the same lifetime
        protected virtual void TriggerDialogue(IDialogueTrigger trigger, int index)
        {
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

            trigger.TriggerDialogue(index);
        }

        protected override void OnPlayerOutOfRange()
        {
            GameLog.Log($"You are too far away to interact with {DisplayName}.");
        }

        //public override void OnPointerClick(PointerEventData eventData)
        //{
        //    base.OnPointerClick(eventData);

        //    if (primaryActionKey == null) return;
        //    if (!eventData.IsLeftMouseButtonEvent()) return;
        //    if (GlobalGameTools.PlayerIsDead || !PlayerInRangeWithNotifications()) return;

        //    if (InteractionOptions.TryGetValue(primaryActionKey, out var a))
        //    {
        //        a?.Invoke();
        //    }
        //}
    }
}