using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using UnityEngine;
using RPGPlatformer.Saving;
using RPGPlatformer.UI;


namespace RPGPlatformer.Skills
{
    public class CharacterProgressionManager : MonoBehaviour, ISavable, IXPGainer
    {
        [SerializeField] CharacterProgressionData progressionData = new();
        [SerializeField] bool canGainXP;

        public int TotalLevel => progressionData.TotalLevel();
        public int CombatLevel => progressionData.CombatLevel();

        public event Action<XPGainEventData> ExperienceGained;
        public event Action<CharacterSkill, int> LevelUp;

        private void Awake()
        {
            progressionData.Configure();
        }

        public int GetLevel(StandardCharacterSkill skill)
        {
            return GetLevel(CharacterSkillBook.GetCharacterSkill(skill));
        }

        public int GetLevel(CharacterSkill skill)
        {
            return progressionData.GetLevel(skill);
        }

        //public bool TryGetLevel(CharacterSkill skill, out int level)
        //{
        //    if (progressionData.TryGetProgressionData(skill, out var data))
        //    {
        //        level = data.Level;
        //        return true;
        //    }
        //    level = 0;
        //    return false;
        //}

        public int AutoCalculatedHealthPoints()
        {
            return 4000 + (312 * (progressionData.GetLevel(CharacterSkillBook.Fitness) - 1));
        }

        public void GainExperience(CharacterSkill skill, int xpToGain)
        {
            if (!canGainXP) return;
            //if (!progressionData.TryGetProgressionData(skill, out var data)) return;

            var data = progressionData.GetProgressionData(skill);
            if (xpToGain <= 0 || data.Level >= skill.XPTable.MaxLevel) return;

            int oldLevel = data.Level;
            int xpGained = data.GainExperience(xpToGain, skill.XPTable);

            ExperienceGained?.Invoke(new(skill, data, xpGained));

            if(data.Level > oldLevel)
            {
                LevelUp?.Invoke(skill, data.Level);
                GameLog.Log($"Level up! You are now level {data.Level} in {skill.SkillName}.");
            }
        }

        public JsonNode CaptureState()
        {
            return JsonSerializer.SerializeToNode(progressionData);
        }

        public void RestoreState(JsonNode jNode)
        {
            progressionData = jNode.Deserialize<CharacterProgressionData>();
            progressionData.Configure();
        }

        private void OnDestroy()
        {
            ExperienceGained = null;
            LevelUp = null;
        }
    }
}