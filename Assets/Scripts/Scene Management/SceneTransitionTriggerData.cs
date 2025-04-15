using System;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    [Serializable]
    public struct SceneTransitionTriggerData
    {
        [SerializeField] string sceneToLoad;
        [SerializeField] SceneStarterDataLite sceneStarterData;//use -1 if you want to use the default spawn point

        //in the future if you need more data than just player spawn point you can make a struct
        //for "scene start data"

        public string SceneToLoad => sceneToLoad;
        public SceneStarterDataLite SceneStarterData => sceneStarterData;
    }
}