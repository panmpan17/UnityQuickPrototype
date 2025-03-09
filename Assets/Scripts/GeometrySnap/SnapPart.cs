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
