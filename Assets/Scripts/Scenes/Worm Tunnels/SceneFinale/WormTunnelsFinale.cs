using Cinemachine;
using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Cinematics
{
    public class WormTunnelsFinale : MonoBehaviour
    {
        [SerializeField] Combatant earthworm;
        [SerializeField] NoiseSettings cameraRumble;

        private void Start()
        {
            if (earthworm)
            {
                earthworm.DeathFinalized += OnEarthwormDeath;
            }
        }

        private async void OnEarthwormDeath()
        {
            earthworm.DeathFinalized -= OnEarthwormDeath;
            if (GlobalGameTools.Instance.PlayerIsDead) return;
            //^e.g. if player dies at same time as worm, we don't want to worry about racing
            //against the respawn portal, so player will just have to escape out of the way they came in
            await MiscTools.DelayGameTime(4, GlobalGameTools.Instance.TokenSource.Token);
            PlayerFollowCamera.SetNoiseProfile(cameraRumble);
            //disable player input for a cinematic cutscene
            //rocks will fall blocking the way player came in (to right)
            //and a passage out will open up (to left)
            //re-enable player input
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