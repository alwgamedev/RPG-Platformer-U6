using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.Combat;

using System;

namespace RPGPlatformer.Effects
{
    public abstract class PoolableEffect : PoolableObject
    {
        [SerializeField] protected bool requeAutomaticallyAtEndOfEffect = true;
        [SerializeField] protected bool requeOnParentDeath = false;

        protected IHealth parentHealth;

        public bool RequeAutomaticallyAtEndOfEffect => requeAutomaticallyAtEndOfEffect;
        public abstract float AutomaticRequeDelay { get; }

        protected abstract void PlayEffect();

        protected abstract void StopEffect();

        public virtual void Play()
        {
            PlayEffect();

            if (requeAutomaticallyAtEndOfEffect)
            {
                Invoke(nameof(Stop), AutomaticRequeDelay);
            }
        }

        public virtual void Stop()
        {
            if (!gameObject.activeSelf) return;
            StopEffect();
            ReturnToPool();
        }

        public void PlayAtPosition(Vector3 position)
        {
            transform.position = position;
            Play();
        }

        public void PlayAtPosition(Transform target)
        {
            transform.SetParent(target);//important e.g. for Invoke power up effect, which needs to move with the player
            transform.localPosition = Vector3.zero;

            if (target.gameObject.TryGetComponent(out IHealth health))
            {
                if (health.IsDead)
                {
                    ReleaseFromParent();
                }
                else
                {
                    ConnectParentHealth(health);
                }
            }

            Play();
        }

        public override void ResetPoolableObject()
        {
            DisconnectParentHealth();
        }

        protected void ConnectParentHealth(IHealth health)
        {
            DisconnectParentHealth();
            parentHealth = health;
            parentHealth.OnDeath += ParentDeathHandler;
        }

        protected void DisconnectParentHealth()
        {
            if(parentHealth != null)
            {
                parentHealth.OnDeath -= ParentDeathHandler;
                parentHealth = null;
            }
        }

        protected void ReleaseFromParent()
        {
            if (requeOnParentDeath)
            {
                Stop();
            }
            else
            {
                transform.SetParent(null, true);
            }
        }

        protected void ParentDeathHandler(IDamageDealer damageDealer)
        {
            ReleaseFromParent();
            DisconnectParentHealth();
        }

        private void OnDestroy()
        {
            DisconnectParentHealth();
        }
    }
}