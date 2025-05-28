using UnityEngine;
using RPGPlatformer.Movement;
using RPGPlatformer.Core;
using System.Threading.Tasks;
using RPGPlatformer.Combat;
using RPGPlatformer.Effects;

namespace RPGPlatformer.AIControl
{
    public class ElectricEel : MonoBehaviour, IDamageDealer
    {
        [SerializeField] float vertexSpacing = .25f;
        [SerializeField] float turnSpeed = 2;
        [SerializeField] float changeDirectionThreshold = -0.125f;
        [SerializeField] float turnOppositeThreshold = 0.25f;
        [SerializeField] float boundsBuffer = 0.25f;
        //^how far outside movementBounds vertices are allowed to go
        //(so destination will always be chosen within movementBounds, but to make sure
        //he has room to flex a little around the destination, the movementBounds will be a bit inset
        //from where he is really allowed to go)
        [SerializeField] float destinationToleranceSqrd = .01f;
        [SerializeField] float baseMoveSpeed = 1.5f;
        [SerializeField] float pursuitMoveSpeed = 3;
        [SerializeField] float acceleration = 1;
        [SerializeField] EelVertex[] vertices;
        [SerializeField] RandomizableVector2 movementBounds;
        [SerializeField] float shockForce;
        [SerializeField] float shockDamage;
        [SerializeField] int shockBleedHits;
        [SerializeField] float shockBleedRate;

        LineRenderer lineRenderer;
        Vector2 moveDirection;
        Vector2 currentDestination;
        //float pursuitCooldownTimer;
        Vector3 playerHeightOffset;
        float currentMoveSpeed;

        //choosing destinations a bit inset from bounds helps him avoid making tight turns near the 
        //edges of bounds
        //(we could give him logic to avoid making those tight turns (turning in the opposite direction when 
        //too close to an edge) but this is an easier fix)
        //(can still get forced into making tight turns when pursuing player, but that would only really
        //arise when player moves over top of him to opposite side of him when near top of water,
        //but player will most likely exit water during that, which will trigger him to choose a new destination)
        Vector2 SoftMax => 0.1f * movementBounds.Min + 0.9f * movementBounds.Max;
        Vector2 SoftMin => 0.9f * movementBounds.Min + 0.1f * movementBounds.Max;
        Vector2 HeadUp => (int)CurrentOrientation * moveDirection.CCWPerp();

        HorizontalOrientation CurrentOrientation => (HorizontalOrientation)Mathf.Sign(-lineRenderer.textureScale.y);
        public CombatStyle CurrentCombatStyle => CombatStyle.Unarmed;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            currentMoveSpeed = baseMoveSpeed;
        }

        private void Start()
        {
            ConfigureVertices();
            InitializeWiggle();
            currentDestination = movementBounds.Value;
            var h = GlobalGameTools.Instance.PlayerMover.Mover.Height;
            playerHeightOffset = -0.35f * h * Vector3.up;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                PlayBodyParticles();
            }

            //if (pursuitCooldownTimer > 0)
            //{
            //    pursuitCooldownTimer -= Time.deltaTime;
            //}

            if (PlayerInBounds() /*&& pursuitCooldownTimer <= 0*/)
            {
                currentDestination = PlayerTargetPosition();
                if (currentMoveSpeed != pursuitMoveSpeed)
                {
                    currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, pursuitMoveSpeed, acceleration * Time.deltaTime);
                }
            }
            else if (currentMoveSpeed != baseMoveSpeed)
            {
                currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, baseMoveSpeed, acceleration * Time.deltaTime);
            }
            LerpMoveDirection(currentDestination);
            UpdateMovement();
            UpdateWiggle();
            EnforceBounds();
            if (HasReachedDestination(currentDestination))
            {
                currentDestination = NewDestination();
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
            //if (pursuitCooldownTimer <= 0)
            //{
            //    pursuitCooldownTimer = pursuitCooldown.Value;
            //    currentDestination = NewDestination();
            //}
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
            bool turnOppositeNearMax = u.x * (int)CurrentOrientation < 0
                && vertices[0].transform.position.y > movementBounds.Max.y - turnOppositeThreshold
                && Vector2.Dot(u, HeadUp) > 0;
            if (turnOppositeNearMax)
            {
                moveDirection = Vector2.Lerp(moveDirection, -HeadUp, Time.deltaTime * turnSpeed).normalized;
                vertices[0].VCGP.SetTangentDir(moveDirection);
                return;
            }

            bool turnOppositeNearMin = u.x * (int)CurrentOrientation < 0
                && vertices[0].transform.position.y < movementBounds.Min.y + turnOppositeThreshold
                && Vector2.Dot(u, HeadUp) < 0;

            if (turnOppositeNearMin)
            {
                moveDirection = Vector2.Lerp(moveDirection, HeadUp, Time.deltaTime * turnSpeed).normalized;
                vertices[0].VCGP.SetTangentDir(moveDirection);
                return;
            }

            moveDirection = Vector2.Lerp(moveDirection, u, Time.deltaTime * turnSpeed).normalized;
            vertices[0].VCGP.SetTangentDir(moveDirection);
            //or we could just have vertices[0] tang direction be 0 always
        }

        private void UpdateMovement()
        {
            Vector3 u;
            vertices[0].transform.position += Time.deltaTime * currentMoveSpeed * (Vector3)moveDirection;
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

        private Vector2 NewDestination()
        {
            return MiscTools.RandomPointInRectangle(SoftMin, SoftMax);
        }

        private bool HasReachedDestination(Vector2 destination)
        {
            return Vector2.SqrMagnitude((Vector2)vertices[0].transform.position - destination)
                < destinationToleranceSqrd;
        }

        private bool PlayerInBounds()
        {
            if (GlobalGameTools.Instance.PlayerIsDead) return false;
            var p = PlayerTargetPosition();
            return p.x > movementBounds.Min.x - boundsBuffer 
                && p.x < movementBounds.Max.x + boundsBuffer
                && p.y > movementBounds.Min.y - boundsBuffer 
                && p.y < movementBounds.Max.y + boundsBuffer;
        }

        private Vector2 PlayerTargetPosition()
        {
            return GlobalGameTools.Instance.PlayerTransform.position + playerHeightOffset;
        }

        private void EnforceBounds()
        {
            foreach (var v in vertices)
            {
                v.EnforceBounds(movementBounds, boundsBuffer);
            }
        }
    }
}