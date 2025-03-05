using System.Collections.Generic;
using UnityEngine;

public enum SnapPointState
{
    Free,
    Attracting,
    Snapped,
}

[System.Serializable]
public struct SnapePointSetting
{
    public Vector2 Position;
    public Vector2 LookDirection;
    public float AttractDistance;
    public float AttractStrength;
    public int Indetifier;
    public bool IsSlot;

    [System.NonSerialized]
    public SnapPart SnappedPart;
    [System.NonSerialized]
    public int SnappedPartIndex;
    [System.NonSerialized]
    public SnapPointState State;
}

public class SnapPart : MonoBehaviour
{
    [SerializeField]
    private SnapePointSetting[] m_snapPoints;
    [SerializeField]
    private AnimationCurve attractForceCurve;

    [SerializeField]
    private new Rigidbody2D rigidbody2D;


    private bool m_isAttracting = false;

    void Awake()
    {
        if (rigidbody2D == null)
            rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void AttractOtherParts()
    {
        ScanAvalibleSnapPartToAttract();
        m_isAttracting = true;
    }

    void ScanAvalibleSnapPartToAttract()
    {
        for (int i = 0; i < m_snapPoints.Length; i++)
        {
            if (m_snapPoints[i].State == SnapPointState.Snapped)
            {
                continue;
            }

            m_snapPoints[i].State = SnapPointState.Free;
            m_snapPoints[i].SnappedPart = null;

            Vector3 origin = transform.TransformPoint(m_snapPoints[i].Position);
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, m_snapPoints[i].LookDirection, m_snapPoints[i].AttractDistance);
            for (int j = 0; j < hits.Length; j++)
            {
                SnapPart snapPart = hits[j].collider.GetComponent<SnapPart>();
                if (snapPart == null || snapPart == this)
                {
                    continue;
                }

                bool isSnapped = false;
                for (int k = 0; k < snapPart.m_snapPoints.Length; k++)
                {
                    if (snapPart.m_snapPoints[k].State == SnapPointState.Snapped)
                    {
                        continue;
                    }


                    m_snapPoints[i].State = SnapPointState.Attracting;
                    m_snapPoints[i].SnappedPart = snapPart;
                    m_snapPoints[i].SnappedPartIndex = k;
                    isSnapped = true;
                    break;
                }

                if (isSnapped)
                {
                    break;
                }
            }
        }
    }

    public void StopAttracting()
    {
        for (int i = 0; i < m_snapPoints.Length; i++)
        {
            if (m_snapPoints[i].State == SnapPointState.Attracting)
            {
                m_snapPoints[i].SnappedPart.m_snapPoints[m_snapPoints[i].SnappedPartIndex].State = SnapPointState.Free;
                m_snapPoints[i].SnappedPart.m_snapPoints[m_snapPoints[i].SnappedPartIndex].SnappedPart = null;

                m_snapPoints[i].State = SnapPointState.Free;
                m_snapPoints[i].SnappedPart = null;
                m_snapPoints[i].SnappedPartIndex = -1;
            }
        }
        m_isAttracting = false;
    }

    void FixedUpdate()
    {
        if (!m_isAttracting)
        {
            return;
        }

        for (int i = 0; i < m_snapPoints.Length; i++)
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

            m_snapPoints[i].SnappedPart.rigidbody2D.linearVelocity = delta * force;
        }
    }

    void OnCollisionEnter2D(Collision2D collision2D)
    {
        SnapPart snapPart = collision2D.collider.GetComponent<SnapPart>();
        if (snapPart == null)
        {
            return;
        }

        for (int i = 0; i < m_snapPoints.Length; i++)
        {
            if (m_snapPoints[i].State != SnapPointState.Attracting)
            {
                continue;
            }

            if (m_snapPoints[i].SnappedPart == snapPart)
            {
                SnapWith(i, snapPart);
                break;
            }
        }
    }

    void SnapWith(int snapPointIndex, SnapPart snapPart)
    {
        m_snapPoints[snapPointIndex].State = SnapPointState.Snapped;

        snapPart.m_snapPoints[m_snapPoints[snapPointIndex].SnappedPartIndex].State = SnapPointState.Snapped;
        snapPart.m_snapPoints[m_snapPoints[snapPointIndex].SnappedPartIndex].SnappedPart = this;

        snapPart.transform.SetParent(transform.parent);
        Vector3 thisSnapPoint = m_snapPoints[snapPointIndex].Position;
        Vector3 otherSnapPoint = snapPart.m_snapPoints[m_snapPoints[snapPointIndex].SnappedPartIndex].Position;
        Vector3 snapPosition = transform.TransformPoint(thisSnapPoint - otherSnapPoint);
        snapPart.transform.SetPositionAndRotation(snapPosition, transform.rotation);
        Destroy(snapPart.rigidbody2D);
    }
}
