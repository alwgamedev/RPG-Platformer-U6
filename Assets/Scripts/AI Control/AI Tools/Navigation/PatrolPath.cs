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
            BuildPath();
        }

        private void BuildPath()
        {
            WayPoints = new ();

            for (int i = 0; i < pathWayPoints.Length; i++)
            {
                foreach (var w in pathWayPoints)
                {
                    WayPoints.AddLast(w);
                }
            }
        }
    }
}