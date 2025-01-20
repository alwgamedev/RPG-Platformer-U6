using RPGPlatformer.Core;

namespace RPGPlatformer.Dialogue
{
    public class PlayerConversant : Conversant
    {
        public override string ConversantName => GlobalGameTools.PlayerName;
    }
}