using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.Core
{
    //note: we will probably be enabling/disabling this component when we enter/exit walk state
    //(so that we can move the legs in other animations, like an attack)
    public class IKLegWalker : MonoBehaviour
    {
        [SerializeField] float stepMin;
        [SerializeField] float stepMax;
        [SerializeField] float reversedStepMin;
        [SerializeField] float reversedStepMax;
        [SerializeField] float initialTimerFraction;
        [SerializeField] float initialPositionFraction;
        [SerializeField] float reversedInitialTimerFraction;
        [SerializeField] float reversedInitialPositionFraction;
        [SerializeField] int stepSmoothingIterations = 5;
        [SerializeField] float stepHeightMultiplier = 1;
        [SerializeField] float stepSpeedMultiplier;
        [SerializeField] float reversedStepSpeedMultiplier;
        [SerializeField] float speedLerpRate;
        [SerializeField] float groundHeightBuffer;
        [SerializeField] float raycastLength;
        [SerializeField] float maintainPositionStrength;
        [SerializeField] Rigidbody2D body;
        [SerializeField] Transform hipJoint;//origin of the raycast
        [SerializeField] Transform ikTarget;

        //to-do: step time should be faster if body is moving faster

        int groundLayer;
        bool stepping;
        bool reversed;
        float smoothedSpeed;
        float stepTimer;
        float currentStepRadius;
        Vector2 currentStepCenter;
        Vector2 currentStepX;
        Vector2 currentStepY;
        Vector2 currentStepGoal;

        Vector2 hipGroundHit;
        Vector2 hipGroundDirection;

        public bool paused;
        public bool steppingDisabled;

        float StepMin => Reversed ? reversedStepMin : stepMin;
        float StepMax => Reversed ? reversedStepMax : stepMax;
        //step max will always be the position right after taking a step
        //step max may be less than step min (when reversing)
        float StepDelta => StepMin - StepMax;
        float StepLength => Mathf.Abs(StepMax - StepMin);
        float InitialTimerFraction 
            => reversed ? reversedInitialTimerFraction : initialTimerFraction;
        float InitialPositionFraction
            => reversed ? reversedInitialPositionFraction : initialPositionFraction;
        float StepSpeedMultiplier => reversed ? reversedStepSpeedMultiplier : stepSpeedMultiplier;

        public bool Reversed
        {
            get => reversed;
            set
            {
                if (value != reversed)
                {
                    reversed = value;
                    //InitializeFootPosition(false);
                    ResetTimerToInitialOffset();
                }
            }
        }

        private void Awake()
        {
            groundLayer = LayerMask.GetMask("Ground");
            //raycastDirection = raycastDirection.normalized;
        }

        private void OnEnable()
        {
            InitializeFootPosition(true);
        }

        private void Start()
        {
            if (body.TryGetComponent(out IEntityOrienter orienter))
            {
                orienter.DirectionChanged += OnDirectionChanged;
            }

            //stepTimer = initialStepPositionFraction * (stepMax - stepMin);

            //currentStepGoal = ikTarget.position;
        }

        private void LateUpdate()
        {
            if (paused) return;

            if (stepping)
            {
                AnimateStep(Time.deltaTime);
            }
            else
            {
                stepTimer += Time.deltaTime * body.linearVelocity.magnitude;

                if (!steppingDisabled && stepTimer > StepLength)
                {
                    UpdateHipGroundData();

                    if (TryFindStepPosition(out var stepPos))
                    {
                        BeginStep(stepPos);
                    }
                }
                else
                {
                    MaintainFootPosition();
                }
            }
        }

        public void InitializeFootPosition(bool snapToPosition)
        {
            ResetTimerToInitialOffset();
            UpdateHipGroundData();
            if (TryFindStepPosition(0, StepMax + InitialPositionFraction * StepDelta, 
                hipGroundDirection, out var s))
            {
                currentStepGoal = s;
                if (snapToPosition)
                {
                    ikTarget.position = s;
                }
            }
        }

        private void ResetTimerToInitialOffset()
        {
            stepping = false;
            stepTimer = InitialTimerFraction * StepLength;
        }


        //IK POSITIONING

        private void BeginStep(Vector3 stepGoal)
        {
            stepGoal = stepGoal + groundHeightBuffer * body.transform.up;
            var stepLength = Vector2.Distance(ikTarget.position, stepGoal);

            currentStepCenter = 0.5f * (ikTarget.position + stepGoal);
            currentStepRadius = 0.5f * stepLength;
            currentStepX = (stepGoal - ikTarget.position) / stepLength;
            currentStepY = Mathf.Sign(stepGoal.x - ikTarget.position.x) * currentStepX.CCWPerp();
            currentStepGoal = stepGoal;

            stepTimer = 0;
            stepping = true;


        }

        private void EndStep()
        {
            stepping = false;
            stepTimer = 0;
            //ikTarget.position = currentStepGoal;

        }

        private void AnimateStep(float dt)
        {
            smoothedSpeed = Mathf.Lerp(smoothedSpeed, body.linearVelocity.magnitude, dt * speedLerpRate);
            stepTimer += dt * smoothedSpeed;
            var t = stepTimer * Mathf.PI * StepSpeedMultiplier;

            if (t > Mathf.PI)
            {
                EndStep();
            }
            else
            {
                ikTarget.position = currentStepCenter - currentStepRadius * Mathf.Cos(t) * currentStepX
                + Mathf.Clamp(smoothedSpeed, 0, 1) * stepHeightMultiplier 
                * currentStepRadius * Mathf.Sin(t) * currentStepY;
                //^multiplying the y by s lets the leg rest at a natural position when body comes to a stop
                //using the smoothedSpeed just for this bc it got a little jittery at slow speed
                //(when speed is less steady)
            }
        }

        private void MaintainFootPosition()
        {
            ikTarget.position = Vector2.Lerp(ikTarget.position, currentStepGoal,
                maintainPositionStrength * smoothedSpeed * Time.deltaTime);
        }

        private void OnDirectionChanged(HorizontalOrientation o)
        {
            var d = currentStepGoal - (Vector2)body.transform.position;
            currentStepGoal = body.transform.position 
                + PhysicsTools.ReflectAcrossPerpendicularHyperplane(body.transform.right, d);
            d = currentStepCenter - (Vector2)body.transform.position;
            currentStepCenter = body.transform.position 
                + PhysicsTools.ReflectAcrossPerpendicularHyperplane(body.transform.right, d);
            currentStepX = PhysicsTools.ReflectAcrossPerpendicularHyperplane(body.transform.right, currentStepX);
            currentStepY = PhysicsTools.ReflectAcrossPerpendicularHyperplane(body.transform.right, currentStepY);
        }


        //DETERMINING STEP POSITION

        private bool TryFindStepPosition(out Vector2 stepPosition)
        {
            return TryFindStepPosition(0, StepMax, hipGroundDirection, out stepPosition);
        }

        private bool TryFindStepPosition(int iteration, float goalOffset, 
            Vector2 searchDirection, out Vector2 stepPosition)
        {
            iteration++;

            var hit = Physics2D.Raycast((Vector2)hipJoint.position + goalOffset * searchDirection, 
                -body.transform.up, raycastLength, groundLayer);

            if (!hit)
            {
                stepPosition = default;
                return false;
            }

            if (iteration < stepSmoothingIterations
                && Vector2.SqrMagnitude(hit.point - hipGroundHit) > StepMax * StepMax)
            {
                searchDirection = (hit.point - hipGroundHit).normalized;
                if (!TryFindStepPosition(iteration, goalOffset, searchDirection, out stepPosition))
                {
                    stepPosition = hit.point;
                }

                return true;
            }
            else
            {
                stepPosition = hit.point;
                return true;
            }
        }

        private void UpdateHipGroundData()
        {
            var hit0 = Physics2D.Raycast(hipJoint.position, -body.transform.up, raycastLength, groundLayer);
            var hit1 = Physics2D.Raycast(hipJoint.position + 0.1f * body.transform.localScale.x * body.transform.right,
                -body.transform.up, raycastLength, groundLayer);

            if (hit0)
            {
                hipGroundHit = hit0.point;

                if (hit1)
                {
                    hipGroundDirection = (hit1.point - hit0.point).normalized;
                }
            }
        }
    }
}