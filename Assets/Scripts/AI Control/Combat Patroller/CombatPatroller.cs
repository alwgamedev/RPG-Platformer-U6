﻿using UnityEngine;
using RPGPlatformer.Combat;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    [RequireComponent(typeof(AINavigator))]
    public class CombatPatroller : GenericCombatPatroller<IAIMovementController, AICombatController>
    { }
}