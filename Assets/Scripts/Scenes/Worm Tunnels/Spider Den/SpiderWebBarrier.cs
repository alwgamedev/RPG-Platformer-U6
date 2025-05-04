using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using RPGPlatformer.UI;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class SpiderWebBarrier : MonoBehaviour
    {
        [SerializeField] LineRenderer topRenderer;
        [SerializeField] LineRenderer bottomRender;
        [SerializeField] HingeJoint2D[] joints;
        [SerializeField] Joint2D breakPointTop;
        [SerializeField] Joint2D breakPointBottom;
        [SerializeField] Vector2 topBreakAcceleration;
        [SerializeField] Vector2 bottomBreakAcceleration;
        [SerializeField] Vector2 damageJiggleAcceleration;
        [SerializeField] float snapTime;

        Health health;
        HealthBarCanvas healthBarCanvas;
        bool healthBarConfigured;

        Rigidbody2D breakPointTopRb;
        Rigidbody2D breakPointBottomRb;

        Collider2D[] colliders;

        bool broken;
        bool snapping;
        float snapTimer;
        Vector2[] initialSnapAnchors;

        private void Awake()
        {
            health = GetComponent<Health>();
            healthBarCanvas = GetComponentInChildren<HealthBarCanvas>();

            breakPointTopRb = breakPointTop.GetComponent<Rigidbody2D>();
            breakPointBottomRb = breakPointBottom.GetComponent<Rigidbody2D>();

            colliders = new Collider2D[joints.Length];

            for (int i = 0; i < joints.Length; i++)
            {
                if (joints[i].TryGetComponent(out Collider2D c))
                {
                    colliders[i] = c;
                }
            }
        }

        //my plan is that mob manager will enable this component when mother spider dies
        //(or when all enemies in room slain)
        //and that will set up the health and make wall breakable
        private void Start()
        {
            ConfigureHealth();
        }

        private void Update()
        {
            //just for quick access in testing;
            if (Input.GetKeyDown(KeyCode.M))
            {
                GlobalGameTools.Instance.Player.Combatant.transform.position
                    = transform.position - 2 * Vector3.right;
            }

            if (snapping)
            {
                snapTimer -= Time.deltaTime;
                if (snapTimer <= 0)
                {
                    snapping = false;
                    Destroy(gameObject);
                    return;
                }

                var q = snapTime / snapTimer;
                q = q * q;
                var s = topRenderer.textureScale;
                topRenderer.textureScale = new(s.x, q);
                s = bottomRender.textureScale;
                bottomRender.textureScale = new(s.x, q);

                q = 1 / (q * q);

                for (int i = 0; i < joints.Length; i++)
                {
                    joints[i].anchor = q * initialSnapAnchors[i];
                }
            }
        }

        private void OnMouseEnter()
        {
            healthBarCanvas.OnMouseEnter();
        }

        private void OnMouseExit()
        {
            healthBarCanvas.OnMouseExit();
        }

        private void ConfigureHealth()
        {
            if (!healthBarConfigured)
            {
                healthBarConfigured = true;
                healthBarCanvas.Configure(health);
            }
            health.OnDeath += DeathHandler;
            health.HealthChangeTrigger += DamageHandler;
        }

        private void DamageHandler(float damage, IDamageDealer d)
        {
            if (!health.IsDead && damage > 0)
            {
                breakPointTopRb.AddForce(breakPointTopRb.mass * damageJiggleAcceleration);
            }
        }

        private void DeathHandler(IDamageDealer d)
        {
            Break();
        }

        private void Break()
        {
            if (broken) return;

            breakPointBottom.enabled = false;

            initialSnapAnchors = new Vector2[joints.Length];

            for (int i = 0; i < joints.Length; i++)
            {
                //j.anchor = Vector2.zero;
                initialSnapAnchors[i] = joints[i].anchor;
                joints[i].autoConfigureConnectedAnchor = false;
                joints[i].useLimits = true;
                if (colliders[i])
                {
                    colliders[i].enabled = false;
                }
                //j.anchor = Vector2.zero;
            }

            broken = true;
            snapping = true;
            snapTimer = snapTime;

            breakPointTopRb.AddForce(breakPointTopRb.mass * topBreakAcceleration, ForceMode2D.Impulse);
            breakPointBottomRb.AddForce(breakPointBottomRb.mass * bottomBreakAcceleration, ForceMode2D.Impulse);
        }

        //private void OnDisable()
        //{
        //    health.OnDeath -= DeathHandler;
        //    health.HealthChangeTrigger -= DamageHandler;
        //}
    }
}