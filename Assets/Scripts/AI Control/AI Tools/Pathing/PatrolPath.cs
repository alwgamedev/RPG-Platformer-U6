using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class PatrolPath : PatrolParemeters
    {
        //Can hold several different paths (in case e.g. you want to randomize the path taken)
        [SerializeField] PatrolPoint[][] pathWayPoints;

        public override object[] Content => WayPoints;
        public LinkedList<PatrolPoint>[] WayPoints { get; private set; }

        private void Awake()
        {
            BuildPaths();
        }

        private void BuildPaths()
        {
            WayPoints = new LinkedList<PatrolPoint>[pathWayPoints.Length];

            for (int i = 0; i < pathWayPoints.Length; i++)
            {
                WayPoints[i] = new();
                foreach (var w in pathWayPoints[i])
                {
                    WayPoints[i].AddLast(w);
                }
            }
        }
    }
}