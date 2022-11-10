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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    // U and V vertices for edge
    // Since Y is constant we use Vector2s for the XZ plane
    public Vector2 U { get; set; }
    public Vector2 V { get; set; }
    public bool IsBad { get; set; }

    public float Distance { get; }

    public Edge()
    {

    }

    public Edge(Vector3 u, Vector3 v)
    {
        U = u;
        V = v;

        Distance = Vector3.Distance(u, v);
    }

    public static bool operator ==(Edge left, Edge right)
    {
        return (left.U == right.U || left.U == right.V)
            && (left.V == right.U || left.V == right.V);
    }

    public static bool operator !=(Edge left, Edge right)
    {
        return !(left == right);
    }

    // Checks to see if edges are the same
    public override bool Equals(object obj)
    {
        if (obj is Edge e)
        {
            return this == e;
        }

        return false;
    }

    // Compares edges
    public bool Equals(Edge e)
    {
        return this == e;
    }

    // Returns hash code for Edge
    public override int GetHashCode()
    {
        return U.GetHashCode() ^ V.GetHashCode();
    }

    // Used for triangulation
    public static bool AlmostEqual(Edge left, Edge right)
    {
        return Delaunay2D.AlmostEqual(left.U, right.U) && Delaunay2D.AlmostEqual(left.V, right.V)
            || Delaunay2D.AlmostEqual(left.U, right.V) && Delaunay2D.AlmostEqual(left.V, right.U);
    }
}
