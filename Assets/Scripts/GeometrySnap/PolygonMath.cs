using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PolygonMath : MonoBehaviour
{
    [SerializeField]
    private GeometryBaseShape shapeA;
    [SerializeField]
    private GeometryBaseShape shapeB;

    static PolygonPointState TestPointStateToShape(GeometryBaseShape shape, Vector2 point)
    {
        Transform t = shape.transform;

        int length = shape.Vertices.Length;
        Vector2 lastPoint = t.TransformPoint(shape.Vertices[length - 1]);

        bool inside = false;
        for (int i = 0; i < length; i++)
        {
            Vector2 newPoint = shape.Vertices[i];

            // TODO: maybe combined the two checks
            if (IsPointOnEdge(point, lastPoint, newPoint))
            {
                return PolygonPointState.OnEdge;
            }

            // Check if point is between the Y-coordinates of the edge
            if ((newPoint.y > point.y) != (lastPoint.y > point.y))
            {
                // Compute intersection of the edge with the horizontal line at point.y
                float xIntersect = (lastPoint.x - newPoint.x) * (point.y - newPoint.y) / (lastPoint.y - newPoint.y) + newPoint.x;

                // Flip the inside flag if the intersection is to the right of the point
                if (point.x < xIntersect)
                {
                    inside = !inside;
                }
            }

            lastPoint = newPoint;
        }

        return inside ? PolygonPointState.Inside : PolygonPointState.Outside;
    }

    /// <summary>
    /// Wether a is on bc line
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <returns>Result</returns> <summary>
    static public bool IsPointOnEdge(Vector2 a, Vector2 b, Vector2 c)
    {
        float crossProduct = (a.y - b.y) * (c.x - b.x) - (a.x - b.x) * (c.y - b.y);
        if (crossProduct != 0) return false;

        bool withinXBounds = a.x >= Mathf.Min(b.x, c.x) && a.x <= Mathf.Max(b.x, c.x);
        bool withinYBounds = a.y >= Mathf.Min(b.y, c.y) && a.y <= Mathf.Max(b.y, c.y);

        return withinXBounds && withinYBounds;
    }

    /// <summary>
    /// Check if point is in triangle
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="point"></param>
    /// <returns></returns> <summary>
    static bool IsPointInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 point)
    {
        float as_x = point.x - a.x;
        float as_y = point.y - a.y;

        bool s_ab = (b.x - a.x) * as_y - (b.y - a.y) * as_x > 0;

        if ((c.x - a.x) * as_y - (c.y - a.y) * as_x > 0 == s_ab)
        {
            return false;
        }

        if ((c.x - b.x) * (point.y - b.y) - (c.y - b.y) * (point.x - b.x) > 0 != s_ab)
        {
            return false;
        }

        return true;
    }

    static public bool InsertTriangleIntoEdge((Vector2, Vector2) edge, (Vector2, Vector2, Vector2) triangle, out Vector2[] insertVerticeOrder)
    {
        bool isPointAOnEdge = IsPointOnEdge(triangle.Item1, edge.Item1, edge.Item2);
        bool isPointBOnEdge = IsPointOnEdge(triangle.Item2, edge.Item1, edge.Item2);
        bool isPointCOnEdge = IsPointOnEdge(triangle.Item3, edge.Item1, edge.Item2);
        int count = isPointAOnEdge ? 1 : 0;
        count += isPointBOnEdge ? 1 : 0;
        count += isPointCOnEdge ? 1 : 0;

        if (count == 3 || count == 0)
        {
            insertVerticeOrder = null;
            return false;
        }

        if (count == 1)
        {
            insertVerticeOrder = new Vector2[4];

            if (isPointAOnEdge)
            {
                insertVerticeOrder[0] = triangle.Item1;
                insertVerticeOrder[1] = triangle.Item2;
                insertVerticeOrder[2] = triangle.Item3;
                insertVerticeOrder[3] = triangle.Item1;
            }
            else if (isPointBOnEdge)
            {
                insertVerticeOrder[0] = triangle.Item2;
                insertVerticeOrder[1] = triangle.Item1;
                insertVerticeOrder[2] = triangle.Item3;
                insertVerticeOrder[3] = triangle.Item2;
            }
            else
            {
                insertVerticeOrder[0] = triangle.Item3;
                insertVerticeOrder[1] = triangle.Item1;
                insertVerticeOrder[2] = triangle.Item2;
                insertVerticeOrder[3] = triangle.Item3;
            }
            return true;
        }

        insertVerticeOrder = new Vector2[3];
        
        Vector2 pointFront;
        Vector2 pointBack;
        if (!isPointAOnEdge)
        {
            insertVerticeOrder[1] = triangle.Item1;

            pointFront = triangle.Item2;
            pointBack = triangle.Item3;
        }
        else if (!isPointBOnEdge)
        {
            insertVerticeOrder[1] = triangle.Item2;
            pointFront = triangle.Item1;
            pointBack = triangle.Item3;
        }
        else
        {
            insertVerticeOrder[1] = triangle.Item3;
            pointFront = triangle.Item1;
            pointBack = triangle.Item2;
        }

        if (Vector2.Distance(pointFront, edge.Item1) < Vector2.Distance(pointBack, edge.Item1))
        {
            insertVerticeOrder[0] = pointFront;
            insertVerticeOrder[2] = pointBack;
        }
        else
        {
            insertVerticeOrder[0] = pointBack;
            insertVerticeOrder[2] = pointFront;
        }

        return true;
    }

    static Dictionary<(int, int), int> GetEdgeCount(ref Vector2[] vertices, ref int[] triangles)
    {
        Dictionary<(int, int), int> edgeCount = new Dictionary<(int, int), int>();

        void AddEdge(int i1, int i2)
        {
            var key = (Mathf.Min(i1, i2), Mathf.Max(i1, i2));
            if (edgeCount.ContainsKey(key))
                edgeCount[key]++;
            else
                edgeCount[key] = 1;
        }

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int a = triangles[i];
            int b = triangles[i + 1];
            int c = triangles[i + 2];
            AddEdge(a, b);
            AddEdge(b, c);
            AddEdge(c, a);
        }

        return edgeCount;
    }

    static public void InsertVerticesIntoEdge(ref List<Vector2> polygonPath, ref List<Vector2> verticesLefts)
    {
        for (int i = 0; i < verticesLefts.Count; i++)
        {
            Vector2 vertex = verticesLefts[i];
            for (int j = 0; j < polygonPath.Count; j++)
            {
                Vector2 a = polygonPath[j];
                Vector2 b = polygonPath[(j + 1) % polygonPath.Count];
                if (IsPointOnEdge(vertex, a, b))
                {
                    polygonPath.Insert(j + 1, vertex);
                    verticesLefts.RemoveAt(i);
                    i--;
                    break;
                }
            }
        }
    }
    

    static public Vector2[] CalculateOuterEdge(ref Vector2[] vertices, ref int[] triangles)
    {
        Dictionary<(int, int), int> edgeCount = GetEdgeCount(ref vertices, ref triangles);

        List<(int, int)> outerEdges = edgeCount.Where(e => e.Value == 1).Select(e => e.Key).ToList();

        List<Vector2> verticesLefts = new List<Vector2>(vertices);

        List<Vector2> polygonPath = new List<Vector2>();
        int current = outerEdges[0].Item1;
        int next = outerEdges[0].Item2;
        polygonPath.Add(vertices[current]);
        verticesLefts.Remove(vertices[current]);

        outerEdges.RemoveAt(0);

        while (outerEdges.Count > 0)
        {
            polygonPath.Add(vertices[next]);
            verticesLefts.Remove(vertices[next]);

            // Find the next edge starting from 'next'
            int index = outerEdges.FindIndex(e => e.Item1 == next || e.Item2 == next);
            if (index == -1) break;

            var edge = outerEdges[index];
            current = next;
            next = (edge.Item1 == next) ? edge.Item2 : edge.Item1;

            outerEdges.RemoveAt(index);
        }

        for (int i = 0; i < polygonPath.Count; i++)
        {
            Vector2 a = polygonPath[i];
            Vector2 b = polygonPath[(i + 1) % polygonPath.Count];
            if (a == b)
            {
                polygonPath.RemoveAt(i);
                i--;
            }
        }

        if (outerEdges.Count == 3)
        {
            int pointAIndex = outerEdges[0].Item1;
            int pointBIndex = outerEdges[0].Item2;
            int pointCIndex = -1;
            for (int i = 1; i < 3; i++)
            {
                if (outerEdges[i].Item1 != pointAIndex && outerEdges[i].Item1 != pointBIndex)
                {
                    pointCIndex = outerEdges[i].Item1;
                    break;
                }
                if (outerEdges[i].Item2 != pointAIndex && outerEdges[i].Item2 != pointBIndex)
                {
                    pointCIndex = outerEdges[i].Item2;
                    break;
                }
            }

            Vector2[] triangle = new Vector2[3] { vertices[pointAIndex], vertices[pointBIndex], vertices[pointCIndex] };

            for (int i = 0; i < polygonPath.Count - 1; i++)
            {
                Vector2[] insertVerticeOrder;
                if (InsertTriangleIntoEdge((polygonPath[i], polygonPath[i + 1]), (triangle[0], triangle[1], triangle[2]), out insertVerticeOrder))
                {
                    for (int j = 0; j < insertVerticeOrder.Length; j++)
                    {
                        polygonPath.Insert(i + 1 + j, insertVerticeOrder[j]);
                    }
                    break;
                }
            }
        }
        return polygonPath.ToArray();
    }

    static public Vector2[] CalculateVerticesInwardExpand(ref Vector2[] vertices, float expandValue)
    {
        Vector2[] pointInwards = new Vector2[vertices.Length];

        Vector2 centerOfMass;
        {
            Vector2 sum = Vector2.zero;
            for (int i = 0; i < vertices.Length; i++)
            {
                sum += vertices[i];
            }
            centerOfMass = sum / vertices.Length;
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector2 center = vertices[i];
            Vector2 left = vertices[(i - 1 + vertices.Length) % vertices.Length];
            Vector2 right = vertices[(i + 1) % vertices.Length];

            Vector2 delteLeft = left - center;
            Vector2 delteRight = right - center;

            Vector2 direction = (delteLeft.normalized + delteRight.normalized) / 2;

            if (Vector2.Dot(direction, centerOfMass - center) < 0)
            {
                direction = -direction;
            }

            pointInwards[i] = vertices[i] + (direction * expandValue);
        }

        return pointInwards;
    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < shapeB.Vertices.Length; i++)
        {
            Vector2 point = shapeB.transform.TransformPoint(shapeB.Vertices[i]);

            // double start = Time.realtimeSinceStartupAsDouble;
            PolygonPointState state = TestPointStateToShape(shapeA, point);
            // Debug.Log("Time: " + (Time.realtimeSinceStartupAsDouble - start));

            if (state == PolygonPointState.Inside)
            {
                Gizmos.color = Color.green;
            }
            else if (state == PolygonPointState.Outside)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.yellow;
            }

            Gizmos.DrawSphere(point, 0.1f);
        }
    }
}

public enum PolygonPointState
{
    Inside,
    Outside,
    OnEdge,
}
