﻿using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

namespace RPGPlatformer.Environment
{
    [RequireComponent(typeof(BreakableObject))]
    public class FallingStalactite : PoolableObject, IDamageDealer
    {
        [SerializeField] RandomizableVector3 scale;
        [SerializeField] float impactDamage;
        [SerializeField] Transform sHead;
        [SerializeField] Transform sBase;
        [SerializeField] float emergeTime;
        [SerializeField] float anchorHeightBuffer;

        Collider2D ceiling;
        Collider2D containerCollider;
        Rigidbody2D containerRb;
        BreakableObject breakableObject;
        State state;

        enum State
        {
            dormant, falling, broken
        }

        public CombatStyle CurrentCombatStyle => CombatStyle.Unarmed;

        private void Awake()
        {
            breakableObject = GetComponent<BreakableObject>();
            containerRb = GetComponent<Rigidbody2D>();
            containerCollider = GetComponent<Collider2D>();
            transform.localScale = scale.Value;
        }

        private void Start()
        {
            containerCollider.enabled = false;
            state = State.dormant;
        }


        //BASIC FUNCTIONS

        public async Task Emerge(CancellationToken token)
        {
            Vector2 p = transform.position;
            Vector2 q = transform.position + sHead.position - sBase.position;

            float t = 0;

            while (t < emergeTime)
            {
                await Task.Yield();
                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                if (state != State.dormant /*&& state != State.rumbling*/)
                {
                    return;
                }

                t += Time.deltaTime;
                transform.position = Vector2.Lerp(p, q, t / emergeTime);
            }
        }

        public void Trigger()
        {
            if (state != State.dormant) return;

            Fall();
        }

        private void Fall()
        {
            containerCollider.enabled = true;
            containerRb.bodyType = RigidbodyType2D.Dynamic;
            state = State.falling;
        }

        private void DamagePlayer(float damage)
        {
            AttackAbility.DealDamage(this, GlobalGameTools.Instance.Player.Combatant.Health, damage);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (state != State.falling || collision.collider == ceiling || breakableObject.HasBroken) return;
            state = State.broken;
            if (collision.transform == GlobalGameTools.Instance.PlayerTransform)
            {
                DamagePlayer(impactDamage);
            }
            breakableObject.Break(collision);
            //NOTE: breakable object will disable container collider and destroy game object
        }


        //POOLABLE OBJECT METHODS

        public override void Configure(object parameters)
        {
            ceiling = ((GameObject)parameters).GetComponent<Collider2D>();
        }

        public override void BeforeSetActive()
        {
            transform.position += transform.position - sHead.position 
                + anchorHeightBuffer * Vector3.up;
        }

        public override async void AfterSetActive()
        {
            await Emerge(GlobalGameTools.Instance.TokenSource.Token);
        }

        public override void ResetPoolableObject() { }
    }
}