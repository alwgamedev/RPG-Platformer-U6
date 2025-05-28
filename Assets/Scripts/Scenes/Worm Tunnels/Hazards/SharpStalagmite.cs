using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class SharpStalagmite : MonoBehaviour, IDamageDealer
    {
        [SerializeField] float triggerForce;
        [SerializeField] float forceDamageScaler;
        [SerializeField] float maxDamage;
        [SerializeField] float hitForceScaler;
        [SerializeField] float maxHitForce;

        public CombatStyle CurrentCombatStyle => CombatStyle.Unarmed;

        private float HitDamage(float collisionForce)
        {
            return Mathf.Min(collisionForce * forceDamageScaler, maxDamage);
        }

        private float HitForce(float collisionForce)
        {
            return Mathf.Min(collisionForce * hitForceScaler, maxHitForce);
        }

        private void OnPlayerCollision(Collision2D collision)
        {
            var c = collision.GetContact(0);
            if (c.normalImpulse < triggerForce) return;

            var pHealth = GlobalGameTools.Instance.Player.Combatant.Health;
            var pRb = ((Mover)GlobalGameTools.Instance.PlayerMover.Mover).Rigidbody;
            AttackAbility.DealDamage(this, pHealth, HitDamage(c.normalImpulse));
            pRb.linearVelocity = Vector2.zero;
            pRb.totalForce = Vector2.zero;
            pRb.AddForce(HitForce(c.normalImpulse) * (2 * Vector2.up - c.normal), ForceMode2D.Impulse);

        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.transform == GlobalGameTools.Instance.PlayerTransform
                && !GlobalGameTools.Instance.PlayerIsDead)
            {
                OnPlayerCollision(collision);
            }
        }
    }
}