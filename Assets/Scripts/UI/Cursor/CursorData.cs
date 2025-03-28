using System;
using UnityEngine;

namespace RPGPlatformer.UI
{
    [Serializable]
    public struct CursorData
    {
        [SerializeField] Texture2D texture;
        [SerializeField] Texture2D clickedTexture;
        [SerializeField] Vector2 hotspot;

        public Texture2D Texture => texture;
        public Texture2D ClickedTexture => clickedTexture;
        public Vector2 Hotspot => hotspot;
    }
}