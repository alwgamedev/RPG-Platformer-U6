using System;
using UnityEngine;
using RPGPlatformer.Movement;

namespace RPGPlatformer.Effects
{
    using static MovementTools;

    public class Bow : MonoBehaviour
    {
        [SerializeField] Transform topString;
        [SerializeField] Transform bottomString;
        [SerializeField] Transform topStringEndPt;
        [SerializeField] Transform bottomStringEndPt;
        [SerializeField] Transform stringGrabPt;
        [SerializeField] Transform stringGrabPtOrigin;
        [SerializeField] Transform stringGrabPtParent;
        [SerializeField] float stringFrequencyMult = 15;
        [SerializeField] float stringElasticity = 0.05f;//higher means string will return to rest faster

        HorizontalOrientation parentOrientation;

        Vector2 u; //displacement of stringGrabPt from origin at moment of release, normalized
        float d; //distance of stringGrabPt from origin at momvent of release
        float t; //timeSinceRelease

        Action OnUpdate;

        private void Start()
        {
            ReturnToOrigin();

            IMovementController movementController = GetComponentInParent<IMovementController>();

            if (movementController != null)
            {
                parentOrientation = movementController.CurrentOrientation;
                movementController.Mover.DirectionChanged += SetOrientation;
            }
        }

        private void Update()
        {
            OnUpdate?.Invoke();

            if (stringGrabPt.hasChanged)
            {
                StretchStrings();
            }
        }

        public void SetOrientation(HorizontalOrientation orientation)
        {
            parentOrientation = orientation;
        }

        public void BeginPull(Transform puller)
        {
            BowReset();
            stringGrabPt.SetParent(puller);
            stringGrabPt.localPosition = Vector3.zero;
        }

        public void ReleasePull()
        {
            stringGrabPt.SetParent(stringGrabPtParent, true);

            var v = stringGrabPtOrigin.position - stringGrabPt.position;
            d = v.magnitude;
            u = v / d;
            t = 0;

            OnUpdate = UpdateStringVibration;
        }

        public void BowReset()
        {
            OnUpdate = null;
            ReturnToOrigin();
        }

        private void ReturnToOrigin()
        {
            stringGrabPt.position = stringGrabPtOrigin.position;
        }

        private void UpdateStringVibration()
        {
            t += Time.deltaTime;

            var a = d - stringElasticity * t;

            if (a < Mathf.Epsilon)
            {
                BowReset();
                return;
            }

            Vector3 delta = a * Mathf.Cos(stringFrequencyMult * t) * u;
            stringGrabPt.position = stringGrabPtOrigin.position + delta;
        }

        private void StretchStrings()
        {
            Vector2 topGoal = TopStringGoalVector();
            Vector2 bottomGoal = BottomStringGoalVector();

            topString.rotation = Quaternion.LookRotation(Vector3.forward, 
                (int) parentOrientation * topGoal.CCWPerp());
            bottomString.rotation = Quaternion.LookRotation(Vector3.forward, 
                (int) parentOrientation * bottomGoal.CCWPerp());

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

        Vector2 TopStringGoalVector() => stringGrabPt.position - topString.position;

        Vector2 BottomStringGoalVector() => stringGrabPt.position - bottomString.position;

        private void OnDisable()
        {
            BowReset();
        }
    }
}
