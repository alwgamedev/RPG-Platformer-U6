using System;
using System.Threading.Tasks;
using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : PoolableObject, IProjectile
    {
        [SerializeField] protected SpriteRenderer head;
        [SerializeField] protected ParticleSystem trailEffect;
        [SerializeField] protected ParticleSystem[] impactEffects = Array.Empty<ParticleSystem>();
        [SerializeField] protected float shootForce;
        [SerializeField] protected float forceMultiplierScale = 1;
        [SerializeField] protected float timeToWaitAfterImpactBeforeReturningToPool = 1.5f;
        [SerializeField] protected float maxLifetime = 5;
        [SerializeField] protected bool updateRotationWhileFlying = false;
        [SerializeField] protected bool freezePositionOnFinalImpact; 
        [SerializeField] protected float timeToBlockEnemyPath = .4f;
        [SerializeField] protected Collider2D triggerCollider;
        [SerializeField] protected Collider2D dynamicCollider;

        protected float powerMultiplier = 1;
        protected int maxHits = 1;
        protected Transform shooter;
        protected Func<Vector2> GetAimPos;
        protected Action<Collider2D> HitAction;

        protected float lifeTimer;
        protected float hits;
        protected Rigidbody2D myRigidbody;
        public float PowerMultiplier => powerMultiplier;
        public Transform Transform => transform;

        protected virtual void Awake()
        {
            myRigidbody = GetComponent<Rigidbody2D>();
        }

        protected virtual void Update()
        {
            if (lifeTimer < maxLifetime)
            {
                lifeTimer += Time.deltaTime;
            }
            else
            {
                ReturnToPool();
            }
        }

        protected virtual void FixedUpdate()
        {
            if (updateRotationWhileFlying)
            {
                transform.up = myRigidbody.linearVelocity;
            }
        }


        //PREPARE AND SHOOT

        public void Prepare(ICombatant combatant, Func<Vector2> getAimPos, float powerMultiplier, Action<Collider2D> hitAction, int maxHits = 1)
        {
            EnableHead(false);
            triggerCollider.enabled = false;
            transform.SetParent(combatant.EquipSlots[EquipmentSlot.Mainhand].transform);
            transform.localPosition = Vector3.zero;
            this.powerMultiplier = powerMultiplier;
            this.maxHits = maxHits;
            GetAimPos = getAimPos;
            HitAction = hitAction;
            shooter = combatant.Transform;
        }
        private void LookAtTarget(Vector2 aimPos)
        {
            transform.up = aimPos - (Vector2)transform.position;//transform.up is automatically normalized
        }

        public virtual void Shoot()
        {
            if (GetAimPos == null || !shooter)
            {
                ReturnToPool();
                return;
            }

            EnableHead(true);
            triggerCollider.enabled = true;
            transform.SetParent(null, true);
            if (trailEffect)
            {
                trailEffect.Play();
            }
            LookAtTarget(GetAimPos());
            myRigidbody.AddForce(powerMultiplier * shootForce * forceMultiplierScale * transform.up, ForceMode2D.Impulse);
        }


        //COLLISION HANDLING

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            OnCollide(collider);
        }

        protected virtual void OnTriggerExit2D(Collider2D collider)
        {
            OnCollide(collider);
        }

        protected virtual void OnCollide(Collider2D collider)
        {
            if (collider.gameObject.transform != shooter && hits < maxHits)
            {
                hits++;
                if(hits >= maxHits && freezePositionOnFinalImpact)
                {
                    myRigidbody.linearVelocity = Vector2.zero;
                    transform.rotation = Quaternion.identity;
                }
                OnHit(collider);
            }
        }

        protected virtual async void OnHit(Collider2D collider)
        {
            HitAction?.Invoke(collider);
            if (hits >= maxHits || !collider.TryGetComponent<Health>(out _))
            {
                OnLastHit();
                if (dynamicCollider)
                {
                    await MiscTools.DelayGameTime(timeToBlockEnemyPath, GlobalGameTools.Instance.TokenSource.Token);
                    if (!GlobalGameTools.Instance.TokenSource.IsCancellationRequested && gameObject.activeSelf)
                    {
                        EnableDynamicCollider(false);
                    }
                }
            }
        }


        //STATE TRANSITIONS

        protected virtual void OnLastHit()
        {
            EnableHead(false);
            triggerCollider.enabled = false;

            if (trailEffect)
            {
                trailEffect.Stop();
            }

            if (impactEffects != null)
            {
                foreach (ParticleSystem ps in impactEffects)
                {
                    if (ps)
                    {
                        ps.Play();
                    }
                }
            }

            Invoke(nameof(ReturnToPool), timeToWaitAfterImpactBeforeReturningToPool);
        }

        protected virtual void EnableHead(bool enable)
        {
            head.enabled = enable;
            myRigidbody.bodyType = enable ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
        }

        protected virtual void EnableDynamicCollider(bool enable)
        {
            if(dynamicCollider)
            {
                dynamicCollider.enabled = enable;
            }
        }

        public override void ResetPoolableObject()
        {
            lifeTimer = 0;
            powerMultiplier = 1;
            maxHits = 1;
            hits = 0;
            GetAimPos = null;
            HitAction = null;
            shooter = null;
            EnableHead(true);
            triggerCollider.enabled = true;
            EnableDynamicCollider(true);
        }

        private void OnDestroy()
        {
            GetAimPos = null;
            HitAction = null;
        }
    }
}