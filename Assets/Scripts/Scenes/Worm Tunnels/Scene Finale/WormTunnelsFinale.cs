using Cinemachine;
using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using RPGPlatformer.SceneManagement;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace RPGPlatformer.Cinematics
{
    public class WormTunnelsFinale : MonoBehaviour
    {
        [SerializeField] PlayerSpawnManager playerSpawnManager;
        [SerializeField] Combatant earthworm;
        [SerializeField] NoiseSettings cameraRumble;
        [SerializeField] Transform exitBlocker;
        [SerializeField] Transform fallingDebrisGroup;
        [SerializeField] Transform caveIn;
        [SerializeField] PlayableDirector caveInDirector;

        private void Start()
        {
            if (earthworm)
            {
                earthworm.DeathFinalized += OnEarthwormDeath;
            }

            fallingDebrisGroup.gameObject.SetActive(false);
            caveIn.gameObject.SetActive(false);
        }

        //so we don't have to worry about player death messing up anything, let's put a save checkpoint inside
        //the earthworm arena
        private async void OnEarthwormDeath()
        {
            earthworm.DeathFinalized -= OnEarthwormDeath;
            await MiscTools.DelayGameTime(3, GlobalGameTools.Instance.TokenSource.Token);
            //give player a few seconds to loot
            while (GlobalGameTools.Instance.PlayerIsDead)
            {
                //because we don't want disable/enable input to compete with the disable/enable input
                //that happens when player dies
                await Task.Yield();
                if (GlobalGameTools.Instance.TokenSource.Token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
            }

            GlobalGameTools.Instance.Player.Combatant.SetInvincible(true);
            ((IInputDependent)GlobalGameTools.Instance.Player).InputSource.DisableInput();
            PlayerFollowCamera.SetNoiseProfile(cameraRumble); 
            fallingDebrisGroup.gameObject.SetActive(true);
            await MiscTools.DelayGameTime(1, GlobalGameTools.Instance.TokenSource.Token);

            //BEGIN CUTSCENE
            caveInDirector.Play();
            caveInDirector.stopped += OnCaveInComplete;//gets called at end of timeline :)
            await MiscTools.DelayGameTime(0.35f, GlobalGameTools.Instance.TokenSource.Token);
            caveIn.gameObject.SetActive(true);
        }

        void OnCaveInComplete(PlayableDirector d)
        {
            if (caveInDirector)
            {
                caveInDirector.stopped -= OnCaveInComplete;
            }

            exitBlocker.gameObject.SetActive(false);
            ((IInputDependent)GlobalGameTools.Instance.Player).InputSource.EnableInput();
            GlobalGameTools.Instance.Player.Combatant.SetInvincible(false);
        }

        private void OnDestroy()
        {
            if (earthworm)
            {
                earthworm.DeathFinalized -= OnEarthwormDeath;
            }
            if (caveInDirector)
            {
                caveInDirector.stopped -= OnCaveInComplete;
            }
        }
    }
}