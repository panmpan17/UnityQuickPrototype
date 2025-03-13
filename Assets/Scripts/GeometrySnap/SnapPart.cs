using System.Collections.Generic;
using MPack;
using UnityEngine;


public class SnapPart : MonoBehaviour
{
    [SerializeField]
    private SnapePointSetting[] m_snapPoints;
    public SnapePointSetting[] SnapSettings => m_snapPoints;

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

    [SerializeField]
    private RigibodyAttractParameter attractParameter;
    private float m_shakeTimer;
    private bool m_isAttracting = false;

    void Awake()
    {
        if (!m_rigidbody2D)
            m_rigidbody2D = GetComponent<Rigidbody2D>();
        
        for (int i = 0; i < m_snapPoints.Length; i++)
        {
            m_snapPoints[i].LookDirationAngle = Vector2.SignedAngle(Vector2.right, m_snapPoints[i].LookDirection);
        }
    }

    void Update()
    {
        if (m_isAttracting)
        {
            if (attractParameter.TimerProgress(m_shakeTimer) < 1)
            {
                m_shakeTimer += Time.deltaTime;
                attractParameter.GetShakeValues(attractParameter.TimerProgress(m_shakeTimer), out float speed, out float amplitude);
                transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time % 1 * speed) * amplitude);
            }
        }
        else
        {
            if (m_shakeTimer > 0)
            {
                m_shakeTimer -= Time.deltaTime;
                if (m_shakeTimer > 0)
                {
                    attractParameter.GetShakeValues(attractParameter.TimerProgress(m_shakeTimer), out float speed, out float amplitude);
                    transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time % 1 * speed) * amplitude);
                }
            }
        }
    }

    public void PullTowards(Vector3 pullTo, int snapPointIndex, float attractForce, float maxDistance, AnimationCurve curve)
    {
        if (!m_isAttracting)
        {
            m_isAttracting = true;
        }

        Vector3 delta = pullTo - transform.position;
        
        // float targetAngularVelocity = Vector2.Angle(delta, transform.TransformDirection(m_snapPoints[snapPointIndex].LookDirection));
        // m_rigidbody2D.angularVelocity += targetAngularVelocity;

        if (m_shakeTimer < attractParameter.ShakeTimer)
        {
            return;
        }

        float distance = delta.magnitude;
        delta /= distance;
        float force = curve.Evaluate(distance / maxDistance) * attractForce;

        m_rigidbody2D.linearVelocity = delta * force;
    }

    public void StopAttracting()
    {
        m_isAttracting = false;
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
    [System.NonSerialized]
    public float LookDirationAngle;

    public float AttractDistance;
    public float AttractStrength;
    public int Indetifier;
    public bool IsSlot;
}
