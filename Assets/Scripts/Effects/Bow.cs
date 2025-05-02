using System;
using UnityEngine;

namespace RPGPlatformer.Effects
{
    public class Bow : MonoBehaviour
    {
        [SerializeField] Transform bottomStringAnchor;
        [SerializeField] Transform topStringAnchor;
        [SerializeField] Transform stringGrabPt;
        [SerializeField] Transform stringGrabPtOrigin;
        [SerializeField] Transform stringGrabPtParent;
        [SerializeField] float bowStringWidth = 0.25f;
        [SerializeField] float stringFrequencyMult = 15;
        [SerializeField] float stringElasticity = 0.05f;//higher means string will return to rest faster

        LineRenderer lineRenderer;
        Vector3[] bowstringPoints = new Vector3[3];

        Vector2 u; //displacement of stringGrabPt from origin at moment of release, normalized
        float d; //distance of stringGrabPt from origin at moment of release
        float t; //timeSinceRelease

        Action OnUpdate;

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            var width = bowStringWidth * Mathf.Abs(transform.lossyScale.x);
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            //the bow prefab is only instantiated when the bow weapon is equipped, so in Start it will already
            //be childed to the bow weilder

            ReturnToOrigin();
            RedrawBowstring();
        }

        private void Update()
        {
            OnUpdate?.Invoke();

            if (stringGrabPt.hasChanged)
            {
                RedrawBowstring();
            }
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
            //should be done in appropriate local space?? take another look at this
            //probably should use u' = u.x * transform.right + u.y * transorm.up;
            //(and when u is originally set you should take its components in local coord system too)
        }

        private void RedrawBowstring()
        {
            lineRenderer.widthMultiplier = bowStringWidth * Math.Abs(transform.lossyScale.x);

            bowstringPoints[0] = bottomStringAnchor.position;
            bowstringPoints[1] = stringGrabPt.position;
            bowstringPoints[2] = topStringAnchor.position;

            lineRenderer.SetPositions(bowstringPoints);
        }

        private void OnDisable()
        {
            BowReset();
        }
    }
}
