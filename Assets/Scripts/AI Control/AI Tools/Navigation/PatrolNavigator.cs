using RPGPlatformer.Movement;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public enum PatrolMode
    {
        rest, bounded, pathForwards, pathBackwards
    }

    //Like a GPS for AIPatrollers
    public class PatrolNavigator : MonoBehaviour, IPathPatroller
    {
        [SerializeField] float destinationTolerance = 0.5f;
        [SerializeField] float boundedPatrolHangTime = 1;
        [SerializeField] float pathPatrolHangTime = 0;

        public bool checkHorizontalDistanceOnly;

        float hangTimer;

        //for bounded patrol
        Vector2 leftBound;
        Vector2 rightBound;
        Vector2 currentDestination;

        public PatrolMode CurrentMode { get; private set; }
        public LinkedListNode<PatrolPoint> TargetPoint { get; private set; }

        public event Action DestinationReached;
        public event Action BeginHangTime;


        //BEGIN NEW NAVIGATION

        public void BeginPatrol(PatrolMode mode, PatrolParemeters p, IMovementController m)
        {
            hangTimer = 0;

            switch(mode)
            {
                case PatrolMode.rest:
                    BeginRest(m);
                    break;
                case PatrolMode.bounded:
                    BeginBoundedPatrol(p);
                    break;
                case PatrolMode.pathForwards:
                    BeginPathPatrol(p, true);
                    break;
                case PatrolMode.pathBackwards:
                    BeginPathPatrol(p, false);
                    break;
            }
        }

        public void BeginRest(IMovementController m)
        {
            CurrentMode = PatrolMode.rest;
            m.SoftStop();
        }

        //making these transforms instead of just positions, in case in future we want to patrol between 
        //two dynamic (possibly moving) objects
        public void BeginBoundedPatrol(PatrolParemeters bounds)
        {
            var bds = bounds.Content as Transform[];

            CurrentMode = PatrolMode.bounded;
            leftBound = bds[0].position;
            rightBound = bds[1].position;
            GetNextBoundedDestination();
        }

        public void BeginPathPatrol(PatrolParemeters paths, bool forwards)
        {
            var ps = paths.Content as LinkedList<PatrolPoint>;

            CurrentMode = forwards ? PatrolMode.pathForwards : PatrolMode.pathBackwards;
            TargetPoint = forwards ? ps.First : ps.Last;
        }


        //PATROL BEHAVIOR

        public void PatrolBehavior(IMovementController m)
        {
            switch (CurrentMode)
            {
                case PatrolMode.rest:
                    break;
                case PatrolMode.bounded:
                    BoundedPatrolBehavior(m);
                    break;
                default:
                    PathPatrolBehavior(m);
                    break;
            }
        }

        public void BoundedPatrolBehavior(IMovementController m)
        {
            if (hangTimer > 0)
            {
                hangTimer -= Time.deltaTime;
                return;
            }

            if (HasReachedDestination())
            {
                OnDestinationReached();
                return;
            }

            m.MoveTowards(currentDestination);
        }

        public void PathPatrolBehavior(IMovementController m)
        {
            if (hangTimer > 0)
            {
                hangTimer -= Time.deltaTime;
                return;
            }

            m.MoveTowards(TargetPoint.Value.transform.position);
        }

        //HANDLE DESTINATION REACHED

        public bool HasReachedDestination()
        {
            if (checkHorizontalDistanceOnly)
            {
                return Mathf.Abs(transform.position.x - currentDestination.x) < destinationTolerance;
            }
            return Vector2.Distance(transform.position, currentDestination) < destinationTolerance;
        }

        public void OnDestinationReached()
        {
            switch(CurrentMode)
            {
                case PatrolMode.bounded:
                    OnBoundedDestinationReached();
                    break;
                case PatrolMode.pathForwards:
                    OnPathDestinationReached(true);
                    break;
                case PatrolMode.pathBackwards:
                    OnPathDestinationReached(false);
                    break;
            }
        }

        public bool GetNextDestination()
        {
            return CurrentMode switch
            {
                PatrolMode.bounded => GetNextBoundedDestination(),
                PatrolMode.pathForwards => GetNextPathDestination(true),
                PatrolMode.pathBackwards => GetNextPathDestination(false),
                _ => false
            };
        }

        private void OnBoundedDestinationReached()
        {
            if (boundedPatrolHangTime > 0)
            {
                hangTimer = boundedPatrolHangTime;
                BeginHangTime?.Invoke();
            }

            DestinationReached?.Invoke();
            //GetNextBoundedDestination();
        }

        private bool GetNextBoundedDestination()
        {
            if (leftBound == null ||  rightBound == null)
            {
                return false;
            }

            currentDestination = new(UnityEngine.Random.Range(leftBound.x, rightBound.x), 
                UnityEngine.Random.Range(Math.Min(leftBound.y, rightBound.y), Math.Max(leftBound.y, rightBound.y)));
            return true;
        }

        private void OnPathDestinationReached(bool forwards)
        {
            if (pathPatrolHangTime > 0)
            {
                hangTimer = pathPatrolHangTime;
                BeginHangTime?.Invoke();
            }

            DestinationReached?.Invoke();
        }

        private bool GetNextPathDestination(bool forwards)
        {
            TargetPoint = forwards ? TargetPoint?.Next : TargetPoint?.Previous;

            return TargetPoint != null;
        }

        private void OnDestroy()
        {
            DestinationReached = null;
            BeginHangTime = null;
        }
    }
}