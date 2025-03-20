using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public interface IPathPatroller
    {
        public LinkedListNode<PatrolPathWayPoint> TargetPoint { get; }

        public void OnDestinationReached(); 
        //implementing class can decide whether to target node.Next or node.Prev
    }
}