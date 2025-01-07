using RPGPlatformer.Dialogue;
using RPGPlatformer.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGPlatformer.UI
{
    public class DialogueUI : HideableUI
    {
        [SerializeField] float dialogueFadeDuration = 0.5f;
        [SerializeField] VerticalLayoutGroup dialogueContainer;
        [SerializeField] VerticalLayoutGroup choicesContainer;
        [SerializeField] CanvasGroup dialogueSegmentPrefab;//use GetComponentInChildren<TMP> to set its text
        //^idea: all dialogue segments will be installed at beginning of a dialogue node (so ui has correct size),
        //but they will fade in 1by1
        [SerializeField] Button choiceButtonPrefab;
        [SerializeField] Button nextButton;
        [SerializeField] Button exitButton;

        RectTransform rectTransform;
        DialogueSO activeDialogue;
        string conversantName;

        protected override void Awake()
        {
            base.Awake();

            rectTransform = GetComponent<RectTransform>();
            DialogueTrigger.DialogueTriggered += StartDialogue;
        }

        public void StartDialogue(DialogueSO dialogue, string conversantName)
        {
            activeDialogue = dialogue;
            this.conversantName = conversantName;
        }

        private IEnumerator DisplayDialogueNode(DialogueNode dialogueNode)
        {
            yield return null;
        }

        private void DisplayResponseChoices(ChoicesDialogueNode choicesNode)
        {

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach(Button b in GetComponentsInChildren<Button>())
            {
                b.onClick.RemoveAllListeners();
            }
        }
    }
}
