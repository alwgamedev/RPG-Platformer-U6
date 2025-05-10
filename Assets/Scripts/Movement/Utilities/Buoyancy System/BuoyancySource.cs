using RPGPlatformer.Environment;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class BuoyancySource : MonoBehaviour
    {
        [SerializeField] float fluidDensity;
        [SerializeField] float dampingFactor;
        //[SerializeField] float testingHeight;

        WaterMeshGenerator waterMesh;

        private void Awake()
        {
            waterMesh = GetComponent<WaterMeshGenerator>();
        }

        public Vector2 BuoyancyForce(float areaDisplaced)
        {
            return areaDisplaced * fluidDensity * Vector2.up;
        }

        public Vector2 DragForce(float speed, float crossSectionWidth, Vector2 velocityDirection)
        {
            //we'll say the density got swallowed into the dampingFactor
            //(when density is high, the dampingfactor has to be really small, making this 
            //more of a pain to fine tune)
            return - dampingFactor * crossSectionWidth * speed * speed * velocityDirection;
        }

        //to-do
        public float FluidHeight(float xPosition)
        {
            if (waterMesh)
            {
                return waterMesh.WaveYPosition(xPosition);
            }
            return transform.position.y + 0.5f * transform.lossyScale.y;
        }
    }
}