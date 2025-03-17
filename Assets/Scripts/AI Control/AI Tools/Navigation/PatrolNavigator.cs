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
        [SerializeField] float destinationTolerance = 0.1f;
        [SerializeField] float boundedPatrolHangTime = 1;
        [SerializeField] float pathPatrolHangTime = 0;

        float hangTimer;

        //for bounded patrol
        Vector2 leftBound;
        Vector2 rightBound;
        Vector2 currentDestination;

        public PatrolMode CurrentMode { get; private set; }
        public LinkedListNode<PatrolPoint> TargetPoint { get; private set; }

        public event Action PatrolComplete;
        public event Action BeginHangTime;


        //BEGIN NEW NAVIGATION

        public void BeginPatrol(PatrolMode mode, PatrolParemeters p, IMovementController m)
        {
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
            CurrentMode = PatrolMode.bounded;
            leftBound = ((Transform)bounds.Content[0]).position;
            rightBound = ((Transform)bounds.Content[1]).position;
            GetNextBoundedDestination();
        }

        public void BeginPathPatrol(PatrolParemeters paths, bool forwards, bool chooseRandomPath = false)
        {
            var ps = paths.Content as LinkedList<PatrolPoint>[];
            int i = chooseRandomPath ? UnityEngine.Random.Range(0, ps.Length) : 0;

            CurrentMode = forwards ? PatrolMode.pathForwards : PatrolMode.pathBackwards;
            TargetPoint = forwards ? ps[i].First : ps[i].Last;
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

            if (TargetPoint != null)
            {
                m.MoveTowards(TargetPoint.Value.transform.position);
            }
        }

        //HANDLE DESTINATION REACHED

        public bool HasReachedDestination(bool checkXOnly = true)
        {
            if (checkXOnly)
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
                    BoundedDestinationReached();
                    break;
                case PatrolMode.pathForwards:
                    PathDestinationReached(true);
                    break;
                case PatrolMode.pathBackwards:
                    PathDestinationReached(false);
                    break;
            }
        }

        private void BoundedDestinationReached()
        {
            if (boundedPatrolHangTime > 0)
            {
                hangTimer = boundedPatrolHangTime;
                BeginHangTime?.Invoke();
            }
            GetNextBoundedDestination();
        }

        private void GetNextBoundedDestination()
        {
            currentDestination = new(UnityEngine.Random.Range(leftBound.x, rightBound.x), 0);
        }

        private void PathDestinationReached(bool forwards)
        {
            if (pathPatrolHangTime > 0)
            {
                hangTimer = pathPatrolHangTime;
                BeginHangTime?.Invoke();
            }

            TargetPoint = forwards ? TargetPoint?.Next : TargetPoint?.Previous;

            if (TargetPoint == null)
            {
                PatrolComplete?.Invoke();
            }
        }

        private void OnDestroy()
        {
            PatrolComplete = null;
        }
    }
}