/*The MIT License (MIT)

Copyright (c) 2019 Ryan Vazquez

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.*/

using System;
using BlueRaja;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_Star_Pathfinding : MonoBehaviour
{
    public class Node
    {
        public Vector2Int Position { get; private set; }
        public Node Previous { get; set; }
        public float Cost { get; set; }

        public Node(Vector2Int position)
        {
            Position = position;
        }
    }

    public struct PathCost
    {
        public bool traversable;
        public float cost;
    }

    static readonly Vector2Int[] neighbors = {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
    };

    Grid<Node> grid;
    SimplePriorityQueue<Node, float> queue;
    HashSet<Node> closed;
    Stack<Vector2Int> stack;

    public A_Star_Pathfinding(Vector2Int size)
    {
        grid = new Grid<Node>(size, Vector2Int.zero);

        queue = new SimplePriorityQueue<Node, float>();
        closed = new HashSet<Node>();
        stack = new Stack<Vector2Int>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                grid[x, y] = new Node(new Vector2Int(x, y));
            }
        }
    }

    void ResetNodes()
    {
        var size = grid.Size;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                var node = grid[x, y];
                node.Previous = null;
                node.Cost = float.PositiveInfinity;
            }
        }

    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, Func<Node, Node, PathCost> costFunction)
    {
        ResetNodes();
        queue.Clear();
        closed.Clear();

        queue = new SimplePriorityQueue<Node, float>();
        closed = new HashSet<Node>();

        grid[start].Cost = 0;
        queue.Enqueue(grid[start], 0);

        while (queue.Count > 0)
        {
            Node node = queue.Dequeue();
            closed.Add(node);

            if (node.Position == end)
            {
                return ReconstructPath(node);
            }

            foreach (var offset in neighbors)
            {
                if (!grid.InBounds(node.Position + offset)) continue;
                var neighbor = grid[node.Position + offset];
                if (closed.Contains(neighbor)) continue;

                var pathCost = costFunction(node, neighbor);
                if (!pathCost.traversable) continue;

                float newCost = node.Cost + pathCost.cost;

                if (newCost < neighbor.Cost)
                {
                    neighbor.Previous = node;
                    neighbor.Cost = newCost;

                    if (queue.TryGetPriority(node, out float existingPriority))
                    {
                        queue.UpdatePriority(node, newCost);
                    }
                    else
                    {
                        queue.Enqueue(neighbor, neighbor.Cost);
                    }
                }
            }
        }

        return null;
    }

    List<Vector2Int> ReconstructPath(Node node)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        while (node != null)
        {
            stack.Push(node.Position);
            node = node.Previous;
        }

        while (stack.Count > 0)
        {
            result.Add(stack.Pop());
        }

        return result;
    }
}