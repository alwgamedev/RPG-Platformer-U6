using RPGPlatformer.SceneManagement;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public class MonoBehaviorInputConfigurer : MonoBehaviour
    {
        private void Start()
        {
            var dpts = GetComponents<IInputDependent>();

            foreach (var d in dpts)
            {
                if (d.InputSource == null)
                {
                    d.InitializeInputSource();
                }
                if (d.InputSource != null)
                {
                    d.InputSource.InputEnabled += d.OnInputEnabled;
                    d.InputSource.InputDisabled += d.OnInputDisabled;
                }
            }
        }
    }
}