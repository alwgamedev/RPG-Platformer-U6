using System;

namespace RPGPlatformer.Movement
{
    public interface IMover
    {
        public event Action<HorizontalOrientation> UpdatedXScale;
    }
}