using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPGPlatformer.Skills;

namespace RPGPlatformer.UI
{
    public class SkillItemUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI header;//<b>SkillName:</b> Level {level}
        [SerializeField] Image xpBar;
        [SerializeField] TextMeshProUGUI xpText;//{xP} / {xpMax} XP

        public void DisplaySkill(ICharacterProgressionManager cpm, ICharacterSkill skill)
        {
            var lvl = cpm.GetLevel(skill);
            header.text = $"<b>{skill.SkillName}:</b> Level {lvl}";
            xpBar.fillAmount = cpm.GetXPFraction(skill);
            xpText.text = $"{cpm.GetXP(skill)} / {skill.XPTable.XPAtLevel(lvl + 1)} XP";
        }
    }
}