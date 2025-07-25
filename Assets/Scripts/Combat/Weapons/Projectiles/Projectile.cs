﻿using System;
using System.Collections.Generic;
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
        protected Func<Collider2D, IHealth> HitAction;
        protected List<IHealth> hitHealths = new();

        protected float lifeTimer;
        protected float hits;
        protected Rigidbody2D myRigidbody;
        public float PowerMultiplier => powerMultiplier;
        //public Transform transform => base.transform;

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

        public void Prepare(ICombatant combatant, Func<Vector2> getAimPos, float powerMultiplier, 
            Func<Collider2D, IHealth> hitAction, int maxHits = 1)
        {
            EnableHead(false);
            triggerCollider.enabled = false;
            transform.SetParent(combatant.ProjectileAnchor);
            transform.localPosition = Vector3.zero;
            this.powerMultiplier = powerMultiplier;
            this.maxHits = maxHits;
            GetAimPos = getAimPos;
            HitAction = hitAction;
            shooter = combatant.transform;
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
            myRigidbody.AddForce(powerMultiplier * shootForce * forceMultiplierScale * transform.up,
                ForceMode2D.Impulse);
        }

        //health assumed to be non-null
        public bool CheckIfRepeatHit(IHealth health)
        {
            if (health == null)
            {
                return false;
            }
            if (hitHealths.Contains(health))
            {
                return true;
            }

            hitHealths.Add(health);
            return false;
        }


        //COLLISION HANDLING

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            if (gameObject.activeInHierarchy)
            {
                OnCollide(collider);
            }
        }

        protected virtual void OnTriggerExit2D(Collider2D collider)
        {
            if (gameObject.activeInHierarchy)
            {
                OnCollide(collider);
            }
        }

        protected virtual void OnCollide(Collider2D collider)
        {
            if (collider.gameObject.activeInHierarchy 
                && collider.gameObject.transform != shooter && hits < maxHits)
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
            var colliderHealth = HitAction?.Invoke(collider);
            if (hits >= maxHits || colliderHealth == null)
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
            hitHealths = new();
            EnableHead(true);
            triggerCollider.enabled = true;
            EnableDynamicCollider(true);
        }

        public override void Configure(object parameters) { }

        public override void BeforeSetActive() { }

        public override void AfterSetActive() { }

        private void OnDestroy()
        {
            GetAimPos = null;
            HitAction = null;
        }
    }
}