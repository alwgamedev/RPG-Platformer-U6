using RPGPlatformer.AIControl;
using RPGPlatformer.Dialogue;
using System;
using System.Collections.Generic;

namespace RPGPlatformer.Core
{
    public class ShuttleHawkNPC : InteractableNPC
    {
        IDialogueTrigger dialogueTrigger;
        bool canTriggerDialogue;

        protected override void Awake()
        {
            base.Awake();

            dialogueTrigger = GetComponent<IDialogueTrigger>();
        }

        private void Start()
        {
            var controller = GetComponent<ShuttleHawkController>();
            controller.DialogueEnabled += EnableDialogue;
        }

        public void EnableDialogue(bool val)
        {
            canTriggerDialogue = val;
            CursorType = val ? UI.CursorType.Dialogue : UI.CursorType.Default;
        }

        public override IEnumerable<(string, Func<bool>, Action)> InteractionOptions()
        {
            if (canTriggerDialogue)
            {
                yield return ($"Talk to {DisplayName}", PlayerCanInteract, TriggerDialogue);
            }

            foreach (var option in base.InteractionOptions())
            {
                yield return option;
            }

            //may add option "Pay {DisplayName}" that triggers a shorter dialogue 
            //that goes straight to the decision node "HasItem(worm)"
        }

        private void TriggerDialogue()
        {
            TriggerDialogue(dialogueTrigger, 0);
        }

        protected override void OnLeftClick()
        {
            if (canTriggerDialogue)
            {
                TriggerDialogue();
            }
        }
    }
}