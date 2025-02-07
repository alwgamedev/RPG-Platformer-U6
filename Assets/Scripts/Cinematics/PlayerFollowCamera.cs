using Cinemachine;
using UnityEngine;

namespace RPGPlatformer.Cinematics
{
    public class PlayerFollowCamera : MonoBehaviour
    {
        CinemachineVirtualCamera vc;

        private void Awake()
        {
            vc = GetComponentInChildren<CinemachineVirtualCamera>();
        }

        private void Start()
        {
            FindAndFollowPlayer();
        }

        private void FindAndFollowPlayer()
        {
            Transform player = GameObject.FindWithTag("Player").transform;
            vc.Follow = player;
        }
    }
}