using System;

namespace RPGPlatformer.Dialogue
{
    //Why this interface?
    //because DialogueTrigger depends on DialogueTriggerData depends on DialogueActor
    //and DialogueActor may want to call methods from controller components like AIController
    //or MovementController (although probably will only know their interfaces),
    //but those controllers may want to in turn call methods from DialogueTrigger,
    //e.g. disable dialogue trigger, so just using interface to keep things clean
    public interface IDialogueTrigger
    {
        public event Action DialogueCancelRequested;
        public event Action<bool> DialogueBeganSuccessfully;
        public event Action DialogueEnded;
        public event Action<bool> TriggerEnabled;

        public void EnableTrigger(bool val);

        public void RequestCancelDialogue();

        public void TriggerDialogue(string dialogueName);

        public void TriggerDialogue(int index);

        public void TriggerDialogue(DialogueTriggerData data);
    }
}