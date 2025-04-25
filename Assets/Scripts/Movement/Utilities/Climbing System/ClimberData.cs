using UnityEngine;

namespace RPGPlatformer.Movement
{
    public struct ClimberData
    {
        public ClimbNode currentNode;
        public float localPosition;
        //position relative to current node measured on a bent number line, where
        //positive numbers go in the direction current -> current.Higher and negatives in the
        //direction current -> current.Lower

        public ClimberData(ClimbNode currentNode, float localPosition)
        {
            this.currentNode = currentNode;
            this.localPosition = localPosition;
        }

        //note this will throw an error if no current node
        public Vector3 WorldPosition()
        {
            return currentNode.LocalToWorldPosition(localPosition);
        }
    }
}