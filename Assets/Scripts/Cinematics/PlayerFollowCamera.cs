using Cinemachine;
using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Cinematics
{
    public class PlayerFollowCamera : MonoBehaviour
    {
        static CinemachineVirtualCamera vc;

        private void Awake()
        {
            vc = GetComponentInChildren<CinemachineVirtualCamera>();
        }

        public static void FollowPlayer(bool val)
        {
            vc.Follow = val ? GlobalGameTools.Instance.PlayerTransform : null;
        }
    }
}