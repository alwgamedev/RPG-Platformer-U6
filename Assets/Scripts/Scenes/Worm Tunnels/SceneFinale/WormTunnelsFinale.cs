using Cinemachine;
using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using RPGPlatformer.SceneManagement;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGPlatformer.Cinematics
{
    public class WormTunnelsFinale : MonoBehaviour
    {
        [SerializeField] PlayerSpawnManager playerSpawnManager;
        [SerializeField] Combatant earthworm;
        [SerializeField] NoiseSettings cameraRumble;
        [SerializeField] Transform exitBlocker;
        [SerializeField] Transform fallingDebrisGroup;

        private void Start()
        {
            if (earthworm)
            {
                earthworm.DeathFinalized += OnEarthwormDeath;
                fallingDebrisGroup.gameObject.SetActive(false);
            }
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
            //begin small rocks falling from ceiling
            await MiscTools.DelayGameTime(1, GlobalGameTools.Instance.TokenSource.Token);

            //BEGIN CUTSCENE
            //show rocks falling, blocking the way player came in (to right)
            fallingDebrisGroup.gameObject.SetActive(true);
            exitBlocker.gameObject.SetActive(false);
            //END CUTSCENE

            ((IInputDependent)GlobalGameTools.Instance.Player).InputSource.EnableInput();
            GlobalGameTools.Instance.Player.Combatant.SetInvincible(false);
            //dust and small rocks falling from ceiling (dealing small damage)
            //may add millipede chasing as well (millipede one-hits player and is invincible (can't be damaged))
            //player should run left
            //if player dies during this time they will just be teleported out of worm tunnels instantly
            //(although maybe with a fade to black)
            //(let's actually add a black fadeout/fadein for all scene transitions)
            //far enough down the exit corridor player will be snatched by evil roots,
            //and pulled up into ceiling; then trigger scene transition back to open scene
            //will emerge out of the tunnels entrance with an upward force (as if being thrown out by the evil roots)
            //(and maybe force goes to side slightly so you don't fall straight back in)
        }

        private void OnDestroy()
        {
            if (earthworm)
            {
                earthworm.DeathFinalized -= OnEarthwormDeath;
            }
        }
    }
}