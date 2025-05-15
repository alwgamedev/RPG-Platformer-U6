using UnityEngine;

namespace RPGPlatformer.Core
{
    public class PoolableObject : MonoBehaviour, IPoolableObject
    {
        public IObjectPool source;

        public virtual void OnEnqueued(IObjectPool source)
        {
            this.source = source;
        }

        public virtual void BeforeSetActive() { }

        public virtual void ResetPoolableObject() { }

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