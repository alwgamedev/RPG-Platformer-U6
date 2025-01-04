using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.Dialogue 
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/New Dialogue")]
    public class Dialogue : ScriptableObject
    {
        [SerializeField] List<DialogueNode> nodes = new();

        DialogueSpeaker playerSpeaker;
        List<DialogueSpeaker> speakerList = new();
        Dictionary<string, DialogueNode> NodeLookup = new();

        //each node will have a unique ID, but we'll set that as the node's name (so node doesn't need a uid field)

        public void InitializeSpeakers(DialogueSpeaker playerSpeaker, List<DialogueSpeaker> speakerList)
        {
            this.playerSpeaker = playerSpeaker;
            this.speakerList = speakerList;
        }

        public void ClearSpeakers()
        {
            playerSpeaker = null;
            speakerList?.Clear();
        }
    }
}
