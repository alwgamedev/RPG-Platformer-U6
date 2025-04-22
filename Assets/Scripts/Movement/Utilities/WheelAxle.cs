using Unity.VisualScripting;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class WheelAxle : MonoBehaviour
    {
        //[SerializeField] Rigidbody2D axle;
        [SerializeField] float maxSpeed;//angular speed deg/sec
        [SerializeField] float torque;

        public Rigidbody2D Rigidbody { get; private set; }

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
        }

        //private void FixedUpdate()
        //{
        //    if (Input.GetKey(KeyCode.RightArrow))
        //    { 
        //        if (Rigidbody.angularVelocity > -maxSpeed)
        //        {
        //            Rigidbody.AddTorque(-torque);
        //        }
        //    }
        //    else if (Input.GetKey(KeyCode.LeftArrow))
        //    {
        //        if (Rigidbody.angularVelocity < maxSpeed)
        //        {
        //            Rigidbody.AddTorque(torque);
        //        }
        //    }
        //}

        public void DriveWheel(float scale)
        {
            if (Mathf.Abs(Rigidbody.angularVelocity) < maxSpeed)
            {
                Rigidbody.AddTorque(scale * torque);
            }
        }
    }
}