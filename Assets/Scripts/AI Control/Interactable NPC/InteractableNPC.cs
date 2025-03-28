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
        //protected Action OnUpdate;

        //private void Update()
        //{
        //    OnUpdate?.Invoke();
        //}

        //NOTE: trigger is assumed to be a component on the NPC, so they have the same lifetime
        //protected virtual void TriggerDialogue(IDialogueTrigger trigger, int index)
        //{
        //    trigger.DialogueBeganSuccessfully += DialogueBeganHandler;

        //    void DialogueBeganHandler(bool success)
        //    {
        //        if (!success)
        //        {
        //            trigger.DialogueBeganSuccessfully -= DialogueBeganHandler;
        //            return;
        //        }

        //        OnUpdate = SendNotificationIfPlayerOutOfRange;
        //        PlayerOutOfRange += trigger.RequestCancelDialogue;
        //        trigger.DialogueEnded += DialogueEndedHandler;
        //        trigger.DialogueBeganSuccessfully -= DialogueBeganHandler;
        //    }

        //    void DialogueEndedHandler()
        //    {
        //        OnUpdate = null;
        //        PlayerOutOfRange -= trigger.RequestCancelDialogue;
        //        trigger.DialogueEnded -= DialogueEndedHandler;
        //    }

        //    trigger.TriggerDialogue(index);
        //}

        protected override void OnPlayerOutOfRange()
        {
            GameLog.Log($"You are too far away to interact with {DisplayName}.");
        }
    }
}