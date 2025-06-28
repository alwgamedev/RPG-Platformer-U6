using RPGPlatformer.Saving;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    public class SceneBounds : MonoBehaviour
    {
        bool ready;

        private void Awake()
        {
            SavingSystem.SceneLoadComplete += Ready;
        }

        private void Ready()
        {
            SavingSystem.SceneLoadComplete -= Ready;
            ready = true;
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (!ready || !gameObject.activeInHierarchy) return;

            if (collider.gameObject && collider.gameObject.activeInHierarchy
                && collider.gameObject.TryGetComponent(out IOutOfBoundsHandler o))
            {
                o?.OnOutOfBounds();
            }
            //if (collider.transform == GlobalGameTools.Instance.PlayerTransform 
            //    && !GlobalGameTools.Instance.PlayerIsDead)
            //{
            //    GlobalGameTools.Instance.Player.Combatant.Instakill();
            //}
            //else
            //{
            //    Destroy(collider.gameObject);
            //}

            //all these dumb checks because trigger exit is getting called every time you exit play mode
        }

        private void OnDestroy()
        {
            SavingSystem.SceneLoadComplete -= Ready;
        }
    }
}