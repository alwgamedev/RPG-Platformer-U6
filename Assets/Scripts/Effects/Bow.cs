using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using RPGPlatformer.Movement;

namespace RPGPlatformer.Effects
{
    public class Bow : MonoBehaviour
    {
        [SerializeField] Transform topString;
        [SerializeField] Transform bottomString;
        [SerializeField] Transform topStringEndPt;
        [SerializeField] Transform bottomStringEndPt;
        [SerializeField] Transform stringGrabPt;
        [SerializeField] Transform stringGrabPtOrigin;
        [SerializeField] Transform stringGrabPtParent;
        [SerializeField] float stringVibrationTimeOut = 3;
        [SerializeField] float stringVibrationSpeed = 10;
        [Range(0,1)][SerializeField] float stringBounceBackFraction = 0.5f;
        [Range(0,1)][SerializeField] float stringDistanceTolerance = .1f;

        HorizontalOrientation currentOrientation;

        Action OnUpdate;

        private void Awake()
        {
            ReturnToOrigin();
        }

        private void OnEnable()
        {
            IMovementController movementController = GetComponentInParent<IMovementController>();
            if (movementController != null)
            {
                currentOrientation = movementController.CurrentOrientation;
                movementController.Mover.UpdatedXScale += (value) => currentOrientation = value;
            }
        }

        private void Update()
        {
            if (stringGrabPt.hasChanged)
            {
                StretchStrings();
            }

            OnUpdate?.Invoke();
        }

        public void BeginPull(Transform puller)
        {
            StopAllCoroutines();
            ReturnToOrigin();
            stringGrabPt.SetParent(puller);
            stringGrabPt.localPosition = Vector3.zero;
        }

        public void ReleasePull()
        {
            stringGrabPt.SetParent(stringGrabPtParent, true);
            StartCoroutine(ReleaseBowString(stringVibrationSpeed, stringDistanceTolerance, stringBounceBackFraction));
        }

        public void BowReset()
        {
            StopAllCoroutines();
            ReturnToOrigin();
        }

        private void ReturnToOrigin()
        {
            stringGrabPt.position = stringGrabPtOrigin.position;
        }

        private IEnumerator ReleaseBowString(float speed, float tolerance, float bounceBackFraction)
        {
            float timer = 0;
            OnUpdate += () => timer += Time.deltaTime;
            while(Vector3.Distance(stringGrabPt.position, stringGrabPtOrigin.position) > tolerance && timer < stringVibrationTimeOut)
            {
                yield return MoveToPoint(stringGrabPt, stringGrabPtOrigin, speed, bounceBackFraction);
            }
            OnUpdate -= () => timer += Time.deltaTime;
            ReturnToOrigin();
        }

        private IEnumerator MoveToPoint(Transform movingObject, Transform target, float speed, float overShootFraction)
        {
            if (movingObject.position == target.position) 
            {
                yield break;
            }

            float distanceToOrigin = Vector3.Distance(movingObject.position, target.position);
            float distanceToTravel = distanceToOrigin * (1 + overShootFraction);
            float distanceTravelled = 0;
            bool passedTarget = false;

            while(distanceTravelled < distanceToTravel)
            {
                float delta = speed * Time.deltaTime;
                Vector3 axis = (target.position - movingObject.position).normalized;
                if(passedTarget)
                {
                    axis *= -1;
                }

                movingObject.position += delta * axis;
                distanceTravelled += delta;
                if(distanceTravelled >= distanceToOrigin)
                {
                    passedTarget = true;
                }

                yield return null;
            }
        }

        private void StretchStrings()
        {
            Vector3 topGoal = TopStringGoalVector();
            Vector3 bottomGoal = BottomStringGoalVector();

            float topAngle = Mathf.Atan2(topGoal.y, topGoal.x);
            float bottomAngle = Mathf.Atan2(bottomGoal.y, bottomGoal.x);

            if(currentOrientation == HorizontalOrientation.left)
            {
                topAngle = topAngle - Mathf.PI;
                bottomAngle = bottomAngle - Mathf.PI;
            }

            topString.rotation = quaternion.RotateZ(topAngle);
            bottomString.rotation = quaternion.RotateZ(bottomAngle);

            UpdateStringLengths(topGoal.magnitude, bottomGoal.magnitude);
        }

        private void UpdateStringLengths(float topGoalLength, float bottomGoalLength)
        {
            float topStringLength = TopStringLength();
            float bottomStringLength = BottomStringLength();
            if (topStringLength == 0 || bottomStringLength == 0) return;

            float topScaleFactor = topGoalLength / topStringLength;
            float bottomScaleFactor = bottomGoalLength / bottomStringLength;

            Vector3 topScale = topString.localScale;
            Vector3 bottomScale = bottomString.localScale;

            topString.localScale = (topScaleFactor * topScale.x) * Vector3.right + topScale.y * Vector3.up
                + topScale.z * Vector3.forward;
            bottomString.localScale = (bottomScaleFactor * bottomScale.x) * Vector3.right + bottomScale.y * Vector3.up 
                + bottomScale.z * Vector3.forward;

        }

        float TopStringLength() => Vector2.Distance(topString.position, topStringEndPt.position);

        float BottomStringLength() => Vector2.Distance(bottomString.position, bottomStringEndPt.position);

        Vector3 TopStringGoalVector() => stringGrabPt.position - topString.position;

        Vector3 BottomStringGoalVector() => stringGrabPt.position - bottomString.position;

        private void OnDisable()
        {
            BowReset();
        }
    }
}
