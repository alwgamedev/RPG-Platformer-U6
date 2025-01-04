using System;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    public interface IProjectile
    {
        public float PowerMultiplier { get; }
        public Transform Transform { get; }

        public void Prepare(ICombatant combatant, Vector2 aimPos, float powerMultiplier, Action<Collider2D> hitAction, int maxHits = 1);

        public void Shoot();
    }
}