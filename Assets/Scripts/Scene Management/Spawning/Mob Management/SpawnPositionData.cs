using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    [SerializeField]
    public struct SpawnPositionData
    {
        enum BoundsSource
        {
            Transform, Position
        }
        enum Mode
        {
            Max, Random
        }

        [SerializeField] BoundsSource boundsSource;
        [SerializeField] Mode mode;
        [SerializeField] Transform minTransform;
        [SerializeField] Transform maxTransform;
        [SerializeField] Vector3 minPosition;
        [SerializeField] Vector3 maxPosition;

        public Vector3 GetSpawnPosition()
        {
            if (mode == Mode.Max)
            {
                return MaxPosition();
            }
            else
            {
                return MiscTools.RandomPointInBox(MinPosition(), MaxPosition());
            }
        }

        Vector3 MinPosition()
        {
            return boundsSource == BoundsSource.Transform ? minTransform.position : minPosition;
        }

        Vector3 MaxPosition()
        {
            return boundsSource == BoundsSource.Transform ? maxTransform.position : maxPosition;
        }
    }
}