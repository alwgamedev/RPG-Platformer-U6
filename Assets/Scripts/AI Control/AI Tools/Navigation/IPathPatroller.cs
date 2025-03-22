using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public interface IPathPatroller
    {
        public LinkedListNode<PatrolPathWayPoint> TargetPoint { get; }

        public void OnDestinationReached(); 
        //e.g. continue to node.Next or node.Prev
    }
}