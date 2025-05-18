using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using RPGPlatformer.Movement;
using RPGPlatformer.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class PoisonMushroom : MonoBehaviour, IDamageDealer
    {
        struct IntruderData
        {
            public ICombatController cc;
            public bool inBounds;
            public bool canBePoisoned;

            public IntruderData(ICombatController cc, bool inBounds, bool canBePoisoned)
            {
                this.cc = cc;
                this.inBounds = inBounds;
                this.canBePoisoned = canBePoisoned;
            }
        }

        [SerializeField] ParticleSystem gasParticles;
        [SerializeField] TriggerColliderMessenger gasBounds;
        [SerializeField] float poisonDamage;
        [SerializeField] float poisonRate;
        [SerializeField] int lingeringBleedCount;

        Dictionary<Collider2D, IntruderData> intruders = new();

        bool gasActive => gasParticles && gasParticles.isPlaying;

        public CombatStyle CurrentCombatStyle => CombatStyle.Unarmed;

        private void Start()
        {
            gasBounds.TriggerEnter += OnGasBoundsStay;
            gasBounds.TriggerStay += OnGasBoundsStay;
            gasBounds.TriggerExit += OnGasBoundsExited;
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!gameObject.activeInHierarchy) return;

            if (intruders.TryGetValue(collider, out var d))
            {
                if (!gasActive)
                {
                    TriggerGas(collider.transform);
                }
            }
            else if (collider.TryGetComponent(out ICombatController cc))
            {
                intruders[collider] = new(cc, false, true);
                if (!gasActive)
                {
                    TriggerGas(collider.transform);
                }
            }
        }

        private void OnGasBoundsStay(Collider2D c)
        {
            if (intruders.TryGetValue(c, out var d))
            {
                if (!d.inBounds)
                {
                    d.inBounds = true;
                    intruders[c] = d;
                }

                if (gasActive && d.canBePoisoned && !d.cc.Combatant.Health.IsDead)
                {
                    PoisonIntruder(c);
                }
            }
            else if (c.TryGetComponent(out ICombatController cc))
            {
                intruders[c] = new(cc, true, true);
                if (gasActive && !cc.Combatant.Health.IsDead)
                {
                    PoisonIntruder(c);
                }
            }
        }

        private void OnGasBoundsExited(Collider2D c)
        {
            if (intruders.TryGetValue(c, out var d))
            {
                d.inBounds = false;
                intruders[c] = d;
            }
            else if (c.TryGetComponent(out ICombatController cc))
            {
                intruders[c] = new(cc, false, true);
            }
        }

        //private void CombatantEnteredGasBounds(ICombatController cc)
        //{
        //    f (gasActive)
        //    {
        //        PoisonIntruder(cc);
        //    }
        //}

        //private void CombatantExitedGasBounds(ICombatController cc)
        //{
        //    inBounds[cc] = false;
        //}

        //private bool CanTrigger(GameObject go)
        //{
        //    return ((1 << go.layer) & layersThatCanTrigger) != 0;
        //}

        private void TriggerGas(Transform t)
        {
            gasParticles.Play();
            if (t == GlobalGameTools.Instance.PlayerTransform)
            {
                GameLog.Log("Stepping on the mushroom causes it to release a noxious green gas.");
            }
            //PoisonIntruder(c);
        }

        private async void PoisonIntruder(Collider2D c)
        {
            var d = intruders[c];
            d.canBePoisoned = false;
            intruders[c] = d;

            if (d.cc == GlobalGameTools.Instance.Player)
            {
                GameLog.Log("You've been poisoned!");
            }

            d.cc.MovementController.SetRunning(false);

            while (gasActive && intruders[c].inBounds)
            {
                if (d.cc?.Combatant?.Health == null || !d.cc.Combatant.Health.transform)
                {
                    intruders.Remove(c);
                    return;
                }
                if (d.cc.Combatant.Health.IsDead)
                {
                    d.canBePoisoned = true;
                    intruders[c] = d;
                    return;
                }
                await PoisonBleedHit(d.cc.Combatant.Health);
            }

            //lingering bleed when intruder exits gas bounds
            int i = 0;
            while (gasActive && i < lingeringBleedCount)
            {
                if (d.cc?.Combatant?.Health == null || !d.cc.Combatant.Health.transform)
                {
                    intruders.Remove(c);
                }
                if (d.cc.Combatant.Health.IsDead)
                {
                    d.canBePoisoned = true;
                    intruders[c] = d;
                    return;
                }
                await PoisonBleedHit(d.cc.Combatant.Health);
                i++;
            }

            d.canBePoisoned = true;
            intruders[c] = d;

        }

        private async Task PoisonBleedHit(IHealth health)
        {
            AttackAbility.DealDamage(this, health, poisonDamage);
            await MiscTools.DelayGameTime(poisonRate, GlobalGameTools.Instance.TokenSource.Token);
        }

        private void OnDestroy()
        {
            gasBounds.TriggerEnter -= OnGasBoundsStay;
            gasBounds.TriggerStay -= OnGasBoundsStay;
            gasBounds.TriggerExit -= OnGasBoundsExited;
        }
    }
}
