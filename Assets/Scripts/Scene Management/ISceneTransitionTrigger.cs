using System;

namespace RPGPlatformer.SceneManagement
{
    public interface ISceneTransitionTrigger
    {
        public static event Action<SceneTransitionTriggerData> SceneTransitionTriggered;

        public void TriggerScene(string sceneName, object args);
    }
}