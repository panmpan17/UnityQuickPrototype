using UnityEngine;


[System.Serializable]
public struct ShapeMountPoint
{
    public Vector3 localPosition;
    public ShapeDirection localLookDirection;
}

public class CombinedGeometryShape : MonoBehaviour
{
    [SerializeField]
    private GeometryBaseShape baseShape;
    [SerializeField]
    private GeometryShapePartData addShape;

    [SerializeField]
    private ShapeMountPoint[] mountingPoints;

    void Start()
    {
        if (baseShape == null || addShape == null)
        {
            return;
        }

        for (int i = 0; i < mountingPoints.Length; i++)
        {
            float zRotation = 0;
            if (mountingPoints[i].localLookDirection != addShape.ShapeDirection)
            {
                int delta = (int)addShape.ShapeDirection - (int)mountingPoints[i].localLookDirection;
                zRotation = delta * 90;
            }
            baseShape.MountShape(addShape, mountingPoints[i].localPosition, Quaternion.Euler(0, 0, zRotation));
        }
    }

    // void OnDrawGizmos()
    // {
    //     if (baseShape == null || addShape == null)
    //     {
    //         return;
    //     }

    //     Gizmos.color = Color.red;
    //     for (int i = 0; i < mountingPoints.Length; i++)
    //     {
    //         Gizmos.DrawSphere(baseShape.transform.TransformPoint(mountingPoints[i].localPosition), 0.1f);
    //     }
    // }
}
