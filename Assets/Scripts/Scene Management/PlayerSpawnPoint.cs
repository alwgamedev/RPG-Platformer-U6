using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    public class PlayerSpawnPoint : MonoBehaviour
    {
        [SerializeField] string id;//we could also just use the game object name

        public string ID => id;
    }
}