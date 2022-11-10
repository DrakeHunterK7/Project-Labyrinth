/* Modified version from https://github.com/Bl4ckb0ne/delaunay-triangulation

Copyright (c) 2015-2019 Simon Zeni (simonzeni@gmail.com)


Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:


The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.


THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Delaunay2D
{
    public class Triangle : IEquatable<Triangle>
    {
        public Vector2 A { get; set; }
        public Vector2 B { get; set; }
        public Vector2 C { get; set; }
        public bool IsBad { get; set; }

        public Triangle()
        {

        }

        public Triangle(Vector2 a, Vector2 b, Vector2 c)
        {
            A = a;
            B = b;
            C = c;
        }

        public bool ContainsVector2(Vector3 v)
        {
            return Vector3.Distance(v, A) < 0.01f
                || Vector3.Distance(v, B) < 0.01f
                || Vector3.Distance(v, C) < 0.01f;
        }

        public bool CircumCircleContains(Vector3 v)
        {
            Vector3 a = A;
            Vector3 b = B;
            Vector3 c = C;

            float ab = a.sqrMagnitude;
            float cd = b.sqrMagnitude;
            float ef = c.sqrMagnitude;

            float circumX = (ab * (c.y - b.y) + cd * (a.y - c.y) + ef * (b.y - a.y)) / (a.x * (c.y - b.y) + b.x * (a.y - c.y) + c.x * (b.y - a.y));
            float circumY = (ab * (c.x - b.x) + cd * (a.x - c.x) + ef * (b.x - a.x)) / (a.y * (c.x - b.x) + b.y * (a.x - c.x) + c.y * (b.x - a.x));

            Vector3 circum = new Vector3(circumX / 2, circumY / 2);
            float circumRadius = Vector3.SqrMagnitude(a - circum);
            float dist = Vector3.SqrMagnitude(v - circum);
            return dist <= circumRadius;
        }

        public static bool operator ==(Triangle left, Triangle right)
        {
            return (left.A == right.A || left.A == right.B || left.A == right.C)
                && (left.B == right.A || left.B == right.B || left.B == right.C)
                && (left.C == right.A || left.C == right.B || left.C == right.C);
        }

        public static bool operator !=(Triangle left, Triangle right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Triangle t)
            {
                return this == t;
            }

            return false;
        }

        public bool Equals(Triangle t)
        {
            return this == t;
        }

        public override int GetHashCode()
        {
            return A.GetHashCode() ^ B.GetHashCode() ^ C.GetHashCode();
        }
    }

    public static bool AlmostEqual(float x, float y)
    {
        return Mathf.Abs(x - y) <= float.Epsilon * Mathf.Abs(x + y) * 2
            || Mathf.Abs(x - y) < float.MinValue;
    }

    public static bool AlmostEqual(Vector2 left, Vector2 right)
    {
        return AlmostEqual(left.x, right.x) && AlmostEqual(left.y, right.y);
    }

    public List<Vector2> Vertices { get; private set; }
    public List<Edge> Edges { get; private set; }
    public List<Triangle> Triangles { get; private set; }

    Delaunay2D()
    {
        Edges = new List<Edge>();
        Triangles = new List<Triangle>();
    }

    public static Delaunay2D Triangulate(List<Vector2> vertices)
    {
        Delaunay2D delaunay = new Delaunay2D();
        delaunay.Vertices = new List<Vector2>(vertices);
        delaunay.Triangulate();

        return delaunay;
    }

    void Triangulate()
    {
        // Set min max for vertices
        float minX = Vertices[0].x;
        float minY = Vertices[0].y;
        float maxX = minX;
        float maxY = minY;

        foreach (var Vector2 in Vertices)
        {
            if (Vector2.x < minX) minX = Vector2.x;
            if (Vector2.x > maxX) maxX = Vector2.x;
            if (Vector2.y < minY) minY = Vector2.y;
            if (Vector2.y > maxY) maxY = Vector2.y;
        }

        float dx = maxX - minX;
        float dy = maxY - minY;
        float deltaMax = Mathf.Max(dx, dy) * 2;

        Vector2 p1 = new Vector2(minX - 1, minY - 1);
        Vector2 p2 = new Vector2(minX - 1, maxY + deltaMax);
        Vector2 p3 = new Vector2(maxX + deltaMax, minY - 1);

        Triangles.Add(new Triangle(p1, p2, p3));

        // Check each of the circumcircles
        foreach (var Vector2 in Vertices)
        {
            List<Edge> polygon = new List<Edge>();

            foreach (var t in Triangles)
            {
                if (t.CircumCircleContains(Vector2))
                {
                    t.IsBad = true;
                    polygon.Add(new Edge(t.A, t.B));
                    polygon.Add(new Edge(t.B, t.C));
                    polygon.Add(new Edge(t.C, t.A));
                }
            }

            Triangles.RemoveAll((Triangle t) => t.IsBad);

            for (int i = 0; i < polygon.Count; i++)
            {
                for (int j = i + 1; j < polygon.Count; j++)
                {
                    if (Edge.AlmostEqual(polygon[i], polygon[j]))
                    {
                        polygon[i].IsBad = true;
                        polygon[j].IsBad = true;
                    }
                }
            }

            polygon.RemoveAll((Edge e) => e.IsBad);

            foreach (var edge in polygon)
            {
                Triangles.Add(new Triangle(edge.U, edge.V, Vector2));
            }
        }

        // Romove triangles that contain the vertice that is inside of the circumcircle
        Triangles.RemoveAll((Triangle t) => t.ContainsVector2(p1) || t.ContainsVector2(p2) || t.ContainsVector2(p3));

        HashSet<Edge> edgeSet = new HashSet<Edge>();

        // Add edges for those which make up the triangulation edge set
        foreach (var t in Triangles)
        {
            var ab = new Edge(t.A, t.B);
            var bc = new Edge(t.B, t.C);
            var ca = new Edge(t.C, t.A);

            if (edgeSet.Add(ab))
            {
                Edges.Add(ab);
            }

            if (edgeSet.Add(bc))
            {
                Edges.Add(bc);
            }

            if (edgeSet.Add(ca))
            {
                Edges.Add(ca);
            }
        }
    }
}