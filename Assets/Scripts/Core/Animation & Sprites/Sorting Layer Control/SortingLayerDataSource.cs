using System;

namespace RPGPlatformer.Core
{
    public interface SortingLayerDataSource
    {
        public int? SortingLayerID { get; }
        public int? SortingOrder { get; }
    }
}