using UnityEngine;
using RPGPlatformer.Movement;
using RPGPlatformer.Core;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using System.Threading;
using RPGPlatformer.Combat;
using RPGPlatformer.Effects;

namespace RPGPlatformer.AIControl
{
    public class GiantEel : MonoBehaviour, IDamageDealer
    {
        [SerializeField] float vertexSpacing = .25f;
        [SerializeField] float turnSpeed = 2;
        [SerializeField] float changeDirectionThreshold = -0.125f;
        [SerializeField] float destinationToleranceSqrd = .01f;
        [SerializeField] float moveSpeed;
        [SerializeField] EelVertex[] vertices;
        [SerializeField] RandomizableVector2 movementBounds;
        [SerializeField] float shockForce;
        [SerializeField] float shockDamage;
        [SerializeField] int shockBleedHits;
        [SerializeField] float shockBleedRate;

        LineRenderer lineRenderer;
        Vector2 moveDirection;
        Vector2 currentDestination;

        HorizontalOrientation CurrentOrientation => (HorizontalOrientation)Mathf.Sign(-lineRenderer.textureScale.y);
        public CombatStyle CurrentCombatStyle => CombatStyle.Unarmed;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        private void Start()
        {
            ConfigureVertices();
            InitializeWiggle();
            currentDestination = movementBounds.Value;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                PlayBodyParticles();
            }

            UpdateWiggle();
            LerpMoveDirection(currentDestination);
            UpdateMovement();
            if (HasReachedDestination(currentDestination))
            {
                currentDestination = movementBounds.Value;
            }
        }


        //SETUP

        private void ConfigureVertices()
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                var leader = i > 0 ? vertices[i - 1] : null;
                var follower = i < vertices.Length - 1 ? vertices[i + 1] : null;
                vertices[i].Configure(leader, follower, vertexSpacing);
            }

            foreach (var v in vertices)
            {
                v.ShockTrigger += OnShockTrigger;
            }
        }


        //ANIMATION & EFFECTS

        private void InitializeWiggle()
        {
            for (int i = 1; i < vertices.Length; i++)
            {
                var d = 2 * ((i + 1) / 2 % 2) - 1;
                vertices[i].InitializeWiggle(d, d * (i % 2));
            }
        }

        private void UpdateWiggle()
        {
            for (int i = 1; i < vertices.Length; i++)
            {
                vertices[i].UpdateWiggle(CurrentOrientation, Time.deltaTime);
            }
        }

        private void PlayBodyParticles()
        {
            foreach (var v in vertices)
            {
                if (v.ParticleSystem)
                {
                    v.ParticleSystem.Play();
                }
            }
        }

        private async void OnShockTrigger(Vector2 collisionNormal)
        {
            await ShockTask(collisionNormal);
        }

        private async Task ShockTask(Vector2 collisionNormal)
        {
            var p = GlobalGameTools.Instance.Player;
            var pRb = ((Mover)((IMovementController)p.MovementController).Mover).Rigidbody;
            PlayBodyParticles();
            pRb.AddForce(-pRb.mass * shockForce * collisionNormal, ForceMode2D.Impulse);
            await AttackAbility.Bleed(this, p.Combatant.Health, shockDamage,
                shockBleedHits, shockBleedRate, null, ShockHitEffect);
        }

        private PoolableEffect ShockHitEffect()
        {
            if (SceneResourcesPooler.Instance)
            {
                return (PoolableEffect)SceneResourcesPooler.Instance.EffectPooler.GetObject("Shock Hit Particles");
            }

            return null;
        }


        //MOVEMENT CONTROL

        private void LerpMoveDirection(Vector2 destination)
        {
            Vector2 u = (destination - (Vector2)vertices[0].transform.position).normalized;
            moveDirection = Vector2.Lerp(moveDirection, u, Time.deltaTime * turnSpeed).normalized;
            vertices[0].VCGP.SetTangentDir(moveDirection);
            //or we could just have vertices[0] tang direction be 0 always
        }

        private void UpdateMovement()
        {
            Vector3 u;
            vertices[0].transform.position += Time.deltaTime * moveSpeed * (Vector3)moveDirection;
            for (int i = 1; i < vertices.Length; i++)
            {
                u = (vertices[i].transform.position - vertices[i - 1].transform.position).normalized;
                vertices[i].transform.position = vertices[i - 1].transform.position + vertexSpacing * u;
            }

            var d = vertices[0].transform.position.x - vertices[1].transform.position.x;
            if (d * (int)CurrentOrientation < changeDirectionThreshold)
            {
                ChangeOrientation();
            }

            foreach (var v in vertices)
            {
                v.UpdateParticleSystemRotation();
            }
        }

        private void ChangeOrientation()
        {
            var s = lineRenderer.textureScale;
            s.y *= -1;
            lineRenderer.textureScale = s;
        }

        private bool HasReachedDestination(Vector2 destination)
        {
            return Vector2.SqrMagnitude((Vector2)vertices[0].transform.position - destination)
                < destinationToleranceSqrd;
        }
    }
}