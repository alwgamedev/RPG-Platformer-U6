using RPGPlatformer.Core;

namespace RPGPlatformer.Dialogue
{
    public class PlayerDialogueActor : DialogueActor
    {
        public override string ActorName => GlobalGameTools.PlayerName;
    }
}