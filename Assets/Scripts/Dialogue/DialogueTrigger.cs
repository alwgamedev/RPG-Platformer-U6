using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Dialogue
{
    public class DialogueTrigger : InteractableGameObject
    {
        [SerializeField] Dialogue dialogue;
        [SerializeField] List<DialogueSpeaker> speakerList;//assumes all speakers will be game objects saved in the scene

        DialogueSpeaker playerSpeaker;

        private void Start()
        {
            playerSpeaker = GameObject.FindWithTag("Player").GetComponent<DialogueSpeaker>();
            if(dialogue)
            {
                dialogue.InitializeSpeakers(playerSpeaker, speakerList);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if(dialogue)
            {
                dialogue.ClearSpeakers();
            }
        }
    }
}