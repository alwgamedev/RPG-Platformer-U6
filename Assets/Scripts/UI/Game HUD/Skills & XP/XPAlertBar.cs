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

        Dictionary<CharacterSkill, XPAlert> activeXPAlerts = new();

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.GetComponent<CharacterProgressionManager>().ExperienceGained +=
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

        private void InstantiateNewAlert(CharacterSkill skill)
        {
            var newAlert = Instantiate(alertPrefab, transform);
            newAlert.QueueEmptied += () => DestroyCompletedAlert(skill, newAlert);
            activeXPAlerts[skill] = newAlert;
        }

        private void DestroyCompletedAlert(CharacterSkill key, XPAlert alert)
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