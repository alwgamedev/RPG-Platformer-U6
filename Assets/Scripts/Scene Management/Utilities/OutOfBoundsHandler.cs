using UnityEngine;
using UnityEngine.Events;

namespace RPGPlatformer.SceneManagement
{
    public class OutOfBoundsHandler : MonoBehaviour, IOutOfBoundsHandler
    {
        [SerializeField] UnityEvent onOutOfBounds;

        public void OnOutOfBounds()
        {
            onOutOfBounds.Invoke();
        }
    }
}