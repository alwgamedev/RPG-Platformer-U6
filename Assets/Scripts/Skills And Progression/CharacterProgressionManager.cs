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
        //[SerializeField] CharacterProgressionDataSO

        [SerializeField] CharacterProgressionData progressionData;
        [SerializeField] bool canGainXP;

        public int TotalLevel => progressionData.TotalLevel();
        public int CombatLevel => progressionData.CombatLevel();

        public event Action<XPGainEventData> ExperienceGained;
        public event Action<CharacterSkill, int> LevelUp;

        public void GainExperience(CharacterSkill skill, int xpToGain)
        {
            if (!canGainXP) return;
            if (!progressionData.TryGetProgressionData(skill, out var data)) return;
            if (xpToGain <= 0 || data.Level >= skill.XPTable.MaxLevel) return;

            int oldLevel = data.Level;
            int xpGained = data.GainExperience(xpToGain, skill.XPTable);

            ExperienceGained?.Invoke(new(skill, data, xpGained));
            //GameLog.Log($"Gained {xpGained} experience points in {skill.SkillName}.");

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