using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class PoolableAIPatrollerConfigurationParameters : MonoBehaviour
    {
        [SerializeField] MBNavigationParameters patrolParameters;
        [SerializeField] Transform leftMovementBound;
        [SerializeField] Transform rightMovementBound;
        [SerializeField] Transform leftAttackBound;
        [SerializeField] Transform rightAttackBound;

        public MBNavigationParameters PatrolParameters => patrolParameters;
        public Transform LeftMovementBound => leftMovementBound;
        public Transform RightMovementBound => rightMovementBound;
        public Transform LeftAttackBound => leftAttackBound;
        public Transform RightAttackBound => rightAttackBound;

    }
}