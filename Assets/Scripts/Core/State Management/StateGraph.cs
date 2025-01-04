using System.Linq;
using System.Collections.ObjectModel;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace RPGPlatformer.Core
{
    public class StateGraph : DirectedGraph<State>
    {
        public Dictionary<string, State> LookupState { get; protected set; } = new();

        public StateGraph() : base() { }
        public StateGraph(Collection<State> vertices = null, Collection<(State, State)> edges = null) : base(vertices, edges) { }
        //note that, because constructor adds the vertices one by one through "AddVertex", the dictionary gets populated automatically.

        public override State AddVertex(State state)
        {
            if (!ContainsStateOfType(state.GetType()))
            {
                LookupState[state.GetType().Name] = state;
                return base.AddVertex(state);
            }
            else
            {
                Debug.LogWarning($"Tried to add a vertex of type {state.GetType().Name} to {GetType().Name}, but it already contains a state of that type.");
                return null;
            }
        }
        public virtual T CreateNewVertex<T>() where T : State
        {
            return (T)AddVertex((State)Activator.CreateInstance(typeof(T)));
        }

        public override void RemoveVertex(State state)
        {
            LookupState.Remove(state.GetType().Name);
            base.RemoveVertex(state);
        }

        public State FindStateOfType(Type type)
        {
            try
            {
                return vertices.First(x => x.GetType() == type);
            }
            catch (InvalidOperationException)
            {
                throw new StateNotFoundException(type);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }

        public bool ContainsStateOfType(Type type)
        {
            if (!typeof(State).IsAssignableFrom(type))
            {
                return false;
            }
            foreach (State state in vertices)
            {
                if (state.GetType().Name == type.Name)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool CanTransition(State state1, State state2)
        {
            return edges.Contains((state1, state2));
        }
    }
}