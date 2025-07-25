﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RPGPlatformer.Dialogue;
using RPGPlatformer.Core;

namespace RPGPlatformer.UI
{
    public class DialogueWindow : MonoBehaviour
    {
        [SerializeField] float dialogueFadeDuration = 0.5f;
        [SerializeField] float delayBetweenDialogueSegments = 4;
        [SerializeField] TextMeshProUGUI speakerLabel;
        [SerializeField] VerticalLayoutGroup dialogueContainer;
        [SerializeField] VerticalLayoutGroup choicesContainer;
        [SerializeField] CanvasGroup dialogueSegmentPrefab;
        [SerializeField] GameObject nextButtonContainer;
        [SerializeField] Button choiceButtonPrefab;
        [SerializeField] Button nextButton;
        [SerializeField] Button closeButton;

        List<CanvasGroup> textSegments = new();

        public GameObject NextButtonContainer => nextButtonContainer;
        public Button CloseButton => closeButton;

        public event Action<int> RequestContinuation;

        public void SetUpWindow(DialogueNode dialogueNode, string conversantName/*, string playerName*/)
        {
            Clear();//just in case there is something there from the prefab;

            foreach (var text in dialogueNode.TextSegments())
            {
                if (string.IsNullOrEmpty(text)) continue;
                var textSegment = Instantiate(dialogueSegmentPrefab, dialogueContainer.transform);
                textSegment.GetComponent<TextMeshProUGUI>().text = text;
                textSegment.alpha = 0;
                textSegments.Add(textSegment);
            }

            if (dialogueNode is ResponseChoicesDialogueNode choicesNode)
            {
                List<ResponseChoiceData> responseChoices = choicesNode.ResponseChoices();

                for (int i = 0; i < responseChoices.Count; i++)
                {
                    var choiceButton = Instantiate(choiceButtonPrefab, choicesContainer.transform);
                    choiceButton.GetComponentInChildren<TextMeshProUGUI>().text = responseChoices[i].choiceText;
                    int index = i;
                    choiceButton.onClick.AddListener(() => RequestContinuation?.Invoke(index));
                }

                if (textSegments != null && textSegments.Count > 0)
                {
                    DisplayMainDialogue(conversantName);
                    nextButton.onClick.AddListener(() => DisplayResponseChoices());
                }
                else
                {
                    DisplayResponseChoices(/*playerName*/);
                }
            }
            else
            {
                DisplayMainDialogue(conversantName);
                nextButton.onClick.AddListener(() => RequestContinuation?.Invoke(-1));
            }

        }

        public void DisplayMainDialogue(string speakerName)
        {
            speakerLabel.text = speakerName;

            dialogueContainer.gameObject.SetActive(true);
            choicesContainer.gameObject.SetActive(false);

            StartCoroutine(SequentiallyFadeInDialogueSegments());
        }

        public void DisplayResponseChoices(/*string playerName*/)
        {
            StopAllCoroutines();

            speakerLabel.text = GlobalGameTools.PlayerName;

            dialogueContainer.gameObject.SetActive(false);
            choicesContainer.gameObject.SetActive(true);
            nextButtonContainer.SetActive(false);
        }

        private IEnumerator SequentiallyFadeInDialogueSegments()
        {
            WaitForSeconds delayBetweenSegments = new WaitForSeconds(delayBetweenDialogueSegments);

            foreach (var textGroup in textSegments)
            {
                yield return textGroup.FadeIn(dialogueFadeDuration);
                yield return delayBetweenSegments;
            }
        }

        private void Clear()
        {
            speakerLabel.text = "";
            ClearDialogue();
            ClearResponseChoices();
        }

        private void ClearDialogue()
        {
            foreach (TextMeshProUGUI text in dialogueContainer.GetComponentsInChildren<TextMeshProUGUI>())
            {
                Destroy(text.gameObject);
            }
        }

        private void ClearResponseChoices()
        {
            foreach (Button response in choicesContainer.GetComponentsInChildren<Button>())
            {
                Destroy(response.gameObject);
            }
        }

        private void OnDestroy()
        {
            foreach (Button b in GetComponentsInChildren<Button>(true))
            {
                b.onClick.RemoveAllListeners();
            }

            RequestContinuation = null;
        }
    }
}