using System.Collections.Generic;
using UnityEngine;

public class SnapController : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve attractForceCurve;

    private bool m_isAttracting = false;

    private List<SnapPart> m_snapParts = new List<SnapPart>();
    private List<SnapPointInfo> m_snapPoints = new List<SnapPointInfo>();


    void Awake()
    {
        SnapPart[] parts = GetComponentsInChildren<SnapPart>();
        for (int i = 0; i < parts.Length; i++)
        {
            AddSnapPartAndSnapPointInfos(parts[i]);
        }
    }

#region  Adding and removing Snap Parts, snap point setting
    void AddSnapPartAndSnapPointInfos(SnapPart snapPart)
    {
        snapPart.SnapController = this;
        m_snapParts.Add(snapPart);

        for (int i = 0; i < snapPart.SnapSettings.Length; i++)
        {
            SnapPointInfo info = new SnapPointInfo();
            info.Setting = snapPart.SnapSettings[i];

            info.ParentPart = snapPart;
            info.SnappedPart = null;
            info.SnappedPartIndex = -1;
            info.State = SnapPointState.Free;
            m_snapPoints.Add(info);
        }
    }
#endregion


#region Snap point manipulation
    bool ScanAvalibleSnapPartToAttract()
    {
        bool canAttract = false;
        int count = m_snapPoints.Count;
        for (int i = 0; i < count; i++)
        {
            if (m_snapPoints[i].State == SnapPointState.Snapped)
            {
                continue;
            }

            m_snapPoints[i].State = SnapPointState.Free;
            m_snapPoints[i].SnappedPart = null;

            Vector3 origin = transform.TransformPoint(m_snapPoints[i].Position);
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, transform.TransformDirection(m_snapPoints[i].LookDirection), m_snapPoints[i].AttractDistance);
            for (int j = 0; j < hits.Length; j++)
            {
                SnapPart snapPart = hits[j].collider.GetComponent<SnapPart>();
                if (snapPart == null || snapPart.SnapController == this)
                {
                    continue;
                }

                bool isSnapped = false;
                for (int k = 0; k < snapPart.SnapSettings.Length; k++)
                {
                    m_snapPoints[i].State = SnapPointState.Attracting;
                    m_snapPoints[i].SnappedPart = snapPart;
                    m_snapPoints[i].SnappedPartIndex = k;
                    isSnapped = true;
                    canAttract = true;
                    break;
                }

                if (isSnapped)
                {
                    break;
                }
            }
        }
        return canAttract;
    }


    void SnapInfoSnapWithPart(int snapPointIndex, SnapPart snapPart)
    {
        m_snapPoints[snapPointIndex].State = SnapPointState.Snapped;

        // Set parent and position
        snapPart.transform.SetParent(transform);

        Vector2 direction = transform.TransformDirection(m_snapPoints[snapPointIndex].LookDirection);
        Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        Vector3 thisSnapPoint = m_snapPoints[snapPointIndex].Position;
        Vector3 otherSnapPoint = snapPart.SnapSettings[m_snapPoints[snapPointIndex].SnappedPartIndex].Position;
        Quaternion rotationDiff = Quaternion.Inverse(rotation) * snapPart.transform.rotation;

        Vector3 snapPosition = transform.TransformPoint(thisSnapPoint) - (rotation * otherSnapPoint);

        snapPart.transform.SetPositionAndRotation(snapPosition, rotation);
        Destroy(snapPart.Rigidbody2D);

        // Add snap part
        snapPart.SnapController = this;
        m_snapParts.Add(snapPart);
        
        // Add snap point infos
        int otherSnapPointIndex = m_snapPoints[snapPointIndex].SnappedPartIndex;
        for (int i = 0; i < snapPart.SnapSettings.Length; i++)
        {
            SnapPointInfo info = new SnapPointInfo();
            info.Setting = snapPart.SnapSettings[i]; // Michael TODO: translate the position
            info.Setting.Position = transform.InverseTransformPoint(snapPart.transform.TransformPoint(info.Position));
            info.Setting.LookDirection = transform.InverseTransformDirection(snapPart.transform.TransformDirection(info.LookDirection));

            info.ParentPart = snapPart;
            info.SnappedPart = null;
            info.SnappedPartIndex = -1;
            info.State = SnapPointState.Free;

            if (i == otherSnapPointIndex)
            {
                info.SnappedPart = m_snapPoints[snapPointIndex].ParentPart;
                info.SnappedPartIndex = snapPointIndex;
                info.State = SnapPointState.Snapped;
            }

            m_snapPoints.Add(info);
        }
    }
#endregion


#region Update
    void FixedUpdate()
    {
        if (m_isAttracting)
        {
            FixedUpdateAttracting();
        }
    }

    void FixedUpdateAttracting()
    {
        Debug.Assert(m_isAttracting);
        int count = m_snapPoints.Count;
        for (int i = 0; i < count; i++)
        {
            if (m_snapPoints[i].State != SnapPointState.Attracting)
            {
                continue;
            }

            if (m_snapPoints[i].SnappedPart == null)
            {
                m_snapPoints[i].State = SnapPointState.Free;
                continue;
            }

            Vector3 delta = transform.position - m_snapPoints[i].SnappedPart.transform.position;
            float distance = delta.magnitude;
            delta /= distance;
            float force = attractForceCurve.Evaluate(distance / m_snapPoints[i].AttractDistance) * m_snapPoints[i].AttractStrength;

            m_snapPoints[i].SnappedPart.Rigidbody2D.linearVelocity = delta * force;
        }
    }
#endregion

#region Inputs
    public void StartAttractingOtherParts()
    {
        m_isAttracting = true;
        ScanAvalibleSnapPartToAttract();
    }

    public void StopAttracting()
    {
        m_isAttracting = false;
    }
#endregion

    void OnCollisionEnter2D(Collision2D collision2D)
    {
        SnapPart snapPart = collision2D.collider.GetComponent<SnapPart>();
        if (snapPart == null)
        {
            return;
        }

        int count = m_snapPoints.Count;
        for (int i = 0; i < count; i++)
        {
            if (m_snapPoints[i].State != SnapPointState.Attracting)
            {
                continue;
            }

            if (m_snapPoints[i].SnappedPart == snapPart)
            {
                SnapInfoSnapWithPart(i, snapPart);
                break;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < m_snapPoints.Count; i++)
        {
            Vector3 point = transform.TransformPoint(m_snapPoints[i].Position);
            Gizmos.DrawSphere(point, 0.1f);
            Gizmos.DrawLine(point, point + transform.TransformDirection(m_snapPoints[i].LookDirection));
        }
    }
}

public enum SnapPointState
{
    Free,
    Attracting,
    Snapped,
}

#if UNITY_EDITOR
[System.Serializable]
#endif
public class SnapPointInfo
{
    public SnapePointSetting Setting;

    public Vector3 Position => Setting.Position;
    public Vector3 LookDirection => Setting.LookDirection;
    public float AttractDistance => Setting.AttractDistance;
    public float AttractStrength => Setting.AttractStrength;

    public SnapPart ParentPart;
    public SnapPart SnappedPart;
    public int SnappedPartIndex;
    public SnapPointState State;
}