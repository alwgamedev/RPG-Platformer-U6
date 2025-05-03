using System;

namespace RPGPlatformer.SceneManagement
{
    public struct SceneTransitionData
    {
        public readonly string prevScene;
        public readonly SceneTransitionTriggerData nextSceneData;

        public SceneTransitionData(string prevScene, SceneTransitionTriggerData nextSceneData)
        {
            this.prevScene = prevScene;
            this.nextSceneData = nextSceneData;
        }
    }
}