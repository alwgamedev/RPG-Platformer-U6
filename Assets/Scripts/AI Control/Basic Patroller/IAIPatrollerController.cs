﻿using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    public interface IAIPatrollerController
    {
        public bool Patrolling { get; }
        //public IAIMovementController MovementController { get; }
        public IAIPatroller Patroller { get; }
        public object DefaultPatrolParams { get; set; }

        public void BeginDefaultPatrol();

        public void BeginPatrolRest();

        public void BeginPatrol(NavigationMode mode, object param);
    }
}