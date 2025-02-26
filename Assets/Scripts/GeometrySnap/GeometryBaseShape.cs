using UnityEngine;
using UnityEngine.UIElements;


public enum GizmosDrawType
{
    None,
    AllTime,
    Selected,
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GeometryBaseShape : MonoBehaviour, IGeometryShapePart
{
    static Mesh CreateMesh(Vector3[] vertices, int[] triangles)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        return mesh;
    }

    [SerializeField]
    private ShapeDirection shapeDirection;
    [SerializeField]
    private Vector3[] vertices;
    [SerializeField]
    private int[] triangles;

    public ShapeDirection ShapeDirection => shapeDirection;
    public Vector3[] Vertices => vertices;
    public int[] Triangles => triangles;


    Mesh m_mesh = null;

    void Awake()
    {
        if (m_mesh != null)
        {
            Destroy(m_mesh);
        }
        m_mesh = CreateMesh(vertices, triangles);
        GetComponent<MeshFilter>().mesh = m_mesh;
    }

    public void MountShape(IGeometryShapePart shapePart, Vector3 localPosition, Quaternion localLookDirection)
    {
        Vector3[] otherVertices = shapePart.Vertices;
        int[] otherTriangles = shapePart.Triangles;

        Vector3[] newVertices = new Vector3[vertices.Length + otherVertices.Length];
        vertices.CopyTo(newVertices, 0);
        for (int i = 0; i < otherVertices.Length; i++)
        {
            // newVertices[vertices.Length + i] = otherShape.transform.InverseTransformPoint(otherShape.transform.TransformPoint(otherVertices[i]) + localPosition);
            newVertices[vertices.Length + i] = localLookDirection * otherVertices[i] + localPosition;
        }

        int[] newTriangles = new int[triangles.Length + otherTriangles.Length];
        triangles.CopyTo(newTriangles, 0);
        for (int i = 0; i < otherTriangles.Length; i++)
        {
            newTriangles[triangles.Length + i] = otherTriangles[i] + vertices.Length;
        }

        if (m_mesh != null)
        {
            Destroy(m_mesh);
        }
        vertices = newVertices;
        triangles = newTriangles;
        m_mesh = CreateMesh(newVertices, newTriangles);
        GetComponent<MeshFilter>().mesh = m_mesh;
    }

#if UNITY_EDITOR
    [ContextMenu("Generate Shape Data Scriptable")]
    void GenerateShapeDataScriptable()
    {
        string path = UnityEditor.EditorUtility.SaveFilePanelInProject("Save Shape Data", "New Shape Data", "asset", "Save Shape Data", "Assets/Scriptables/GeometryShapeDatas");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        GeometryShapePartData shapeData = ScriptableObject.CreateInstance<GeometryShapePartData>();
        shapeData.name = "New Shape Data";
        shapeData.SetData(vertices, triangles, shapeDirection);

        UnityEditor.AssetDatabase.CreateAsset(shapeData, path);
        UnityEditor.AssetDatabase.SaveAssets();
    }

#region Drawing Gizmos
    [SerializeField]
    private GizmosDrawType showGizmos;

    void OnDrawGizmos()
    {
        if (showGizmos == GizmosDrawType.AllTime)
        {
            DrawGizmos();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (showGizmos == GizmosDrawType.Selected)
        {
            DrawGizmos();
        }
    }

    void DrawGizmos()
    {
        if (vertices == null || vertices.Length == 0)
        {
            return;
        }

        Gizmos.color = Color.red;
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(transform.TransformPoint(vertices[i]), 0.1f);
        }

        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (i + 2 >= triangles.Length)
            {
                continue;
            }
            Vector3 p0 = transform.TransformPoint(vertices[triangles[i]]);
            Vector3 p1 = transform.TransformPoint(vertices[triangles[i + 1]]);
            Vector3 p2 = transform.TransformPoint(vertices[triangles[i + 2]]);
            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p0);
        }
    }
#endregion
#endif
}
