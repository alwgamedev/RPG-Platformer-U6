using RPGPlatformer.Core;
using RPGPlatformer.Inventory;
using RPGPlatformer.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPGPlatformer.Combat
{
    public class WormTunnelsOnlyItem : MonoBehaviour
    {
        [SerializeField] EquippableItemSO item;

        private void Start()
        {
            if (SceneManager.GetActiveScene().name != "WormTunnels")
            {
                GameLog.Log($"Your {item.BaseData.DisplayName} evaporates to dust in the sunlight.");
                IEquippableCharacter p = GlobalGameTools.Instance.Player.Combatant;
                p.UnequipItem(EquipmentSlot.Mainhand, false);

                //if we have time we can add an actual evaporating effect later
            }
        }
    }
}