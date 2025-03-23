using System.Collections.Generic;
using System.Linq;
using MPack;
using Unity.VisualScripting.FullSerializer;
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
    static Mesh CreateMesh(ref Vector2[] vertices, ref int[] triangles)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices3D = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices3D[i] = new Vector3(vertices[i].x, vertices[i].y, 0);
        }
        mesh.vertices = vertices3D;
        mesh.triangles = triangles;
        return mesh;
    }

    static Mesh CreateMesh(ref Vector2[] vertices, ref int[] triangles, ref Color[] colors)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices3D = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices3D[i] = new Vector3(vertices[i].x, vertices[i].y, 0);
        }
        mesh.vertices = vertices3D;
        mesh.triangles = triangles;
        mesh.colors = colors;
        return mesh;
    }

    [Header("Shape Data")]
    [SerializeField]
    private ShapeDirection shapeDirection;
    [SerializeField]
    private Vector2[] vertices;
    [SerializeField]
    private int[] triangles;

    [SerializeField]
    private PolygonCollider2D polygonCollider;

    [Header("Outline")]
    [SerializeField]
    private float hue = 0.5f;
    [SerializeField]
    private bool randomHue = false;
    [SerializeField]
    private RangeStruct randomHueRange;
    [SerializeField]
    private float surfaceIntensity = 1f;
    [SerializeField]
    private float outlineIntensity = 0.5f;
    [SerializeField]
    private bool liveUpdate = false;
    private float m_oldHue, m_oldSurfaceIntensity, m_oldOutlineIntensity;

    [SerializeField]
    private ValueWithEnable<float> outlineWidth;

    [Header("Add on")]
    [SerializeField]
    private AddonPart addonPart;
    private AbstractWeaponAddon weaponAddon;
    private AbstractPowerUpAddon powerUpAddon;

    public ShapeDirection ShapeDirection => shapeDirection;
    public Vector2[] Vertices => vertices;
    public int[] Triangles => triangles;

    Mesh m_mesh = null;
    MeshFilter m_meshFilter = null;

    void Awake()
    {
        if (m_mesh != null)
        {
            Destroy(m_mesh);
        }

        if (addonPart)
        {
            if (addonPart.Prefab)
            {
                GameObject addon = Instantiate(addonPart.Prefab, transform);
                weaponAddon = addon.GetComponent<AbstractWeaponAddon>();
                powerUpAddon = addon.GetComponent<AbstractPowerUpAddon>();
            }
            randomHue = false;
            hue = addonPart.OverrideColor;
        }

        m_meshFilter = GetComponent<MeshFilter>();

        if (randomHue)
        {
            hue = Random.Range(randomHueRange.Min, randomHueRange.Max);
        }

        if (outlineWidth.Enable)
        {
            GenerateOutline();
        }
        else
        {
            m_mesh = CreateMesh(ref vertices, ref triangles);
            m_meshFilter.mesh = m_mesh;
        }
    }

    public void MountShape(IGeometryShapePart shapePart, Vector3 localPosition, Quaternion localLookDirection)
    {
        return;

        Vector2[] otherVertices = shapePart.Vertices;
        int[] otherTriangles = shapePart.Triangles;

        Vector2[] newVertices = new Vector2[vertices.Length + otherVertices.Length];
        vertices.CopyTo(newVertices, 0);
        for (int i = 0; i < otherVertices.Length; i++)
        {
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
        m_mesh = CreateMesh(ref newVertices, ref newTriangles);
        m_meshFilter.mesh = m_mesh;
    }


    void GenerateOutline()
    {
        Vector2[] edgePoints = polygonCollider.GetPath(0);
        Vector2[] edgeInwardPoints = PolygonMath.CalculateVerticesInwardExpand(ref edgePoints, outlineWidth.Value);

        int edgePointCount = edgePoints.Length;
        int verticesCount = vertices.Length;
        Vector2[] newVertices = new Vector2[verticesCount + edgePointCount + edgePointCount];
        Vector2[] uv = new Vector2[newVertices.Length];
        for (int i = 0; i < verticesCount; i++)
        {
            newVertices[i] = vertices[i];
            uv[i] = new Vector2(surfaceIntensity, hue);
        }
        for (int i = 0; i < edgePointCount; i++)
        {
            newVertices[verticesCount + i] = edgePoints[i];
            uv[verticesCount + i] = new Vector2(outlineIntensity, hue);
        }
        for (int i = 0; i < edgePointCount; i++)
        {
            newVertices[verticesCount + edgePointCount + i] = edgeInwardPoints[i];
            uv[verticesCount + edgePointCount + i] = new Vector2(outlineIntensity, hue);
        }

        List<int> newTriangles = new List<int>(triangles);
        for (int i = 0; i < edgePoints.Length; i++)
        {
            int a1 = i + verticesCount;
            int a2 = (i + 1) % edgePointCount + verticesCount;
            int b1 = a1 + edgePointCount;
            int b2 = a2 + edgePointCount;

            newTriangles.Add(a1);
            newTriangles.Add(a2);
            newTriangles.Add(b1);

            newTriangles.Add(a2);
            newTriangles.Add(b2);
            newTriangles.Add(b1);
        }

        Vector2[] verticesArr = newVertices.ToArray();
        int[] trianglesArr = newTriangles.ToArray();
        m_mesh = CreateMesh(ref verticesArr, ref trianglesArr);
        m_mesh.uv = uv;

        m_meshFilter.mesh = m_mesh;
    }


    void Update()
    {
        if (!outlineWidth.Enable || !liveUpdate)
        {
            return;
        }

        if (m_oldHue != hue || m_oldSurfaceIntensity != surfaceIntensity || m_oldOutlineIntensity != outlineIntensity)
        {
            m_oldHue = hue;
            m_oldSurfaceIntensity = surfaceIntensity;
            m_oldOutlineIntensity = outlineIntensity;

            Vector2[] uv = new Vector2[m_mesh.vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                uv[i] = new Vector2(surfaceIntensity, hue);
            }
            for (int i = vertices.Length; i < uv.Length; i++)
            {
                uv[i] = new Vector2(outlineIntensity, hue);
            }
            
            m_mesh.uv = uv;
        }
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
        // for (int i = 0; i < vertices.Length; i++)
        // {
        //     Gizmos.DrawSphere(transform.TransformPoint(vertices[i]), 0.1f);
        // }

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

    [ContextMenu("Generate Polygon Collider")]
    void GeneratePolygonCollider()
    {
        var polygonCollider = GetComponent<PolygonCollider2D>();
        if (polygonCollider == null)
        {
            return;
        }

        Vector2[] path = PolygonMath.CalculateOuterEdge(ref vertices, ref triangles);
        var polyCollider = GetComponent<PolygonCollider2D>();
        polyCollider.pathCount = 1;
        polyCollider.SetPath(0, path);
    }
#endregion
#endif
}
