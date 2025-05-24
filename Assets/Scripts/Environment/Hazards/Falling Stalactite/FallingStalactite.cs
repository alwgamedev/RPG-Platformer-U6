using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

namespace RPGPlatformer.Environment
{
    [RequireComponent(typeof(BreakableObject))]
    public class FallingStalactite : PoolableObject, IDamageDealer
    {
        [SerializeField] float impactDamage;
        [SerializeField] float rumbleDisplacement;
        [SerializeField] float rumbleFrequency;//time to go from 0 to rumbleDisplacement
        [SerializeField] float rumbleDuration;
        [SerializeField] Transform sHead;
        [SerializeField] Transform sBase;
        [SerializeField] float emergeSpeed;

        Collider2D ceiling;
        Collider2D containerCollider;
        Rigidbody2D containerRb;
        BreakableObject breakableObject;
        State state;

        enum State
        {
            dormant, rumbling, falling, broken
        }

        public CombatStyle CurrentCombatStyle => CombatStyle.Unarmed;

        private void Awake()
        {
            breakableObject = GetComponent<BreakableObject>();
            containerRb = GetComponent<Rigidbody2D>();
            containerCollider = GetComponent<Collider2D>();
        }

        private void Start()
        {
            containerCollider.enabled = false;
            state = State.dormant;
        }

        //private void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.P))
        //    {
        //        Trigger();
        //    }
        //}


        //BASIC FUNCTIONS

        public async Task Emerge(CancellationToken token)
        {
            Vector3 d = sHead.position - sBase.position;
            float l = d.magnitude;
            if (l < 1E-05f) return;

            d = d / l;
            float t = 0;
            float T = l / emergeSpeed;

            while (t < T)
            {
                await Task.Yield();
                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                if (state != State.dormant && state != State.rumbling)
                {
                    return;
                }

                t += Time.deltaTime;
                transform.position += d * emergeSpeed * Time.deltaTime;
            }
        }

        public async void Trigger()
        {
            if (state != State.dormant) return;

            await Rumble(GlobalGameTools.Instance.TokenSource.Token);
            Fall();
        }

        private async Task Rumble(CancellationToken token)
        {
            state = State.rumbling;
            float timer = 0;
            float d;
            float displacement = 0;
            float speed = rumbleDisplacement / rumbleFrequency;
            int direction = 1;

            while (timer < rumbleDuration)
            {
                await Task.Yield();
                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                timer += Time.deltaTime;
                d = Time.deltaTime * speed * direction;
                displacement += d;
                transform.position += d * Vector3.right;
                if (displacement * direction > rumbleDisplacement)
                {
                    direction *= -1;
                }
            }
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
            transform.position += transform.position - sHead.position;
        }

        public override async void AfterSetActive()
        {
            await Emerge(GlobalGameTools.Instance.TokenSource.Token);
        }

        public override void ResetPoolableObject() { }
    }
}