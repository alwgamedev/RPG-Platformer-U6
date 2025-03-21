using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    public class PlayerConversant : Conversant
    {
        public override string ConversantName => GlobalGameTools.PlayerName;

        public void TestAction(string[] p)
        {
            Debug.Log("testing");
        }
    }
}