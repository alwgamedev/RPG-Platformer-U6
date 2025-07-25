﻿using RPGPlatformer.Core;
using RPGPlatformer.Movement;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.AIControl
{

    public enum NavigationMode
    {
        rest, timedRest, singleDestination, bounded, pathForwards, pathBackwards
    }

    public class AINavigator : MonoBehaviour, IPathPatroller
    {
        [SerializeField] float destinationTolerance = 0.5f;
        [SerializeField] float boundedPatrolHangTime = 1;
        [SerializeField] float pathPatrolHangTime = 0;

        float hangTimer;

        //for bounded patrol & singleTarget patrol
        //Vector2 leftBound;
        //Vector2 rightBound;
        RandomizableVector3 bounds;
        Vector2 currentDestination;

        public bool checkHorizontalDistanceOnly = true;

        public NavigationMode CurrentMode { get; private set; }
        public LinkedListNode<PatrolPathWayPoint> TargetPoint { get; private set; }

        public event Action DestinationReached;
        public event Action BeginHangTime;


        //BEGIN NEW NAVIGATION

        public void BeginPatrol(NavigationMode mode, object parameters, IMovementController m)
        {
            hangTimer = 0;

            switch(mode)
            {
                case NavigationMode.rest:
                    BeginRest(m);
                    break;
                case NavigationMode.timedRest:
                    BeginTimedRest(m, (float)parameters);
                    break;
                case NavigationMode.singleDestination:
                    BeginSingleDestinationPatrol((Vector3)parameters);
                    break;
                case NavigationMode.bounded:
                    BeginBoundedPatrol((RandomizableVector3/*(Transform, Transform)*/)parameters);
                    break;
                case NavigationMode.pathForwards:
                    BeginPathPatrol((LinkedListNode<PatrolPathWayPoint>)parameters, true);
                    break;
                case NavigationMode.pathBackwards:
                    BeginPathPatrol((LinkedListNode<PatrolPathWayPoint>)parameters, false);
                    break;
            }
        }

        private void BeginRest(IMovementController m)
        {
            CurrentMode = NavigationMode.rest;
            m.SoftStop();
        }

        private void BeginTimedRest(IMovementController m, float time)
        {
            CurrentMode = NavigationMode.timedRest;
            m.SoftStop();
            hangTimer = time;
        }

        private void BeginSingleDestinationPatrol(Vector2 targetPosition)
        {
            CurrentMode = NavigationMode.singleDestination;
            currentDestination = targetPosition;
        }

        private void BeginBoundedPatrol(RandomizableVector3 bounds)
        {
            CurrentMode = NavigationMode.bounded;
            this.bounds = bounds;
            GetNextBoundedDestination();
        }

        private void BeginPathPatrol(LinkedListNode<PatrolPathWayPoint> startNode, bool forwards)
        {
            CurrentMode = forwards ? NavigationMode.pathForwards : NavigationMode.pathBackwards;
            TargetPoint = startNode;
        }


        //PATROL BEHAVIOR

        public void PatrolBehavior(IMovementController m)
        {
            switch (CurrentMode)
            {
                case NavigationMode.timedRest:
                    TimedRestBehavior();
                    break;
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

        private void TimedRestBehavior()
        {
            if (hangTimer > 0)
            {
                hangTimer -= Time.deltaTime;
            }
            else
            {
                OnDestinationReached();
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

        public void ReversePath()
        {
            switch (CurrentMode)
            {
                case NavigationMode.pathForwards:
                    TargetPoint = TargetPoint.Previous ?? TargetPoint;
                    CurrentMode = NavigationMode.pathBackwards;
                    break;
                case NavigationMode.pathBackwards:
                    TargetPoint = TargetPoint.Next ?? TargetPoint;
                    CurrentMode = NavigationMode.pathForwards;
                    break;
                default:
                    break;
            }
        }

        //HANDLE DESTINATION REACHED

        public void OnDestinationReached()
        {
            switch(CurrentMode)
            {
                case NavigationMode.timedRest:
                    DestinationReached?.Invoke();
                    break;
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
            return Vector2.SqrMagnitude((Vector2)transform.position - currentDestination) 
                < destinationTolerance * destinationTolerance;
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
            currentDestination = bounds.Value;
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