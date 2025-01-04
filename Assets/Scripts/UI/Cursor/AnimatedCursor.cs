using System;
using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.UI
{
    [Serializable]
    public class AnimatedCursor : Cycle<Texture2D>
    {
        public float secondsPerFrame = 0.2f;
        public Vector2 hotspot;
        private float timer;

        public Texture2D CurrentTexture => Current as Texture2D;

        public override bool MoveNext()
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer += secondsPerFrame;
                return base.MoveNext();
            }
            return false;
        }

        public override void Reset()
        {
            base.Reset();
            timer = 0;
        }
    }
}