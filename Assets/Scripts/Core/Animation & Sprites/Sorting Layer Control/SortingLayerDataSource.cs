using System;
using System.Collections.Generic;

namespace RPGPlatformer.Core
{
    public interface SortingLayerDataSource
    {
        public int? SortingLayerID { get; }
        public int? SortingOrder { get; }

        public event Action DataUpdated;
    }
}