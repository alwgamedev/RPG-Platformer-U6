using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using RPGPlatformer.UI;
using UnityEngine;

public class PoisonMushroom : MonoBehaviour, IDamageDealer
{
    [SerializeField] ParticleSystem gasParticles;
    IHealth playerHealth => GlobalGameTools.Instance.Player.Combatant.Health;

    public CombatStyle CurrentCombatStyle => CombatStyle.Unarmed;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!gameObject.activeInHierarchy) return;

        if (collider.transform == GlobalGameTools.Instance.PlayerTransform)
        {
            if (CanSpawnGas())
            {
                SpawnGas();
            }
        }
    }

    private async void SpawnGas()
    {
        gasParticles.Play();
        GlobalGameTools.Instance.Player.MovementController.SetRunning(false);
        GameLog.Log("Stepping on the mushroom causes it to release a noxious green gas.");
        GameLog.Log($"You've been poisoned!");
        await AttackAbility.Bleed(this, playerHealth, 35, 12, 1);
        //+disable run
    }

    private bool CanSpawnGas()
    {
        return !gasParticles.isPlaying;
    }
}
