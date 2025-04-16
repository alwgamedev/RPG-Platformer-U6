using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPGPlatformer.UI
{
    public class GameUICanvas : MonoBehaviour
    {
        Canvas canvas;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();

            SceneManager.sceneLoaded += FindUICamera;
        }

        private void FindUICamera(Scene scene, LoadSceneMode mode)
        {
            var uiCam = GameObject.FindWithTag("UI Camera");
            if (uiCam)
            {
                canvas.worldCamera = uiCam.GetComponent<Camera>();
            }
            else
            {
                canvas.worldCamera = Camera.main;
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= FindUICamera;
        }
    }
}