using System;
using System.Collections.Generic;
using RPGPlatformer.Core;

namespace RPGPlatformer.AIControl
{
    public class InteractableNPC : InteractableGameObject
    {

        Action primaryAction;//i.e. left click action (if any)
        //Maybe this can be chosen from a set of pre-prepared static methods based on the IGO's CURSOR TYPE
        //(e.g. if cursor type dialogue, primaryAction = get component dialogueTrigger and trigger dialogue)

        public List<(string, Action)> InteractionOptions { get; protected set; } = new();
        //items are (rc menu text, action) exactly like inventory item
        //actions can be e.g. Talk To, View Shop, Pickpocket, etc.

        private void OnEnable()
        {
            InitializeInteractionOptions();
        }

        protected virtual void InitializeInteractionOptions()
        {
            InteractionOptions = new();
            //if(TryGetComponent(out DialogueTrigger dialogueTrigger))
            //{
            //    interactionOptions.Add(dialogueTrigger.TriggerDialogue);
            //}
        }

        protected override void OnMouseDown()
        {
            if (GlobalGameTools.PlayerIsDead || !PlayerInRangeWithNotifications()) return;

            primaryAction?.Invoke();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            primaryAction = null;
        }
    }
}