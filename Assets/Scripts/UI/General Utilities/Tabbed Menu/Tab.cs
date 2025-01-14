using System;
using UnityEngine;
using UnityEngine.UI;

namespace RPGPlatformer.UI
{
    //not a MB because the Button and Content will likely be attached to different game objects
    //(a common parent would be the tab menu, but we can't attach multiple Tab components to one object)
    [Serializable]
    public class Tab
    {
        [SerializeField] string name;
        [SerializeField] Button button;
        [SerializeField] GameObject buttonObscurer;
        [SerializeField] GameObject content;

        public string Name => name;
        public Button Button => button;
        public GameObject Content => content;

        public void SetOpen(bool val)
        {
            if (content != null)
            {
                content.SetActive(val);
            }
            buttonObscurer.SetActive(!val);
        }

        public void ObscureButton(bool val)
        {
            buttonObscurer.SetActive(val);
        }
    }
}