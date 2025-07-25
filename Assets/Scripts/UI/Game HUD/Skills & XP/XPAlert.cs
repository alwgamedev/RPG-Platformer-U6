﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPGPlatformer.Skills;
using RPGPlatformer.Core;

namespace RPGPlatformer.UI
{
    public class XPAlert : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI skillNameText;
        [SerializeField] TextMeshProUGUI levelText;
        [SerializeField] TextMeshProUGUI xpGainedText;
        [SerializeField] Image progressBar;
        [SerializeField] Image skillIcon;
        [SerializeField] float fillAmountPerSecond = 0.25f;
        [SerializeField] float timeToWaitBeforeDestroy = 3;

        Queue<XPGainEventData> alertQueue = new();
        Animation xpGainedTextAnimation;

        Action Destroyed;

        public event Action NewXPGainEventReceived;
        public event Action QueueEmptied;

        private void Awake()
        {
            xpGainedTextAnimation = xpGainedText.GetComponent<Animation>();
        }

        public async Task HandleXPGainEvent(XPGainEventData eventData, CancellationToken token)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            try
            {
                Destroyed += cts.Cancel;
                NewXPGainEventReceived?.Invoke();
                PlayXPGainedTextAnimation(eventData);
                await ScheduleAlert(eventData, cts.Token);
            }
            catch (TaskCanceledException)
            {
                return;
            }
            finally
            {
                Destroyed -= cts.Cancel;
            }
        }

        private async Task ScheduleAlert(XPGainEventData eventData, CancellationToken token)
        {
            if(!gameObject.activeSelf)
            {
                return;
            }
            else if(alertQueue.Count == 0)
            {
                await PlayAlert(eventData, token);
            }
            else
            {
                alertQueue.Enqueue(eventData);
            }
        }

        private async Task PlayAlert(XPGainEventData eventData, CancellationToken token)
        {
            var skill = eventData.skill;
            var data = eventData.progressionData;
            int xpGain = eventData.xpGained;

            int startingXP = data.XP - xpGain;
            int endingXP = data.XP;
            //int endingLevel = data.Level;
            int startingLevel = skill.XPTable.LevelAtXP(data.XP - xpGain);
            //^check directly because we could have gained multiple levels

            float animXP = startingXP;
            int animLevel = startingLevel;
            int nextAnimLevelXP = skill.XPTable.XPAtLevel(animLevel + 1);
            float animXPPerSecond = skill.XPTable.LevelXPDelta(animLevel) * fillAmountPerSecond;

            DisplaySkillAndLevel(eventData.skill, animLevel);
            SetProgressBarFillAmount(skill.XPTable, animLevel, animXP);

            while (animXP < endingXP)
            {
                await Task.Yield();

                if(token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                animXP += animXPPerSecond * Time.deltaTime;
                if (progressBar)
                {
                    progressBar.fillAmount += fillAmountPerSecond * Time.deltaTime;
                }

                if(animXP >= nextAnimLevelXP)
                {
                    OnLevelUpHit();

                    animLevel = skill.XPTable.LevelAtXP((int)animXP);
                    nextAnimLevelXP = skill.XPTable.XPAtLevel(animLevel + 1);
                    animXPPerSecond = skill.XPTable.LevelXPDelta(animLevel) * fillAmountPerSecond;

                    DisplaySkillAndLevel(eventData.skill, animLevel);
                    SetProgressBarFillAmount(skill.XPTable, animLevel, animXP);
                }
            }

            await OnAlertComplete(token);
        }

        private void PlayXPGainedTextAnimation(XPGainEventData eventData)
        {
            xpGainedTextAnimation.Stop();
            xpGainedText.text = $"+{eventData.xpGained} XP";
            xpGainedTextAnimation.Play();
        }

        private void DisplaySkillAndLevel(ICharacterSkill skill, int level)
        {
            skillNameText.text = skill.SkillName;
            levelText.text = $"Level {level}";
            if(GlobalGameTools.Instance.ResourcesManager.CircularSkillIcons.TryGetValue(skill, out var sprite))
            {
                skillIcon.sprite = sprite;
            }
        }

        private void SetProgressBarFillAmount(IXPTable xpTable, int currentLevel, float currentXP)
        {
            progressBar.fillAmount = (currentXP - xpTable.XPAtLevel(currentLevel)) / xpTable.LevelXPDelta(currentLevel);
        }

        private void OnLevelUpHit()
        {
            Debug.Log("Fireworks! (XP Alert animation hit a level up.)");
        }

        private async Task OnAlertComplete(CancellationToken token)
        {
            if(alertQueue.Count == 0)
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);

                try
                {
                    NewXPGainEventReceived += cts.Cancel;
                    await MiscTools.DelayGameTime(timeToWaitBeforeDestroy, cts.Token);
                    Destroyed -= cts.Cancel;
                    NewXPGainEventReceived -= cts.Cancel;
                    QueueEmptied?.Invoke();
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                finally
                {
                    NewXPGainEventReceived -= cts.Cancel;
                }
            }
            else
            {
                await PlayAlert(alertQueue.Dequeue(), token);
            }
        }

        private void OnDestroy()
        {
            Destroyed?.Invoke();

            QueueEmptied = null;
            NewXPGainEventReceived = null;
            Destroyed = null;
        }
    }
}