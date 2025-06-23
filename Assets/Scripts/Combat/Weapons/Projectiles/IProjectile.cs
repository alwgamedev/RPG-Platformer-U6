using System;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    public interface IProjectile
    {
        public float PowerMultiplier { get; }
        public Transform transform { get; }

        public void Prepare(ICombatant combatant, Func<Vector2> getAimPos, float powerMultiplier, 
            Func<Collider2D, IHealth> hitAction, int maxHits = 1);

        public void Shoot();

        public bool CheckIfRepeatHit(IHealth health);
    }
}