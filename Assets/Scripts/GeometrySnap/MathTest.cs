using System.Collections.Generic;
using MPack;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class MathTest : MonoBehaviour
{
    [SerializeField]
    private PolygonCollider2D polygonCollider;

    [SerializeField]
    private Vector2[] pointInwards;

    [SerializeField, InspectorButton("Test", "Test")]
    private bool test;

    

    void Test()
    {
        Vector2[] path = polygonCollider.GetPath(0);
        pointInwards = new Vector2[path.Length];

        Vector2 centerOfMass = polygonCollider.bounds.center;

        for (int i = 0; i < path.Length; i++)
        {
            Vector2 center = path[i];
            Vector2 left = path[(i - 1 + path.Length) % path.Length];
            Vector2 right = path[(i + 1) % path.Length];

            Vector2 delteLeft = left - center;
            Vector2 delteRight = right - center;

            pointInwards[i] = (delteLeft.normalized + delteRight.normalized) / 2;

            // if (Vector2.Dot(pointInwards[i], centerOfMass - center) < 0)
            // {
            //     pointInwards[i] = -pointInwards[i];
            // }
        }

        // Repaint the gizmos
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }

    void OnDrawGizmos()
    {
        if (pointInwards == null || pointInwards.Length == 0)
            return;

        Vector2[] path = polygonCollider.GetPath(0);

        Gizmos.color = Color.red;
        for (int i = 0; i < pointInwards.Length; i++)
        {
            Gizmos.DrawLine(path[i], path[i] + pointInwards[i]);
        }
    }
}
