using RPGPlatformer.Movement;
using UnityEngine;

public class PillBugMover : MonoBehaviour
{
    [SerializeField] RopeWalker walker;
    [SerializeField] WheelAxle axle;
    [SerializeField] PhysicsMaterial2D defaultBodyMat;
    [SerializeField] PhysicsMaterial2D rollingBodyMat;
    [SerializeField] float radiusMultiplier = 1;
    [SerializeField] float defaultBodyLinearDamping = 5;
    [SerializeField] float rollingBodyLinearDamping = 100;

    int n;
    bool walking;//either walking or rolling
    float wheelRadius;
    float dTheta;//in rad (not Mathf.Cos takes rad but rigidbody2D.rotation is in deg)

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
        n = walker.n;
        wheelRadius = radiusMultiplier * (n / (float)(n - 1)) * walker.Length / (2 * Mathf.PI);
        wheelHinges = new HingeJoint2D[n];
        dTheta = 2 * Mathf.PI / n;

        for (int i = 0; i < n; i++)
        {
            wheelHinges[i] = walker.BodyPieces[i].gameObject.GetComponent<HingeJoint2D>();
            wheelHinges[i].connectedAnchor = new(wheelRadius * Mathf.Cos(i * dTheta), 
                wheelRadius * Mathf.Sin(i * dTheta));
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

    private void FixedUpdate()
    {
        if (!walking)
        {
            UpdateRollingRotations();
        }
    }

    private void SetState(bool walking)
    {
        this.walking = walking;

        walker.enabled = walking;
        axle.enabled = !walking;

        if (walking)
        {

            for (int i = 0; i < n; i++)
            {
                wheelHinges[i].enabled = false;
                walker.BodyPieces[i].linearDamping = defaultBodyLinearDamping;
                walker.BodyPieces[i].sharedMaterial = defaultBodyMat;
            }

            axle.Rigidbody.bodyType = RigidbodyType2D.Static;//just so it doesn't go flying off somewhere
        }
        else
        {
            axle.transform.position =
                    0.5f * (walker.BodyPieces[0].position + walker.BodyPieces[n - 1].position);
            //axle.Rigidbody.totalForce = Vector2.zero;
            //axle.Rigidbody.totalTorque = 0;
            axle.Rigidbody.bodyType = RigidbodyType2D.Dynamic;

            for (int i = 0; i < n; i++)
            {
                walker.BodyPieces[i].linearDamping = rollingBodyLinearDamping;
                wheelHinges[i].enabled = true;
            }
        }
    }

    private void UpdateRollingRotations()
    {
        for (int i = 0; i < n; i++)
        {
            walker.BodyPieces[i].SetRotation(-90 + Mathf.Rad2Deg * i * dTheta + axle.Rigidbody.rotation);
        }
    }
}
