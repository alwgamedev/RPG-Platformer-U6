using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.Core
{
    //note: we will probably be enabling/disabling this component when we enter/exit walk state
    //(so that we can move the legs in other animations, like an attack)
    public class IKLegWalker : MonoBehaviour
    {
        [SerializeField] float stepLength;
        [SerializeField] float stepTime;//the time it should take to complete a step of length stepLength;
        [SerializeField] Transform body;
        [SerializeField] Transform hipJoint;//origin of the raycast
        [SerializeField] Transform ikTarget;

        int groundLayer;
        bool stepping;
        float stepTimer;
        float currentStepSpeed;//angular speed in radians per second
        float currentStepRadius;
        Vector2 currentStepCenter;
        Vector2 currentStepX;
        Vector2 currentStepY;

        private void Awake()
        {
            groundLayer = LayerMask.GetMask("Ground");
        }

        private void Update()
        {
            if (stepping)
            {
                AnimateStep(Time.deltaTime);
            }
            else if (ShouldStep(out var stepLength))
            {
                BeginStep(stepLength);
            }
        }

        private bool ShouldStep(out Vector3 stepGoal)
        {
            var hit = Physics2D.Raycast(hipJoint.position, -body.up, groundLayer);
            if (!hit)
            {
                stepGoal = default;
                return false;
            }

            stepGoal = hit.point;
            return (stepGoal - ikTarget.position).sqrMagnitude > stepLength * stepLength;
        }

        private void BeginStep(Vector3 stepGoal)
        {
            var stepLength = Vector2.Distance(ikTarget.position, stepGoal);

            currentStepCenter = 0.5f * (ikTarget.position + stepGoal);
            currentStepRadius = 0.5f * stepLength;
            currentStepX = (stepGoal - ikTarget.position) / stepLength;
            currentStepY = Mathf.Sign(stepGoal.x - ikTarget.position.x) * currentStepX.CCWPerp();
            currentStepSpeed = Mathf.PI * this.stepLength / (stepTime * stepLength);
                //pi / desired step duration (bc step speed is angular speed in rad/sec)

            stepTimer = 0;
            stepping = true;


        }

        private void EndStep()
        {
            stepping = false;
            ikTarget.position = currentStepCenter + currentStepRadius * currentStepX;
        }

        private void AnimateStep(float dt)
        {
            stepTimer += dt;
            var t = stepTimer * currentStepSpeed;

            if (t > Mathf.PI)
            {
                EndStep();
            }
            else
            {
                ikTarget.position = currentStepCenter - currentStepRadius * Mathf.Cos(t) * currentStepX
                + currentStepRadius * Mathf.Sin(t) * currentStepY;
            }
        }
    }
}