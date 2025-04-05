using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public class StateGraph : DirectedGraph<State>
    {
        public Dictionary<string, State> LookupState { get; protected set; } = new();

        public StateGraph() : base() { }
        public StateGraph(ICollection<State> vertices = null, ICollection<(State, State)> edges = null) 
            : base(vertices, edges) { }
        //note that, because constructor adds the vertices one by one through "AddVertex",
        //the dictionary gets populated automatically.

        public override State AddVertex(State state)
        {
            if (!ContainsStateOfType(state.GetType()))
            {
                LookupState[state.name] = state;
                return base.AddVertex(state);
            }
            else
            {
                Debug.LogWarning($"State graph of type {GetType()} already contains a " +
                    $"state of type {state.name}");
                return null;
            }
        }
        public virtual T CreateNewVertex<T>() where T : State
        {
            return (T)AddVertex((State)Activator.CreateInstance(typeof(T)));
        }

        public override void RemoveVertex(State state)
        {
            LookupState.Remove(state.name);
            base.RemoveVertex(state);
        }

        public State FindStateOfType(Type type)
        {
            return vertices?.FirstOrDefault(v => v.name == type.Name);
        }

        public bool ContainsStateOfType(Type type)
        {
            return FindStateOfType(type) != null;
        }

        public virtual bool CanTransition(State state1, State state2)
        {
            return edges.Contains((state1, state2));
        }
    }
}