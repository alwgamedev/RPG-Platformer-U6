using System;

namespace RPGPlatformer.SceneManagement
{
    public struct SceneTransitionData
    {
        public readonly string sceneUnloaded;
        public readonly string sceneLoaded;
        public readonly SceneStarterDataLite sceneStarterData;

        public SceneTransitionData(string sceneUnloaded, string sceneLoaded, SceneStarterDataLite sceneStarterData)
        {
            this.sceneUnloaded = sceneUnloaded;
            this.sceneLoaded = sceneLoaded;
            this.sceneStarterData = sceneStarterData;
        }
    }
}