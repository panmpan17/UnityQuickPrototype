using UnityEngine;

public interface IGeometryShapePart
{
    ShapeDirection ShapeDirection { get; }
    Vector3[] Vertices { get; }
    int[] Triangles { get; }
}

public enum ShapeDirection
{
    Right = 0,
    Left = 2,
    Up = 3,
    Down = 1,
}

[CreateAssetMenu(fileName = "GeometryShapePartData", menuName = "Scriptable Objects/GeometryShapePartData")]
public class GeometryShapePartData : ScriptableObject, IGeometryShapePart
{
    [SerializeField]
    private ShapeDirection shapeDirection;
    public ShapeDirection ShapeDirection => shapeDirection;
    [SerializeField]
    private Vector3[] vertices;
    public Vector3[] Vertices => vertices;
    [SerializeField]
    private int[] triangles;
    public int[] Triangles => triangles;

    public void SetData(Vector3[] vertices, int[] triangles, ShapeDirection shapeDirection)
    {
        this.shapeDirection = shapeDirection;
        this.vertices = vertices;
        this.triangles = triangles;
    }
}
