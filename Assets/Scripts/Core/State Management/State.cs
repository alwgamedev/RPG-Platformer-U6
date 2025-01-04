using System;

namespace RPGPlatformer.Core
{
    public abstract class State
    {
        public Action OnEntry;//fires only when entering from a different state
        public Action OnEntryToSameState;//fires when re-entering the same state
        public Action OnExit;//fires only when exiting to a different state
        public Action OnExitToSameState;//fires when exiting to the same state

        //NOTE: to prevent issues, it fires either entry OR reentry, not both.
        //(because if both fired on state re-entry then we have to choose which fires first, and we can't always make the right choice
    }

    public class NullState : State { }

    public class StateNotFoundException : Exception
    {
        public StateNotFoundException(string stateName) : base($"Unable to find a state with name {stateName}") { }
        public StateNotFoundException(State state) : base($"Unable to find a state with name {StateName(state)}") { }
        public StateNotFoundException(Type type) : base($"Unable to find a state of type {type.Name}") { }

        static string StateName(State state)
        {
            string stateName;
            try
            {
                stateName = state.GetType().Name;
            }
            catch
            {
                stateName = "(unknown)";
            }
            return stateName;
        }
    }
}