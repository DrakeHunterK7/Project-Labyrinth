using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prim_MST
{
    public static List<Edge> MinimumSpanningTree(List<Edge> edges, Vector3 start)
    {
        HashSet<Vector3> openSet = new HashSet<Vector3>();
        HashSet<Vector3> closedSet = new HashSet<Vector3>();

        // Add edges to open set (set of edges to be checked)
        foreach (var edge in edges)
        {
            openSet.Add(edge.U);
            openSet.Add(edge.V);
        }

        closedSet.Add(start);

        List<Edge> results = new List<Edge>();

        // compare distances and edges to find minimum costing path
        while (openSet.Count > 0)
        {
            bool chosen = false;
            Edge chosenEdge = null;
            float minWeight = float.PositiveInfinity;

            foreach (var edge in edges)
            {
                int closedVertices = 0;
                if (!closedSet.Contains(edge.U)) closedVertices++;
                if (!closedSet.Contains(edge.V)) closedVertices++;
                if (closedVertices != 1) continue;

                if (edge.Distance < minWeight)
                {
                    chosenEdge = edge;
                    chosen = true;
                    minWeight = edge.Distance;
                }
            }

            if (!chosen) break;
            results.Add(chosenEdge);

            openSet.Remove(chosenEdge.U);
            openSet.Remove(chosenEdge.V);

            closedSet.Add(chosenEdge.U);
            closedSet.Add(chosenEdge.V);
        }

        return results;
    }
}

