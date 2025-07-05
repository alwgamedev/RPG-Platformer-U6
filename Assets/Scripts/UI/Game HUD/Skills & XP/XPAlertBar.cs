using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using RPGPlatformer.Skills;
using RPGPlatformer.Core;

namespace RPGPlatformer.UI
{
    public class XPAlertBar : MonoBehaviour
    {
        [SerializeField] XPAlert alertPrefab;

        Dictionary<ICharacterSkill, XPAlert> activeXPAlerts = new();

        private void Start()
        {
            GlobalGameTools.Instance.PlayerProgressionManager.ExperienceGained +=
                async (eventData) => await HandleXPGainEvent(eventData);
        }

        private async Task HandleXPGainEvent(XPGainEventData eventData)
        {
            if (eventData.skill == null) return;

            if (!activeXPAlerts.ContainsKey(eventData.skill) || !activeXPAlerts[eventData.skill])
            {
                InstantiateNewAlert(eventData.skill);
            }

            await activeXPAlerts[eventData.skill]
                .HandleXPGainEvent(eventData, GlobalGameTools.Instance.TokenSource.Token);
        }

        private void InstantiateNewAlert(ICharacterSkill skill)
        {
            var newAlert = Instantiate(alertPrefab, transform);
            newAlert.QueueEmptied += () => DestroyCompletedAlert(skill, newAlert);
            activeXPAlerts[skill] = newAlert;
        }

        private void DestroyCompletedAlert(ICharacterSkill key, XPAlert alert)
        {
            if (key == null)

            activeXPAlerts[key] = null;
            if (alert)
            {
                Destroy(alert.gameObject);
            }
        }
    }
}