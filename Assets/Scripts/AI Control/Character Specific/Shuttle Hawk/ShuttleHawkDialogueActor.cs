using RPGPlatformer.AIControl;
using RPGPlatformer.Core;

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
            GetDecisionFunction = new()
            {
                ["IsFirstEncounter"] = args => hasEncounteredPlayer ? 1 : 0
            };
        }

        public override void OnBeginDialogue()
        {
            patrollerController.BeginPatrolRest();
            patrollerController.MovementController.FaceTarget(GlobalGameTools.Player.Combatant.Transform);
        }

        public override void OnEndDialogue()
        {
            hasEncounteredPlayer = true;

            if (patrollerController.Patrolling)//if not then we are in preparing for departure state
            {
                patrollerController.BeginDefaultPatrol();
            }
        }
    }
}