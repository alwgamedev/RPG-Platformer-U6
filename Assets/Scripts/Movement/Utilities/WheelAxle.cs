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

        public void Stop()
        {
            Rigidbody.angularVelocity = 0;
        }

        public void DriveWheel(float scale)
        {
            if (Mathf.Abs(Rigidbody.angularVelocity) < maxSpeed)
            {
                Rigidbody.AddTorque(scale * torque);
            }
        }
    }
}