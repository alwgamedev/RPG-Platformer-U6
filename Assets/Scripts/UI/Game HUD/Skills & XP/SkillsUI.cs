using RPGPlatformer.Core;
using RPGPlatformer.Saving;
using RPGPlatformer.Skills;
using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.UI
{
    public class SkillsUI : MonoBehaviour
    {
        [SerializeField] RectTransform skillsContainer;
        [SerializeField] SkillItemUI skillItemPrefab;

        Dictionary<CharacterSkill, SkillItemUI> GetSkillUI = new();

        private void Awake()
        {
            CreateUI();
            SavingSystem.SceneLoadComplete += UpdateUI;
        }

        private void Start()
        {
            UpdateUI();
            GlobalGameTools.Instance.PlayerProgressionManager.ExperienceGained += HandleExperienceGain;
        }

        //will need to update ui on SavingSystem.SceneLoadComplete and on progression manager's xp gain event

        private void CreateUI()
        {
            foreach (var s in CharacterSkillBook.AllSkills())
            {
                var u = Instantiate(skillItemPrefab, skillsContainer);
                GetSkillUI[s] = u;
            }
        }

        private void UpdateUI()
        {
            var cpm = GlobalGameTools.Instance.PlayerProgressionManager;
            foreach (var e in GetSkillUI)
            {
                e.Value.DisplaySkill(cpm, e.Key);
            }
        }

        private void HandleExperienceGain(XPGainEventData d)
        {
            GetSkillUI[d.skill].DisplaySkill(GlobalGameTools.Instance.PlayerProgressionManager, d.skill);
        }

        private void OnDestroy()
        {
            SavingSystem.SceneLoadComplete -= UpdateUI;
            var cpm = GlobalGameTools.Instance.PlayerProgressionManager;
            if (cpm)
            {
                cpm.ExperienceGained -= HandleExperienceGain;
            }
        }
    }
}