using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public interface IPathPatroller
    {
        public LinkedListNode<PatrolPoint> TargetPoint { get; }

        public void DestinationReached(); //implementing class can decide whether to target Node.next or Node.prev
    }
}