using RPGPlatformer.AIControl;
using RPGPlatformer.Core;
using RPGPlatformer.Inventory;

namespace RPGPlatformer.Dialogue
{
    public class ShuttleHawkDialogueActor : DialogueActor
    {
        IAIPatrollerController patrollerController;

        bool hasEncounteredPlayer;

        private void Awake()
        {
            patrollerController = GetComponent<IAIPatrollerController>();
        }

        protected override void BuildDecisionFunctionDict()
        {
            //0 = false, 1 = true
            GetDecisionFunction = new()
            {
                ["IsFirstEncounter"] = args => hasEncounteredPlayer ? 0 : 1
            };
        }

        public override void OnBeginDialogue()
        {
            patrollerController.BeginPatrolRest();
            patrollerController.Patroller.AIMovementController.FaceTarget(GlobalGameTools.Instance.PlayerTransform);
        }

        public override void OnEndDialogue()
        {
            hasEncounteredPlayer = true;

            if (patrollerController.Patrolling)//if not then we are in preparing for departure state
            {
                patrollerController.BeginDefaultPatrol();
            }
        }

        public void TakePayment(string[] args)
        {
            var p = GlobalGameTools.Instance.Player;
            var io = (IInventoryOwner)p.Combatant;
            io.Inventory.RemoveItem(args[0], 1);
        }
    }
}