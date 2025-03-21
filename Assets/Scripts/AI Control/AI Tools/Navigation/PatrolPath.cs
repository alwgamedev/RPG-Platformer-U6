using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class PatrolPath : MbNavigationParameters
    {
        [SerializeField] PatrolPathWayPoint[] pathWayPoints;

        public override object Content => WayPoints;
        public LinkedList<PatrolPathWayPoint> WayPoints { get; private set; }

        private void Awake()
        {
            WayPoints = new(pathWayPoints);
        }

        //private void BuildPath()
        //{
        //    WayPoints = new ();

        //    foreach (var w in pathWayPoints)
        //    {
        //        WayPoints.AddLast(w);
        //    }
        //}
    }
}