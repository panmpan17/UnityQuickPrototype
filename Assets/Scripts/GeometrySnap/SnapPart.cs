using System.Collections.Generic;
using UnityEngine;


public class SnapPart : MonoBehaviour
{
    [SerializeField]
    private SnapePointSetting[] m_snapPoints;
    public SnapePointSetting[] SnapSettings => m_snapPoints;
    [SerializeField]
    private AnimationCurve attractForceCurve;

    public SnapController SnapController { get; set; }

    [SerializeField]
    private Rigidbody2D m_rigidbody2D;
    public Rigidbody2D Rigidbody2D {
        get {
            if (!m_rigidbody2D)
                m_rigidbody2D = GetComponent<Rigidbody2D>();
            return m_rigidbody2D;
        }
    }

    private bool m_isAttracting = false;

    void Awake()
    {
        if (!m_rigidbody2D)
            m_rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void OnDrawGizmosSelected()
    {
        if (m_snapPoints == null)
            return;

        Gizmos.color = Color.blue;
        for (int i = 0; i < m_snapPoints.Length; i++)
        {
            Vector3 point = transform.TransformPoint(m_snapPoints[i].Position);
            Gizmos.DrawSphere(point, 0.1f);

            if (m_snapPoints[i].IsSlot)
            {
                Gizmos.DrawLine(point, point + transform.TransformDirection(-m_snapPoints[i].LookDirection));
            }
            else
            {
                Gizmos.DrawLine(point, point + transform.TransformDirection(m_snapPoints[i].LookDirection));
            }
        }
    }
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
}
