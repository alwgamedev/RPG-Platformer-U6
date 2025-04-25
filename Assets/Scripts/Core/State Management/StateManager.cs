using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    //Each state system should consist of three main parts:

    //(*) Controller ("the pilot")
    //- decides what the system should be doing based on input and current state, and instructs the StateDriver to do it
    //- the highest level script in the system

    //(*) StateManager ("the transmission")
    //- holds the StateGraph information and the actual StateMachine, which is responsible for setting the current state
    //- serves as a reference point for State entry/exit functions (so any callbacks can be registered through
    //  the StateManager)

    //(*) StateDriver ("the engine")
    //- the lowest level script in the system
    //- the "man on the ground" that is responsible for carrying out all the tasks relevant to the state system
    //  (like move, attack, etc.)
    //- its interactions with the game environment drive the state machine,
    //  and it will fire a Trigger event when something has happened that necessitates a state change
    //- the StateDriver should do nothing on its own; it needs to be told what to do by the Controller

    //The hierarchy looks like this:
    //             Controller
    //                /     \
    //               /       \
    //         StateManager   \
    //               \         |
    //                \        |
    //                StateDriver

    public class StateManager<T0, T1, T2>
        where T0 : StateGraph
        where T1 : StateMachine<T0>
        where T2 : StateDriver
    {
        public T0 StateGraph => StateMachine.stateGraph;
        public T1 StateMachine { get; protected set; }

        protected T2 stateDriver;

        public StateManager(T1 stateMachine = null, T2 stateDriver = null)
        {
            if (stateMachine != null)
            {
                StateMachine = stateMachine;
            }
            else
            {
                StateMachine = (T1)Activator.CreateInstance(typeof(T1));
            }
            this.stateDriver = stateDriver;
        }

        public virtual void Configure()
        {
            stateDriver.TriggerEvent += StateMachine.SetCurrentState;
        }

        public State GetState(string stateName)
        {
            try
            {
                return StateGraph.LookupState[stateName];
            }
            catch
            {
                Debug.Log($"State Manager of type {GetType().Name} was unable to find state with name '{stateName}'");
                return StateMachine.nullState;
            }
        }

        public virtual void Freeze()
        {
            StateMachine.Freeze();
        }

        public virtual void Unfreeze()
        {
            StateMachine.Unfreeze();
        }
    }
}