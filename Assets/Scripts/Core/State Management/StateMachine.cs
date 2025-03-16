using System;

namespace RPGPlatformer.Core
{
    public class StateMachine<T> where T : StateGraph
    {
        public T stateGraph;
        public readonly NullState nullState;

        protected State storedState;

        public State CurrentState { get; protected set; }
        public bool IsFrozen { get; protected set; }

        public event Action<State> StateChange;
        public event Action<State> StateStored;

        public StateMachine()
        {
            nullState = new();
            CurrentState = nullState;
            stateGraph = (T)Activator.CreateInstance(typeof(T));
        }

        public virtual void SetCurrentState(State newState)
        {
            if (IsFrozen)
            {
                StoreState(newState);
                return;
            }
            if (stateGraph.CanTransition(CurrentState, newState)
                || newState == CurrentState
                || (CurrentState == nullState && stateGraph.ContainsVertex(newState)))
            {
                TransitionToState(newState);
            }
        }

        public virtual void SetCurrentState(string stateName)//we should do away with this now that manager has a public reference to the state machine
        {
            SetCurrentState(stateGraph.LookupState[stateName]);
        }

        public virtual void ForceCurrentState(State newState)//note: does not affect stored state, so you can do this after freezing to move to an "inactive" state
        {
            if (stateGraph.ContainsVertex(newState))
            {
                TransitionToState(newState);
                //NOTE: does not check if Frozen, does not check if CanTransition
                //also does NOT this new state in storedState
            }
        }

        protected virtual void TransitionToState(State newState)
        {
            State previousState = CurrentState;
            CurrentState = newState;
            //bool transitioningToSameState = previousState.Equals(CurrentState);

            if (previousState.Equals(CurrentState))
            {
                previousState.OnExitToSameState?.Invoke();
                CurrentState.OnEntryToSameState?.Invoke();
            }
            else
            {
                previousState.OnExit?.Invoke();
                CurrentState.OnEntry?.Invoke();
            }

            StateChange?.Invoke(newState);
        }

        protected virtual void StoreState(State state)
        {
            storedState = state;
            StateStored?.Invoke(storedState);
        }

        protected virtual void ReturnToStoredState()
        {
            SetCurrentState(storedState);
        }

        public virtual void Freeze()
        {
            if (IsFrozen) return;
            IsFrozen = true;
            StoreState(CurrentState);
        }

        public virtual void Unfreeze()
        {
            IsFrozen = false;
            ReturnToStoredState();
        }

        public bool HasState(string stateName)
        {
            return CurrentState.name == stateName;
        }

        public bool HasState(Type type)
        {
            return CurrentState.GetType() == type;
        }
    }
}