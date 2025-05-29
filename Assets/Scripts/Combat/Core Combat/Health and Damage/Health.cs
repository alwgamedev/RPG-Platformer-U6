using System;
using UnityEngine;
using RPGPlatformer.UI;
using RPGPlatformer.Movement;

namespace RPGPlatformer.Combat
{
    public class Health : MonoBehaviour, IHealth, IHealthPointer
    {
        [SerializeField] bool takeDamageAutomatically;//these 3 bools are mainly for the combat dummy.
        [SerializeField] bool takeDefaultValueOnStart;//characters with a combat controller will set up 
        [SerializeField] bool findStatBarInChildren;//their Health themselves
        [SerializeField] ReplenishableStat stat;
        [SerializeField] Transform hitEffectTransformOverride;

        public bool IsDead { get; private set; }
        public IDamageDealer Killer { get; private set; }
        //public Transform Transform => transform; 
        public float TargetingTolerance { get; private set; }
        public ReplenishableStat Stat => stat;
        public Transform HitEffectTransform { get; private set; }

        public event Action<float, IDamageDealer> HealthChangeTrigger;
        public event Action<float, bool> OnStunned;//signature is (duration, freezeAnimation)
        public event Action<IDamageDealer> OnDeath;

        private void Awake()
        {
            if (takeDamageAutomatically)//mainly for combat dummy
            {
                HealthChangeTrigger += (damage, damageDealer) =>
                {
                    GainHealth(-damage, true);
                    if (stat.CurrentValue <= stat.MinValue)
                    {
                        Die(damageDealer);
                    }
                };
            }

            if (findStatBarInChildren)//this is just so the combat dummy's health displays
            {
                stat.statBar = GetComponentInChildren<StatBarItem>();
            }

            HitEffectTransform = hitEffectTransformOverride ? hitEffectTransformOverride : transform;
        }

        private void Start()
        {
            if (takeDefaultValueOnStart)
            {
                stat.TakeDefaultValue();
            }

            if (TryGetComponent(out IMover mover))
            {
                TargetingTolerance = mover.Width / 2;
            }
        }


        private void Update()
        {
            stat.Update();
        }

        //point of having this separate from GainHealth is to allow higher level scripts (like combatant)
        //to decide how to take damage (e.g. combatant may have bonuses or effects to take into account)
        public void ReceiveDamage(float damage, IDamageDealer damageDealer)
        {
            HealthChangeTrigger?.Invoke(damage, damageDealer);

            //combat controller will receive this and ask combatant to compute the effective damage
            //which is passed into gain health, and then cc broadcasts via an event the effective health change
        }

        public void GainHealth(float amount, bool clamped)
        {
            if (clamped)
            {
                stat.AddValueClamped(amount);
            }
            else
            {
                stat.CurrentValue += amount;
            }
        }

        public void ReceiveStun(float duration, bool freezeAnimation)
        {
            OnStunned?.Invoke(duration, freezeAnimation);
        }

        public void Die(IDamageDealer killer = null)
        {
            IsDead = true;
            Killer = killer;
            OnDeath?.Invoke(killer);
        }

        public void Revive()
        {
            Killer = null;
            stat.CurrentValue = stat.DefaultValue;
            IsDead = false;
        }

        public IHealth HealthComponent()
        {
            return this;
        }

        public static IHealth GetHealthComponent(Collider2D collider)
        {
            if (collider)
            {
                return collider.GetComponent<IHealthPointer>()?.HealthComponent();
            }

            return null;
        }

        private void OnDestroy()
        {
            HealthChangeTrigger = null;
            OnStunned = null;
            OnDeath = null;
        }
    }
}
