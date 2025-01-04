using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public class DirectedGraph<T>
    {
        //collection is like Enumerable (unordered collection) but you can also Add/Remove and use Contains
        protected Collection<T> vertices;
        protected Collection<(T, T)> edges;

        public IEnumerable<T> Vertices => vertices;
        public IEnumerable<(T, T)> Edges => edges;

        public DirectedGraph()
        {
            vertices = new();
            edges = new();
        }

        public DirectedGraph(Collection<T> vertices = null, Collection<(T, T)> edges = null)
        {
            if (vertices != null)
            {
                foreach (T vertex in vertices)
                {
                    AddVertex(vertex);
                    //do it this way in case derived classes have extra conditions for adding vertices
                }
            }
            else
            {
                this.vertices = new Collection<T>();
            }
            if (this.edges != null)
            {
                foreach ((T, T) edge in edges)
                {
                    AddEdge(edge);
                }
            }
            else
            {
                this.edges = new Collection<(T, T)>();
            }
        }

        public virtual bool ContainsVertex(T vertex)
        {
            return vertices.Contains(vertex);
        }

        public virtual bool ContainsEdge((T, T) edge)
        {
            return edges.Contains(edge);
        }

        public virtual T AddVertex(T vertex)
        {
            vertices.Add(vertex);
            return vertex;
        }

        public virtual (T, T) AddEdge((T, T) edge)
        {
            if (vertices.Contains(edge.Item1) && vertices.Contains(edge.Item2) && !edges.Contains(edge))
            {
                edges.Add(edge);
                return edge;
            }
            Debug.LogWarning("Edge could not be added to graph (one of the vertices is invalid).");
            return edge;

        }

        public void AddEdgeBothWays((T, T) edge)
        {
            AddEdge(edge);
            AddEdge((edge.Item2, edge.Item1));
        }

        public virtual void RemoveVertex(T vertex)
        {
            if (vertices.Contains(vertex))
            {
                foreach ((T, T) edge in edges)
                {
                    if (edge.Item1.Equals(vertex) || edge.Item2.Equals(vertex))
                    {
                        edges.Remove(edge);
                    }
                }
                vertices.Remove(vertex);
            }
        }

        public virtual void RemoveEdge((T, T) edge)
        {
            if (edges.Contains(edge))
            {
                edges.Remove(edge);
            }
        }
    }

}