using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.Movement;
using System;
using RPGPlatformer.Combat;

namespace RPGPlatformer.AIControl
{
    public class EelVertex : MonoBehaviour
    {
        public float wiggleMax;
        public float wiggleTime;//time to go from 0 to wiggleMax

        EelVertex leader;
        EelVertex follower;
        int wiggleDirection = 1;//1 = up, -1 = down
        //float wiggleTimer;//oscilates between -1 & 1
        //VisualCurveGuidePoint vcgp;//will be childed to EelVertex, so that EelVertex.pos can be the neutral position

        public float WiggleTimer { get; private set; }
        public VisualCurveGuidePoint VCGP { get; private set; }
        public CollisionMessenger CollisionMessenger { get; private set; }
        public CapsuleCollider2D Collider { get; private set; }
        public ParticleSystem ParticleSystem { get; private set; }

        public event Action<Vector2> ShockTrigger;

        private void Awake()
        {
            VCGP = GetComponentInChildren<VisualCurveGuidePoint>();
            CollisionMessenger = GetComponentInChildren<CollisionMessenger>();
            Collider = GetComponentInChildren<CapsuleCollider2D>();
            ParticleSystem = GetComponentInChildren<ParticleSystem>();
        }

        private void Start()
        {
            if (CollisionMessenger)
            {
                CollisionMessenger.CollisionEnter += OnCollision;
            }
        }

        public void Configure(EelVertex leader, EelVertex follower, float spacing)
        {
            this.leader = leader;
            this.follower = follower;

            if (leader)
            {
                transform.position = leader.transform.position - spacing * Vector3.right;
                var s = Collider.size;
                s.x = spacing;
                Collider.size = s;
                Collider.offset = new(spacing / 2, 0);
            }

            if (ParticleSystem)
            {
                var sh = ParticleSystem.shape;
                var t = sh.scale;
                t.x = spacing;
                sh.scale = t;
            }
        }


        //ANIMATION

        public void InitializeWiggle(int direction, float time)
        {
            wiggleDirection = direction;
            WiggleTimer = time;
        }

        public void UpdateWiggle(HorizontalOrientation o, float dt)
        {
            if (!leader) return;
            var u = ((Vector2)leader.transform.position - (Vector2)transform.position).normalized;
            var v = ((int)o) * u.CCWPerp();
            var z = 1 / Mathf.Sqrt(1 + leader.WiggleTimer * leader.WiggleTimer);
            //because sin'(x) = sin(x + pi/2), slope of the eel's curve at vertex[i]
            //should be vertex[i - 1].leadWiggletimer,
            //so tangent vector should be (z, leadWiggleTimer * z) (in the basis u,v, with z as above)

            Collider.transform.right = u;

            if (WiggleTimer * wiggleDirection > wiggleTime)
            {
                wiggleDirection *= -1;
            }

            WiggleTimer += wiggleDirection * dt;
            VCGP.SetPoint((Vector2)transform.position + WiggleTimer * wiggleMax * v);
            VCGP.SetTangentDir(z * u + leader.WiggleTimer * z * v);

            //if (ParticleSystem)
            //{
            //    ParticleSystem.transform.right = leader.VCGP.Point() - VCGP.Point();
            //}
        }

        public void UpdateParticleSystemRotation()
        {
            if (follower && ParticleSystem)
            {
                ParticleSystem.transform.right = VCGP.Point() - follower.VCGP.Point();
            }
        }

        private void OnCollision(Collision2D collision)
        {
            if (collision.transform == GlobalGameTools.Instance.PlayerTransform
                && !GlobalGameTools.Instance.PlayerIsDead)
            {
                ShockTrigger?.Invoke(collision.GetContact(0).normal);
            }
        }

        private void OnDestroy()
        {
            ShockTrigger = null;
        }
    }
}