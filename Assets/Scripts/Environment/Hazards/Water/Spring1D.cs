using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class Spring1D
    {
        //float springConstant;
        //float dampingFactor;

        float displacement;
        float velocity;
        //float springAcceleration;
        float acceleration;

        public float Displacement => displacement;
        public float Velocity => velocity;
        //public float Acceleration => acceleration;

        //public Spring1D(float springConstant, float dampingFactor)
        //{
        //    this.springConstant = springConstant;
        //    this.dampingFactor = dampingFactor;
        //}

        public void Update(float springConstant, float dampingFactor)
        {
            displacement += velocity;// * Time.deltaTime;
            velocity += acceleration;// * Time.deltaTime;
            acceleration = -springConstant * displacement - dampingFactor * velocity;
        }

        public void ApplyAcceleration(float a)
        {
            velocity += a;// * Time.deltaTime;
        }

        //public void ApplyImpulse(float dv)
        //{
        //    velocity += dv;
        //}

        public void ApplyVelocity(float v)
        {
            displacement += v;// * Time.deltaTime;
        }
    }
}