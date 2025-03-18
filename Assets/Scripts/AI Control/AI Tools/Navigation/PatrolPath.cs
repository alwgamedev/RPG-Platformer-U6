using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class PatrolPath : PatrolParemeters
    {
        [SerializeField] PatrolPoint[] pathWayPoints;

        public override object Content => WayPoints;
        public LinkedList<PatrolPoint> WayPoints { get; private set; }

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