using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    [Serializable]
    public struct SpriteSwapData
    {
        [SerializeField] string category;
        [SerializeField] string label;

        public string Category => category;
        public string Label => label;
    }
}