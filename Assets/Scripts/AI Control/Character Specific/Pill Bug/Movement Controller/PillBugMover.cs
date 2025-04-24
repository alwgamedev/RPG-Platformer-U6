using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class PillBugMover : ChainedMover
    {
        [SerializeField] WheelAxle axle;
        [SerializeField] float wheelAngleRange;//rad -- range of angle over which the bodypieces will be spread out
        [SerializeField] float radiusMultiplier = 1;
        [SerializeField] float defaultBodyLinearDamping = 5;
        [SerializeField] float rollingBodyLinearDamping = 100;
        [SerializeField] protected PhysicsMaterial2D defaultMaterial;
        [SerializeField] protected PhysicsMaterial2D rollingMaterial;

        bool curled;
        HingeJoint2D[] wheelHinges;

        float wheelRadius => radiusMultiplier * (NumBodyPieces / (float)(NumBodyPieces - 1)) * Width / (2 * Mathf.PI);
        float dTheta => wheelAngleRange / NumBodyPieces;

        public bool Curled => curled;
        public WheelAxle Axle => axle;

        private void Start()
        {
            wheelHinges = new HingeJoint2D[NumBodyPieces];

            for (int i = 0; i < NumBodyPieces; i++)
            {
                wheelHinges[i] = bodyPieces[i].gameObject.GetComponent<HingeJoint2D>();
            }

            CurrentOrientation = HorizontalOrientation.right;
            SetCurled(false);
        }

        protected override void UpdatePhysicsData()
        {
            if (!curled)
            {
                base.UpdatePhysicsData();
            }
            else
            {
                UpdateRollingRotations();
            }
        }

        private void UpdateRollingRotations()
        {
            for (int i = 0; i < NumBodyPieces; i++)
            {
                bodyPieces[i].SetRotation(((int)CurrentOrientation) * (-90 + Mathf.Rad2Deg * i * dTheta)
                    + axle.Rigidbody.rotation);
            }
        }


        //BASIC FUNCTIONS

        public override void Stop()
        {
            base.Stop();
            axle.Stop();
        }

        protected override void Move(float moveInput)
        {
            if (!curled)
            {
                base.Move(moveInput);
            }
            else
            {
                axle.DriveWheel(-moveInput);
            }
        }

        protected override void ChangeDirection()
        {
            if (!curled)
            {
                base.ChangeDirection();
            }
            else
            {
                ChangeDirectionRolling();
            }
        }

        private void ChangeDirectionRolling()
        {
            Vector3 d;

            for (int i = 0; i < NumBodyPieces; i++)
            {
                d = bodyPieces[i].transform.position - axle.transform.position;
                d.x *= -1;
                bodyPieces[i].transform.position = d + axle.transform.position;
                d = bodyPieces[i].transform.localScale;
                d.x *= -1;
                bodyPieces[i].transform.localScale = d;
            }

            ConfigureConnectedAnchors();
        }

        private void ConfigureConnectedAnchors()
        {
            for (int i = 0; i < NumBodyPieces; i++)
            {
                wheelHinges[i].connectedAnchor = new(((int)CurrentOrientation) * wheelRadius * Mathf.Cos(i * dTheta),
                    wheelRadius * Mathf.Sin(i * dTheta));
            }
        }


        //TRANSITION BTWN ROLLING/WALKING

        public void SetCurled(bool val)
        {
            curled = val;

            if (!curled)
            {
                for (int i = 0; i < NumBodyPieces; i++)
                {
                    wheelHinges[i].enabled = false;
                    bodyPieces[i].linearDamping = defaultBodyLinearDamping;
                    bodyPieces[i].sharedMaterial = defaultMaterial;
                }

                axle.Rigidbody.bodyType = RigidbodyType2D.Static;//just so it doesn't go flying off somewhere

                Trigger(typeof(PillBugUncurled).Name);
            }
            else
            {
                ConfigureConnectedAnchors();

                axle.transform.position =
                        0.5f * (bodyPieces[0].position + bodyPieces[NumBodyPieces - 1].position);
                axle.Rigidbody.bodyType = RigidbodyType2D.Dynamic;

                for (int i = 0; i < NumBodyPieces; i++)
                {
                    bodyPieces[i].linearDamping = rollingBodyLinearDamping;
                    bodyPieces[i].sharedMaterial = rollingMaterial;
                    wheelHinges[i].enabled = true;
                }

                Trigger(typeof(PillBugCurled).Name);
            }
        }
    }
}
