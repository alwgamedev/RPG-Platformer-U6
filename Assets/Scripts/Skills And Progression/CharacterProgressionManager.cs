﻿using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using UnityEngine;
using RPGPlatformer.Saving;
using RPGPlatformer.UI;
using RPGPlatformer.Core;


namespace RPGPlatformer.Skills
{
    public class CharacterProgressionManager : MonoBehaviour, ISavable, ICharacterProgressionManager
    {
        [SerializeField] CharacterProgressionData progressionData = new();
        [SerializeField] bool canGainXP;

        public bool CanGainXP => canGainXP;
        public int TotalLevel => progressionData.TotalLevel();
        public int CombatLevel => progressionData.CombatLevel();
        public int AutoCalculatedHealthPoints => progressionData.AutoCalculatedHealthPoints();

        public event Action<XPGainEventData> ExperienceGained;
        public event Action<ICharacterSkill, int> LevelUp;
        public event Action StateRestored;

        private void Awake()
        {
            Configure();
        }

        protected virtual void Configure()
        {
            progressionData.ForceLevels = !canGainXP;
            progressionData?.Configure();
        }

        public int GetLevel(StandardCharacterSkill skill)
        {
            return GetLevel(CharacterSkillBook.GetCharacterSkill(skill));
        }

        public int GetLevel(ICharacterSkill skill)
        {
            return progressionData.GetLevel(skill);
        }

        public int GetXP(ICharacterSkill skill)
        {
            return progressionData.GetXP(skill);
        }

        public float GetXPFraction(ICharacterSkill skill)
        {
            return progressionData.GetXPFraction(skill);
        }

        public void GainExperience(ICharacterSkill skill, int xpToGain)
        {
            if (!canGainXP) return;

            var data = progressionData.GetProgressionData(skill);
            if (xpToGain <= 0 /*|| data.Level >= skill.XPTable.MaxLevel*/) return;

            int oldLevel = data.Level;
            int xpGained = data.GainExperience(xpToGain, skill.XPTable);

            ExperienceGained?.Invoke(new(skill, data, xpGained));

            if (data.Level > oldLevel)
            {
                LevelUp?.Invoke(skill, data.Level);
                if (transform == GlobalGameTools.Instance.PlayerTransform)
                {
                    GameLog.Log($"Level up! You are now level {data.Level} in {skill.SkillName}.");
                }
            }
        }

        public JsonNode CaptureState()
        {
            return JsonSerializer.SerializeToNode(progressionData);
        }

        public void RestoreState(JsonNode jNode)
        {
            progressionData = jNode.Deserialize<CharacterProgressionData>();
            Configure();
            StateRestored?.Invoke();
        }

        private void OnDestroy()
        {
            ExperienceGained = null;
            LevelUp = null;
            StateRestored = null;
        }
    }
}