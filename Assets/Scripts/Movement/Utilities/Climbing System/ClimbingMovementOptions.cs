using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    [Serializable]
    public struct ClimbingMovementOptions
    {
        [SerializeField] float nodeDetectionRadius;
        [SerializeField] float climbSpeed;
        [SerializeField] float rotationSpeed;
        [SerializeField] float positionTweenRate;

        public float NodeDetectionRadius => nodeDetectionRadius;
        public float ClimbSpeed => climbSpeed;
        public float RotationSpeed => rotationSpeed;
        public float PositionLerpRate => positionTweenRate;
    }
}