using UnityEngine;

namespace RPGPlatformer.Environment
{
    public interface IEvilRootManager
    {
        public Transform RootSortingLayerDataSource { get; }
        public Collider2D Platform { get; }
    }
}