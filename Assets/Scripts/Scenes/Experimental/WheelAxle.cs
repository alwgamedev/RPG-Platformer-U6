using Unity.VisualScripting;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class WheelAxle : MonoBehaviour
    {
        [SerializeField] Rigidbody2D axle;
        [SerializeField] float maxSpeed;//angular speed deg/sec
        [SerializeField] float torque;

        private void FixedUpdate()
        {
            if (Input.GetKey(KeyCode.RightArrow))
            { 
                if (axle.angularVelocity > -maxSpeed)
                {
                    axle.AddTorque(-torque);
                }
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (axle.angularVelocity < maxSpeed)
                {
                    axle.AddTorque(torque);
                }
            }
        }
    }
}