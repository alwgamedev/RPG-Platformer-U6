using UnityEngine;

namespace RPGPlatformer.Core
{
    public abstract class PoolableObject : MonoBehaviour, IPoolableObject
    {
        public IObjectPool source;

        public abstract void Configure(object parameters);

        public virtual void OnEnqueued(IObjectPool source)
        {
            this.source = source;
        }

        //you could put this in OnEnable instead,
        //but then we can't guarantee that other components have completed Awake
        //so that is the one small reason to do this
        public abstract void BeforeSetActive();

        //because we can't rely on OnEnable
        public abstract void AfterSetActive();

        //e.g. pill bug needs to override this
        public virtual void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public abstract void ResetPoolableObject();

        public virtual void ReturnToPool()
        {
            if (source == null || (source is Component c && !c))
            {
                if (gameObject)
                {
                    Destroy(gameObject);
                }
                return;
            }
            ResetPoolableObject();
            source.ReturnObject(this);
        }
    }
}