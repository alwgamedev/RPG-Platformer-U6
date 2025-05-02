using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public abstract class PoolableObject : MonoBehaviour, IPoolableObject
    {
        public IObjectPool source;

        public abstract void ResetPoolableObject();

        public virtual void ReturnToPool()
        {
            if (source == null || (source is Component c && !c))
            {
                Destroy(gameObject);
                return;
            }
            ResetPoolableObject();
            source.ReturnObject(this);
        }
    }
}