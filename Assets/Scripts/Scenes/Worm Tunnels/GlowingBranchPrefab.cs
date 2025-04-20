using RPGPlatformer.Core;
using RPGPlatformer.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPGPlatformer.Combat
{
    public class GlowingBranchPrefab : MonoBehaviour
    {
        private void Start()
        {
            if (SceneManager.GetActiveScene().name != "WormTunnels")
            {
                GameLog.Log("The glowing branch evaporates to dust in the sunlight.");
                IEquippableCharacter p = GlobalGameTools.Instance.Player.Combatant;
                p.UnequipItem(EquipmentSlot.Mainhand, false);

                //if we have time we can add an actual evaporating effect later
            }
        }
    }
}