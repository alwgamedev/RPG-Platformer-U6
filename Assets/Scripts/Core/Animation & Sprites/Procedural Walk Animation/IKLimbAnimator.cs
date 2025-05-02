using Cinemachine.Utility;
using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public enum LimbAnimatorMode
    {
        walking, trackingTransform, trackingPosition
    }

    public class IKLimbAnimator : MonoBehaviour
    {
        [SerializeField] Transform trackingGuide;
        [SerializeField] float trackingLerpRate;//how quickly you should move towards goal tracking position
        [SerializeField] float trackingStrength;//btwn 0 - 1, how closely you want to track target 
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
        [SerializeField] float baseStepSinkRate;
        [SerializeField] float groundHeightBuffer;
        [SerializeField] float raycastLength;
        [SerializeField] float maintainPositionSpeedScale;
        [SerializeField] float baseMaintainPositionStrength;
        [SerializeField] Rigidbody2D body;
        [SerializeField] Transform hipJoint;
        [SerializeField] Transform ikTarget;

        int groundLayer;

        Transform trackedTransform;
        Vector3 trackedPosition;
        Vector3 defaultPosition;
        //while tracking, you will interpolate between defaultPosition and target position based on trackingStrength

        //walk animation parameters
        bool stepCancelled;
        bool stepping;
        float smoothedSpeed;
        float stepTimer;
        float currentStepRadius;
        Vector2 currentStepCenter;
        Vector2 currentStepX;
        //Vector2 currentStepY;
        Vector2 currentStepGoal;
        Vector2 hipGroundHit;
        Vector2 hipGroundDirection;

        public bool animationDisabled;//timer still running, but doesn't animate anything
        public bool steppingDisabled;

        float StepMin => Reversed ? reversedStepMin : stepMin;
        float StepMax => Reversed ? reversedStepMax : stepMax;
        //step max is the position right after taking a step
        //step max may be less than step min (e.g. when reversing)
        float StepDelta => StepMin - StepMax;
        float StepLength => Mathf.Abs(StepDelta);
        float InitialTimerFraction 
            => Reversed ? reversedInitialTimerFraction : initialTimerFraction;
        float InitialPositionFraction
            => Reversed ? reversedInitialPositionFraction : initialPositionFraction;
        float InitialPositionOffset => StepMax + InitialPositionFraction * StepDelta;
        float StepSpeedMultiplier => Reversed ? reversedStepSpeedMultiplier : stepSpeedMultiplier;

        public LimbAnimatorMode AnimationMode { get; private set; } = LimbAnimatorMode.walking;
        public bool Reversed { get; private set; }

        private void Awake()
        {
            groundLayer = LayerMask.GetMask("Ground");
        }

        private void OnEnable()
        {
            ResetWalkAnimation(true);
        }

        private void Start()
        {
            if (body.TryGetComponent(out IEntityOrienter orienter))
            {
                orienter.DirectionChanged += OnDirectionChanged;
            }
        }

        private void LateUpdate()
        {
            if (animationDisabled) return;

            switch (AnimationMode)
            {
                case LimbAnimatorMode.walking:
                    UpdateWalkAnimation();
                    break;
                case LimbAnimatorMode.trackingTransform:
                    UpdateTracking(trackedTransform);
                    break;
                case LimbAnimatorMode.trackingPosition:
                    UpdateTracking(trackedPosition);
                    break;
            }
        }

        public void UpdateTimer(float smoothedSpeed)
        {
            this.smoothedSpeed = smoothedSpeed;
            stepTimer += Time.deltaTime * smoothedSpeed;
        }


        public void SetReversed(bool val)
        {
            if (val == Reversed) return;

            Reversed = val;
            ResetWalkAnimation(false);
        }

        
        //TARGET TRACKING

        public void BeginTrackingGuide()
        {
            BeginTrackingTarget(trackingGuide);
        }

        public void BeginTrackingTarget(Transform target)
        {
            //var r = Physics2D.Raycast(ikTarget.position + body.transform.up, -body.transform.up, 
            //    raycastLength, groundLayer);
            if (!TryFindStepPosition(0, InitialPositionOffset, hipGroundDirection, out var p))
            {
                p = currentStepGoal;
            }
            BeginTrackingTarget(target, p/*r ? r.point : currentStepGoal*/);
        }

        public void BeginTrackingPosition(Vector3 position)
        {
            //var r = Physics2D.Raycast(ikTarget.position + body.transform.up, -body.transform.up, 
            //   raycastLength, groundLayer);
            if (!TryFindStepPosition(0, InitialPositionOffset, hipGroundDirection, out var p))
            {
                p = currentStepGoal;
            }
            BeginTrackingPosition(position, p/*r ? r.point : currentStepGoal*/);
        }

        public void BeginTrackingTarget(Transform target, Vector3 defaultPosition)
        {
            trackedTransform = target;
            this.defaultPosition = defaultPosition;
            AnimationMode = LimbAnimatorMode.trackingTransform;
        }

        public void BeginTrackingPosition(Vector3 position, Vector3 defaultPosition)
        {
            trackedPosition = position;
            this.defaultPosition = defaultPosition;
            AnimationMode = LimbAnimatorMode.trackingPosition;
        }

        public void EndTracking()
        {
            //if you want to return to walking state without restarting the walk anim.;
            AnimationMode = LimbAnimatorMode.walking;
            //currentStepGoal = defaultPosition;
            if (stepping)
            {
                stepCancelled = true;
            }
            else
            {
                currentStepGoal = defaultPosition;
            }
        }

        private void UpdateTracking(Transform target)
        {
            if (target)
            {
                UpdateTracking(target.position);
            }
        }

        private void UpdateTracking(Vector3 targetPosition)
        {
            var g = Vector3.LerpUnclamped(defaultPosition, targetPosition, trackingStrength);
            ikTarget.position = Vector3.Lerp(ikTarget.position, g, trackingLerpRate * Time.deltaTime);
        }


        //WALK ANIMATION

        private void UpdateWalkAnimation()
        {
            if (stepping)
            {
                AnimateStep();
            }
            else
            {
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

        public void ResetWalkAnimation(bool snapToPosition)
        {
            ResetTimerToInitialOffset();
            UpdateHipGroundData();
            if (AnimationMode == LimbAnimatorMode.walking
                && TryFindStepPosition(0, InitialPositionOffset, 
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

        private void StepTimerModEqStepLength()
        {
            while (stepTimer >= StepLength && StepLength > 0)
            {
                stepTimer -= StepLength;
            }
        }

        private void BeginStep(Vector3 stepGoal)
        {
            stepCancelled = false;
            stepGoal = stepGoal + groundHeightBuffer * body.transform.up;
            var stepDistance = Vector2.Distance(ikTarget.position, stepGoal);

            currentStepCenter = 0.5f * (ikTarget.position + stepGoal);
            currentStepRadius = 0.5f * stepDistance;
            currentStepX = (stepGoal - ikTarget.position) / stepDistance;
            //currentStepY = Mathf.Sign(stepGoal.x - ikTarget.position.x) * currentStepX.CCWPerp();
            currentStepGoal = stepGoal;

            StepTimerModEqStepLength();
            stepping = true;
        }

        private void EndStep()
        {
            stepping = false;
            stepTimer = 0;//just set = 0 here, bc we don't know the time to animate step circle
            //(only know the time to drag back)

        }

        private void AnimateStep()
        {
            var t = stepTimer * Mathf.PI * StepSpeedMultiplier;

            if (t > Mathf.PI)
            {
                EndStep();
            }
            else if (!stepCancelled)
            {

                ikTarget.position = currentStepCenter - currentStepRadius * Mathf.Cos(t) * currentStepX
                + Mathf.Clamp(smoothedSpeed - baseStepSinkRate * Time.deltaTime, 0, 1) * stepHeightMultiplier
                * currentStepRadius * Mathf.Sin(t) * (Vector2)body.transform.up;//currentStepY;
                //if (Vector2.Dot(ikTarget.position - hipJoint.position, body.transform.up)
                //    > smoothedSpeed + Vector2.Dot(currentStepGoal - (Vector2)hipJoint.position, body.transform.up))
                //{
                //    ikTarget.position -= baseStepSinkRate * Time.deltaTime * body.transform.up;
                //}

                //^multiplying the y by s lets the leg rest at a natural position when body comes to a stop
            }
            //else
            //{
            //    MaintainFootPosition();
            //}
        }

        private void MaintainFootPosition()
        {
            ikTarget.position = Vector2.Lerp(ikTarget.position, currentStepGoal,
                (baseMaintainPositionStrength + maintainPositionSpeedScale * smoothedSpeed) * Time.deltaTime);
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
            //currentStepY = PhysicsTools.ReflectAcrossPerpendicularHyperplane(body.transform.right, currentStepY);
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
                searchDirection = Mathf.Sign(goalOffset) * (hit.point - hipGroundHit).normalized;
                if (!TryFindStepPosition(iteration, goalOffset, searchDirection, 
                    out stepPosition))
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