using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    [Serializable]
    public struct WallDetectionOptions
    {
        [SerializeField] bool detectWalls;
        [SerializeField] int numWallCastsPerThird;
        [SerializeField] float wallCastDistanceFactor;
        [SerializeField] float wallClingRotationSpeed;

        public bool DetectWalls => detectWalls;
        public int NumWallCastsPerThird => numWallCastsPerThird;
        public float WallCastDistanceFactor => wallCastDistanceFactor;
        public float WallClingRotationSpeed => wallClingRotationSpeed;
    }
}