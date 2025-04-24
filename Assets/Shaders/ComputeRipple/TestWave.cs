using NUnit.Framework.Internal;
using UnityEngine;

public class TestWave : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer;
    [SerializeField]
    private LineRenderer speedLineRenderer;
    [SerializeField]
    private int points = 30;
    [SerializeField]
    private float segentLength = 0.3f;

    [SerializeField]
    private float acceleration = 1f;
    [SerializeField]
    private float SpeedDamping = 1f;
    [SerializeField]
    private float heightDamping = 1f;

    [SerializeField]
    private float transferFactor = 0.5f;

    private RipplePoints[] m_ripplePoints;
    private RipplePoints[] m_ripplePoints2;
    private bool m_firstToSecond = true;
    private Vector3[] m_positions;
    private Vector3[] m_speedPositions;


    void Awake()
    {
        m_ripplePoints = new RipplePoints[points];
        m_ripplePoints2 = new RipplePoints[points];
        m_positions = new Vector3[points];
        m_speedPositions = new Vector3[points];
        for (int i = 0; i < points; i++)
        {
            m_ripplePoints[i].height = 0;
            m_ripplePoints[i].speed = 0;
            m_positions[i] = new Vector3(i * segentLength, 0, 0);
            m_speedPositions[i] = new Vector3(i * segentLength, 0, 0);
        }

        m_ripplePoints[points / 2].speed = 1.0f;
        m_ripplePoints[points / 2].height = 1.0f;

        // m_ripplePoints[points / 2 + 1].speed = 10.0f;
        // m_ripplePoints[points / 2 + 1].height = 1.0f;

        lineRenderer.positionCount = points;
        speedLineRenderer.positionCount = points;
        UpdateLine(ref m_ripplePoints);
    }

    void Update()
    {
        UpdatePositionTo(ref m_ripplePoints, ref m_ripplePoints);
        UpdateLine(ref m_ripplePoints);
        // if (m_firstToSecond)
        // {
        //     UpdatePositionTo(ref m_ripplePoints, ref m_ripplePoints2);
        //     UpdateLine(ref m_ripplePoints2);
        //     m_firstToSecond = false;
        // }
        // else
        // {
        //     UpdatePositionTo(ref m_ripplePoints2, ref m_ripplePoints);
        //     UpdateLine(ref m_ripplePoints);
        //     m_firstToSecond = true;
        // }
    }

    void UpdatePositionTo(ref RipplePoints[] ripplePointsFrom, ref RipplePoints[] ripplePointsTo)
    {
        for (int i = 0; i < points; i++)
        {
            ripplePointsTo[i].height = ripplePointsFrom[i].height;
            ripplePointsTo[i].speed = ripplePointsFrom[i].speed;
        }

        for (int i = 1; i < points - 1; i++)
        {
            Test3(ref ripplePointsFrom, ref ripplePointsTo, i);
        }
    }

    void Test1(ref RipplePoints[] ripplePointsFrom, ref RipplePoints[] ripplePointsTo, int i)
    {
        if (ripplePointsFrom[i].speed > 0)
        {
            float amount = Mathf.Clamp(ripplePointsFrom[i].speed * Time.deltaTime, acceleration * Time.deltaTime, ripplePointsFrom[i].speed);
            if (i < points - 1)
            {
                ripplePointsTo[i + 1].speed = ripplePointsTo[i + 1].speed + amount;
            }

            bool frontIsBigger = false;
            if (i > 0)
            {
                frontIsBigger = ripplePointsTo[i - 1].speed > ripplePointsTo[i].speed;
            }

            if (!frontIsBigger)
                ripplePointsTo[i].speed = ripplePointsTo[i].speed - amount;
        }
    }

    void Test2(ref RipplePoints[] ripplePointsFrom, ref RipplePoints[] ripplePointsTo, int i)
    {
        if (ripplePointsFrom[i].speed > 0)
        {
            bool frontIsBigger = false;
            if (i > 0)
            {
                frontIsBigger = ripplePointsTo[i - 1].height >= ripplePointsTo[i].height;
            }

            float amount = Mathf.Min(ripplePointsFrom[i].speed * Time.deltaTime, ripplePointsFrom[i].height);
            if (i < points - 1)
            {
                ripplePointsTo[i + 1].height = ripplePointsTo[i + 1].height + amount;

                if (frontIsBigger)
                    ripplePointsTo[i + 1].speed = ripplePointsTo[i].speed;
            }

            if (!frontIsBigger)
                ripplePointsTo[i].height = ripplePointsTo[i].height - amount;
        }
    }

    void Test3(ref RipplePoints[] ripplePointsFrom, ref RipplePoints[] ripplePointsTo, int i)
    {
        float heightSum = 0;
        // if (ripplePointsFrom[i - 1].speed > 0)
        heightSum += ripplePointsFrom[i - 1].height;
        // if (ripplePointsFrom[i + 1].speed < 0)
        heightSum += ripplePointsFrom[i + 1].height;
        float force = heightSum - 2 * ripplePointsFrom[i].height;
        float vel = ripplePointsFrom[i].speed + force * transferFactor;
        vel = Mathf.MoveTowards(vel, 0, SpeedDamping * Time.deltaTime);
        float height = ripplePointsFrom[i].height + vel * Time.deltaTime;
        height = Mathf.MoveTowards(height, 0, heightDamping * Time.deltaTime);

        ripplePointsTo[i].speed = vel;
        ripplePointsTo[i].height = height;

        // float h = ripplePointsFrom[i].height;
        // float v = ripplePointsFrom[i].speed;

        // if (v > 0) {
        //     ripplePointsFrom[i+1].height += h * v * transferFactor;
        // } else if (v < 0) {
        //     ripplePointsFrom[i-1].height += h * (-v) * transferFactor;
        // }

        // // Optional: decay own height
        // ripplePointsTo[i].height += ripplePointsFrom[i].height * (1.0f - transferFactor);
    }

    void UpdateLine(ref RipplePoints[] ripplePoints)
    {
        for (int i = 0; i < points; i++)
        {
            m_positions[i].y = ripplePoints[i].height;
            m_speedPositions[i].y = ripplePoints[i].speed;
        }
        lineRenderer.SetPositions(m_positions);
        speedLineRenderer.SetPositions(m_speedPositions);
    }


    [System.Serializable]
    public struct RipplePoints
    {
        public float height;
        public float speed;
    }
}
