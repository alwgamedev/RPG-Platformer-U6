using System;
using UnityEngine;
using RPGPlatformer.UI;

namespace RPGPlatformer.Combat
{
    public class Health : MonoBehaviour, IHealth
    {
        [SerializeField] bool takeDamageAutomatically;//these 3 bools are mainly for the combat dummy.
        [SerializeField] bool takeDefaultValueOnStart;//characters with a combat controller will set up 
        [SerializeField] bool findStatBarInChildren;//their Health themselves
        [SerializeField] ReplenishableStat stat;

        public bool IsDead { get; private set; }
        public IDamageDealer Killer { get; private set; }
        public Transform Transform => transform; 
        public ReplenishableStat Stat => stat;

        public event Action<float, IDamageDealer> HealthChanged;
        public event Action<float, bool> OnStunned;//signature is (duration, freezeAnimation)
        public event Action<IDamageDealer> OnDeath;

        //private void Awake()
        //{
        //    if (CompareTag("Player"))
        //    {
        //        stat.statBar = GameObject.Find("Player Health Bar").GetComponent<StatBarItem>();
        //    }
        //    else
        //    {
        //        stat.statBar = GetComponentInChildren<StatBarItem>();
        //    }
        //}

        //private void OnEnable()
        //{

        //}

        private void Start()
        {
            if (takeDamageAutomatically)//mainly for combat dummy
            {
                HealthChanged += (damage, damageDealer) => GainHealth(-damage, true);
            }

            if (findStatBarInChildren)//this is just so the combat dummy's health displays
            {
                stat.statBar = GetComponentInChildren<StatBarItem>();
            }

            if (takeDefaultValueOnStart)
            {
                stat.TakeDefaultValue();
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
            HealthChanged?.Invoke(damage, damageDealer);
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

        private void OnDestroy()
        {
            HealthChanged = null;
            OnStunned = null;
            OnDeath = null;
        }
    }
}
