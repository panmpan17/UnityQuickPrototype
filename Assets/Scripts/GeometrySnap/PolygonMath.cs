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
    static bool IsPointOnEdge(Vector2 a, Vector2 b, Vector2 c)
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
