using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using RPGPlatformer.Movement;
using RPGPlatformer.UI;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class PoisonMushroom : MonoBehaviour, IDamageDealer
    {
        [SerializeField] ParticleSystem gasParticles;
        [SerializeField] TriggerColliderMessenger gasBounds;
        [SerializeField] float poisonDamage;
        [SerializeField] float poisonRate;
        [SerializeField] int lingeringBleedCount;

        bool playerInBounds;

        IHealth playerHealth => GlobalGameTools.Instance.Player.Combatant.Health;
        bool gasActive => gasParticles && gasParticles.isPlaying;

        public CombatStyle CurrentCombatStyle => CombatStyle.Unarmed;

        private void Start()
        {
            gasBounds.TriggerEnter += OnGasBoundsEntered;
            gasBounds.TriggerExit += OnGasBoundsExited;
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!gameObject.activeInHierarchy) return;

            if (collider.transform == GlobalGameTools.Instance.PlayerTransform)
            {
                if (!gasActive)
                {
                    TriggerGas();
                }
            }
        }

        private void OnGasBoundsEntered(Collider2D collider)
        {
            if (collider.transform == GlobalGameTools.Instance.PlayerTransform)
            {
                PlayerEnteredGasBounds();
            }
        }

        private void OnGasBoundsExited(Collider2D collider)
        {
            if (collider.transform == GlobalGameTools.Instance.PlayerTransform)
            {
                PlayerExitedGasBounds();
            }
        }

        private void PlayerEnteredGasBounds()
        {
            playerInBounds = true;

            if (gasActive)
            {
                PoisonPlayer();
            }
        }

        private void PlayerExitedGasBounds()
        {
            playerInBounds = false;
        }

        private void TriggerGas()
        {
            gasParticles.Play();
            GameLog.Log("Stepping on the mushroom causes it to release a noxious green gas.");
            PoisonPlayer();
        }

        private async void PoisonPlayer()
        {
            GameLog.Log("You've been poisoned!");
            GlobalGameTools.Instance.Player.MovementController.SetRunning(false);

            while (gasActive && playerInBounds)
            {
                if (playerHealth == null || playerHealth.IsDead)
                    break;
                await PoisonBleedHit();
            }

            //lingering bleed when player exits gas bounds
            int i = 0;
            while (gasActive && i < lingeringBleedCount)
            {
                if (playerHealth == null || playerHealth.IsDead)
                    break;
                await PoisonBleedHit();
                i++;
            }
        }

        private async Task PoisonBleedHit()
        {
            AttackAbility.DealDamage(this, playerHealth, poisonDamage);
            await MiscTools.DelayGameTime(poisonRate, GlobalGameTools.Instance.TokenSource.Token);
        }

        private void OnDestroy()
        {
            gasBounds.TriggerEnter -= OnGasBoundsEntered;
            gasBounds.TriggerExit -= OnGasBoundsExited;
        }
    }
}
