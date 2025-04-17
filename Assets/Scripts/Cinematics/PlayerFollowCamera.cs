using Cinemachine;
using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Cinematics
{
    public class PlayerFollowCamera : MonoBehaviour
    {
        static CinemachineVirtualCamera vc;

        static PlayerFollowCamera Instance;

        private void Awake()
        {
            Instance = this;
            vc = GetComponentInChildren<CinemachineVirtualCamera>();
        }

        public static void FollowPlayer(bool val)
        {
            if (vc)
            {
                if (val && GlobalGameTools.Instance)
                { 
                    vc.Follow = GlobalGameTools.Instance.PlayerTransform;
                }
                else
                {
                    vc.Follow = null;
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                vc = null;
            }
        }
    }
}