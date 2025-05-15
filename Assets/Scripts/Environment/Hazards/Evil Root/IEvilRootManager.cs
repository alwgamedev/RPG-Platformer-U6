using UnityEngine;

namespace RPGPlatformer.Environment
{
    public interface IEvilRootManager
    {
        //public Transform transform { get; }
        public Collider2D Platform { get; }
    }
}