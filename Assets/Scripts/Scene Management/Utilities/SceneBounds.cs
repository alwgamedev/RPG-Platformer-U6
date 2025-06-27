using RPGPlatformer.Core;
using RPGPlatformer.Combat;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    public class SceneBounds : MonoBehaviour
    {
        private void OnTriggerExit2D(Collider2D collider)
        {
            if (!gameObject.activeInHierarchy) return;
            if (collider.transform == GlobalGameTools.Instance.PlayerTransform 
                && !GlobalGameTools.Instance.PlayerIsDead)
            {
                GlobalGameTools.Instance.Player.Combatant.Instakill();
            }

            //all these dumb checks because trigger exit is getting called every time you exit play mode
        }
    }
}