using RPGPlatformer.Movement;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.AIControl
{

    public enum NavigationMode
    {
        rest, singleDestination, bounded, pathForwards, pathBackwards
    }

    public class AINavigator : MonoBehaviour, IPathPatroller
    {
        [SerializeField] float destinationTolerance = 0.5f;
        [SerializeField] float boundedPatrolHangTime = 1;
        [SerializeField] float pathPatrolHangTime = 0;

        public bool checkHorizontalDistanceOnly;
        public float hangTimer;

        //for bounded patrol & singleTarget patrol
        Vector2 leftBound;
        Vector2 rightBound;
        Vector2 currentDestination;

        public NavigationMode CurrentMode { get; private set; }
        public LinkedListNode<PatrolPathWayPoint> TargetPoint { get; private set; }

        public event Action DestinationReached;
        public event Action BeginHangTime;


        //BEGIN NEW NAVIGATION

        public void BeginPatrol(NavigationMode mode, object parameters, IMovementController m)
        {
            switch(mode)
            {
                case NavigationMode.rest:
                    BeginRest(m);
                    break;
                case NavigationMode.singleDestination:
                    BeginSingleDestinationPatrol((Vector2)parameters);
                    break;
                case NavigationMode.bounded:
                    BeginBoundedPatrol((Transform[])parameters);
                    break;
                case NavigationMode.pathForwards:
                    BeginPathPatrol((LinkedList<PatrolPathWayPoint>)parameters, true);
                    break;
                case NavigationMode.pathBackwards:
                    BeginPathPatrol((LinkedList<PatrolPathWayPoint>)parameters, false);
                    break;
            }
        }

        private void BeginRest(IMovementController m)
        {
            CurrentMode = NavigationMode.rest;
            m.SoftStop();
        }

        private void BeginSingleDestinationPatrol(Vector2 targetPosition)
        {
            CurrentMode = NavigationMode.singleDestination;
            currentDestination = targetPosition;
        }

        private void BeginBoundedPatrol(Transform[] bounds)
        {
            CurrentMode = NavigationMode.bounded;
            leftBound = bounds[0].position;
            rightBound = bounds[1].position;
            GetNextBoundedDestination();
        }

        private void BeginPathPatrol(LinkedList<PatrolPathWayPoint> path, bool forwards)
        {
            CurrentMode = forwards ? NavigationMode.pathForwards : NavigationMode.pathBackwards;
            TargetPoint = forwards ? path.First : path.Last;
        }


        //PATROL BEHAVIOR

        public void PatrolBehavior(IMovementController m)
        {
            switch (CurrentMode)
            {
                case NavigationMode.bounded:
                    TargetedPatrolBehavior(m);
                    break;
                case NavigationMode.singleDestination:
                    TargetedPatrolBehavior(m);
                    break;
                case NavigationMode.pathForwards:
                    PathPatrolBehavior(m);
                    break;
                case NavigationMode.pathBackwards:
                    PathPatrolBehavior(m);
                    break;
                default:
                    break;
            }
        }

        private void TargetedPatrolBehavior(IMovementController m)
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

        private void PathPatrolBehavior(IMovementController m)
        {
            if (hangTimer > 0)
            {
                hangTimer -= Time.deltaTime;
                return;
            }

            m.MoveTowards(TargetPoint.Value.transform.position);
        }

        //HANDLE DESTINATION REACHED

        public void OnDestinationReached()
        {
            switch(CurrentMode)
            {
                case NavigationMode.bounded:
                    OnBoundedDestinationReached();
                    break;
                case NavigationMode.singleDestination:
                    DestinationReached?.Invoke();
                    break;
                case NavigationMode.pathForwards:
                    OnPathDestinationReached(true);
                    break;
                case NavigationMode.pathBackwards:
                    OnPathDestinationReached(false);
                    break;
            }
        }

        public bool GetNextDestination()
        {
            return CurrentMode switch
            {
                NavigationMode.bounded => GetNextBoundedDestination(),
                NavigationMode.singleDestination => false,
                NavigationMode.pathForwards => GetNextPathDestination(true),
                NavigationMode.pathBackwards => GetNextPathDestination(false),
                _ => false
            };
        }

        private bool HasReachedDestination()
        {
            if (checkHorizontalDistanceOnly)
            {
                return Mathf.Abs(transform.position.x - currentDestination.x) < destinationTolerance;
            }
            return Vector2.Distance(transform.position, currentDestination) < destinationTolerance;
        }

        private void OnBoundedDestinationReached()
        {
            if (boundedPatrolHangTime > 0)
            {
                hangTimer = boundedPatrolHangTime;
                BeginHangTime?.Invoke();
            }

            DestinationReached?.Invoke();
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