using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using RPGPlatformer.Inventory;
using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    public class PlayerDialogueActor : DialogueActor
    {
        ICombatController combatController;
        IMovementController movementController;

        public override string ActorName => GlobalGameTools.PlayerName;

        private void Awake()
        {
            combatController = GetComponent<ICombatController>();
            movementController = GetComponent<IMovementController>();
        }

        protected override void BuildDecisionFunctionDict()
        {
            GetDecisionFunction = new()
            {
                ["HasItem"] = HasItem
            };
        }

        private int HasItem(string[] args)
        {
            return ((IInventoryOwner)combatController.Combatant).HasItem(args[0]) ? 1 : 0;
        }
    }
}