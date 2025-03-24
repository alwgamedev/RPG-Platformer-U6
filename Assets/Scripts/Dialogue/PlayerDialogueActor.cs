using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using RPGPlatformer.Movement;

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
                ["HasItem"] = args => 0
            };
        }
    }
}