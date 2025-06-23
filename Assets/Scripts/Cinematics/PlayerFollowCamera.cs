using Cinemachine;
using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Cinematics
{
    public class PlayerFollowCamera : MonoBehaviour
    {
        static CinemachineVirtualCamera VC;
        static CinemachineBasicMultiChannelPerlin Noise;
        //why static? so we can make the methods below static
        static PlayerFollowCamera Instance;
        //and why this? so we know who's responsible for clearing the static fields OnDestroy 

        private void Awake()
        {
            Instance = this;
            VC = GetComponentInChildren<CinemachineVirtualCamera>();
            if (VC)
            {
                Noise = VC.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            }
        }

        public static void FollowPlayer(bool val)
        {
            if (VC)
            {
                if (val && GlobalGameTools.Instance)
                { 
                    VC.Follow = GlobalGameTools.Instance.PlayerTransform;
                }
                else
                {
                    VC.Follow = null;
                }
            }
        }

        public static void SetNoiseProfile(NoiseSettings n)
        {
            if (Noise)
            {
                Noise.m_NoiseProfile = n;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                VC = null;
                Noise = null;
            }
        }
    }
}