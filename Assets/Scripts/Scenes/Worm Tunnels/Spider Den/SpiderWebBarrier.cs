using RPGPlatformer.AIControl;
using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using RPGPlatformer.SceneManagement;
using RPGPlatformer.UI;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class SpiderWebBarrier : SingleSpawnDefeatableEntity
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
        [SerializeField] SingleSpawnDefeatableEntity motherSpider;

        Health health;
        HealthBarCanvas healthBarCanvas;
        bool healthBarConfigured;

        Rigidbody2D breakPointTopRb;
        Rigidbody2D breakPointBottomRb;

        Collider2D[] contactColliders;
        Collider2D healthCollider;

        bool broken;
        bool snapping;
        float snapTimer;
        Vector2[] initialSnapAnchors;

        protected override void Awake()
        {
            base.Awake();

            health = GetComponent<Health>();
            healthBarCanvas = GetComponentInChildren<HealthBarCanvas>();

            breakPointTopRb = breakPointTop.GetComponent<Rigidbody2D>();
            breakPointBottomRb = breakPointBottom.GetComponent<Rigidbody2D>();

            contactColliders = new Collider2D[joints.Length];

            for (int i = 0; i < joints.Length; i++)
            {
                if (joints[i].TryGetComponent(out Collider2D c))
                {
                    contactColliders[i] = c;
                }
            }

            healthCollider = GetComponent<Collider2D>();

            ConfigureHealth();

            if (motherSpider)
            {
                motherSpider.InitializationComplete += OnMotherSpiderInitialized;
                //^make sure we only set up health after motherSpider has restored state
            }
        }

        private void Update()
        {
            if (snapping)
            {
                snapTimer -= Time.deltaTime;
                if (snapTimer <= 0)
                {
                    snapping = false;
                    //Destroy(gameObject);
                    Defeat();
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

        //protected override void Configure()
        //{
        //    base.Configure();

        //    ConfigureHealth();

        //    if (motherSpider && !motherSpider.Defeated && motherSpider.TryGetComponent(out ICombatController cc))
        //    {
        //        healthCollider.enabled = false;
        //        cc.OnDeath += EnableHealth;
        //    }
        //}

        private void OnMotherSpiderInitialized()
        {
            if (defeated)
            {
                motherSpider.InitializationComplete -= OnMotherSpiderInitialized;
                return;
            }

            //ConfigureHealth();

            if (motherSpider && !motherSpider.Defeated)
            {
                healthCollider.enabled = false;
                motherSpider.OnDefeated += EnableHealth;
            }

            motherSpider.InitializationComplete -= OnMotherSpiderInitialized;
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

        private void EnableHealth()
        {
            healthCollider.enabled = true;
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
                initialSnapAnchors[i] = joints[i].anchor;
                joints[i].autoConfigureConnectedAnchor = false;
                if (contactColliders[i])
                {
                    contactColliders[i].enabled = false;
                }
            }

            broken = true;
            snapping = true;
            snapTimer = snapTime;

            breakPointTopRb.AddForce(breakPointTopRb.mass * topBreakAcceleration, ForceMode2D.Impulse);
            breakPointBottomRb.AddForce(breakPointBottomRb.mass * bottomBreakAcceleration, ForceMode2D.Impulse);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (motherSpider)
            {
                motherSpider.InitializationComplete -= OnMotherSpiderInitialized;
                motherSpider.OnDefeated -= EnableHealth;
            }
        }
    }
}