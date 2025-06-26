using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    public class SceneBounds : MonoBehaviour
    {
        private void OnTriggerExit2D(Collider2D collider)
        {
            if (!gameObject.activeInHierarchy) return;
            var p = GlobalGameTools.Instance.PlayerTransform;
            if (p && collider.transform == p
                && !GlobalGameTools.Instance.PlayerIsDead)
            {
                GlobalGameTools.Instance.Player.Combatant.Instakill();
            }
        }
    }
}