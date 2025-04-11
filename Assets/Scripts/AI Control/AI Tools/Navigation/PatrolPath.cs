using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class PatrolPath : MBNavigationParameters
    {
        [SerializeField] PatrolPathWayPoint[] pathWayPoints;

        public override object Content => WayPoints;
        public LinkedList<PatrolPathWayPoint> WayPoints { get; private set; }

        private void Awake()
        {
            WayPoints = new(pathWayPoints);
        }
    }
}