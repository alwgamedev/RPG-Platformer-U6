using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public class DirectedGraph<T>
    {
        protected HashSet<T> vertices;
        protected HashSet<(T, T)> edges;

        //public IEnumerable<T> Vertices => vertices;
        //public IEnumerable<(T, T)> Edges => edges;

        public DirectedGraph()
        {
            vertices = new();
            edges = new();
        }

        public DirectedGraph(IEnumerable<T> vertices = null, IEnumerable<(T, T)> edges = null)
        {
            this.vertices = new();
            if (vertices != null)
            {
                foreach (var vertex in vertices)
                {
                    AddVertex(vertex);
                    //do it this way in case derived classes have extra conditions for adding vertices
                }
            }

            this.edges = new();
            if (edges != null)
            {
                foreach (var edge in edges)
                {
                    AddEdge(edge);
                }
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
            Debug.LogWarning("Edge could not be added to graph (one of the vertices is invalid," +
                " or the edge is already present).");
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
            edges.Remove(edge);
        }
    }

}