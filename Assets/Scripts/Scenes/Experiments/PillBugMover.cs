using RPGPlatformer.Movement;
using UnityEngine;

public class PillBugMover : RopeWalker
{
    //[SerializeField] RopeWalker walker;
    [SerializeField] WheelAxle axle;
    [SerializeField] float wheelAngleRange;//rad -- range of angle over which the bodypieces will be spread out
    [SerializeField] float radiusMultiplier = 1;
    [SerializeField] float defaultBodyLinearDamping = 5;
    [SerializeField] float rollingBodyLinearDamping = 100;
    [SerializeField] protected PhysicsMaterial2D defaultMaterial;
    [SerializeField] protected PhysicsMaterial2D rollingMaterial;

    //int n;
    bool walking;//either walking or rolling
    float wheelRadius => radiusMultiplier * (n / (float)(n - 1)) * Length / (2 * Mathf.PI);
    float dTheta => wheelAngleRange / n;

    HingeJoint2D[] wheelHinges;

    //WARNING: release move input when you transition state or else it goes crazy
    //also need to do:
        //a) flip direction while in curled state
        //b) curl and uncurl sequences
            //for curl for now we can just enable the hinge joint (although this is a little quick,
            //so we might still tween them into place)
            //uncurl doesn't really "unfurl" like i'd want (getting overcorrection/too much springiness
            //and adding linar damping makes them act like they have a parachute attached as they fall)
            //(what we could do WCS is just figure out the goal positions and move them by force to the goal positions,
            //with hinge joints disabled during this tween of course)


    private void Start()
    {
        wheelHinges = new HingeJoint2D[n];

        for (int i = 0; i < n; i++)
        {
            wheelHinges[i] = bodyPieces[i].gameObject.GetComponent<HingeJoint2D>();
            //wheelHinges[i].connectedAnchor = new(wheelRadius * Mathf.Cos(i * dTheta), 
            //    wheelRadius * Mathf.Sin(i * dTheta));
        }

        SetState(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            SetState(!walking);
        }
    }

    protected override void UpdatePhysicsData()
    {
        if (walking)
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
        for (int i = 0; i < n; i++)
        {
            bodyPieces[i].SetRotation(((int)CurrentOrientation) * (-90 + Mathf.Rad2Deg * i * dTheta) 
                + axle.Rigidbody.rotation);
        }
    }


    //BASIC FUNCTIONS

    protected override void HandleMoveInput(float moveInput)
    {
        if (walking)
        {
            base.HandleMoveInput(moveInput);
        }
        else
        {
            axle.DriveWheel(-moveInput);
        }
    }

    protected override void ChangeDirection()
    {
        if (walking)
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
        CurrentOrientation = (HorizontalOrientation)(-(int)CurrentOrientation);

        Vector3 d;

        for (int i = 0; i < n; i++)
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
        for (int i = 0; i < n; i++)
        {
            wheelHinges[i].connectedAnchor = new(((int)CurrentOrientation) * wheelRadius * Mathf.Cos(i * dTheta),
                wheelRadius * Mathf.Sin(i * dTheta));
        }
    }


    //TRANSITION BTWN ROLLING/WALKING

    private void SetState(bool walking)
    {
        this.walking = walking;

        //enabled = walking;
        axle.enabled = !walking;

        if (walking)
        {

            for (int i = 0; i < n; i++)
            {
                wheelHinges[i].enabled = false;
                bodyPieces[i].linearDamping = defaultBodyLinearDamping;
                bodyPieces[i].sharedMaterial = defaultMaterial;
            }

            axle.Rigidbody.bodyType = RigidbodyType2D.Static;//just so it doesn't go flying off somewhere
        }
        else
        {
            ConfigureConnectedAnchors();

            axle.transform.position =
                    0.5f * (bodyPieces[0].position + bodyPieces[n - 1].position);
            axle.Rigidbody.bodyType = RigidbodyType2D.Dynamic;

            for (int i = 0; i < n; i++)
            {
                bodyPieces[i].linearDamping = rollingBodyLinearDamping;
                bodyPieces[i].sharedMaterial = rollingMaterial;
                wheelHinges[i].enabled = true;
            }
        }
    }
}
