using UnityEngine;

public class TestWave2 : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer;
    [SerializeField]
    private int points = 30;
    [SerializeField]
    private float segentLength = 0.3f;

    // [SerializeField]
    // private float acceleration = 1f;
    [SerializeField]
    private float strength = 5;
    [SerializeField]
    private float heightDamping = 1f;
    [SerializeField]
    private float moveSpeed = 1f;
    [SerializeField]
    private int area = 5;
    [SerializeField]
    private AnimationCurve curve;

    private float m_position;

    private RipplePoints[] m_ripplePoints;
    private Vector3[] m_positions;


    void Awake()
    {
        m_ripplePoints = new RipplePoints[points];
        m_positions = new Vector3[points];
        for (int i = 0; i < points; i++)
        {
            m_ripplePoints[i].height = 0;
            m_positions[i] = new Vector3(i * segentLength, 0, 0);
        }

        m_position = -area;

        lineRenderer.positionCount = points;
        UpdateLine(ref m_ripplePoints);
    }

    void Update()
    {
        m_position += moveSpeed * Time.time;

        // segentLength * points;
        int closestIndex = Mathf.FloorToInt(m_position / segentLength);
        int startIndex = closestIndex - area;
        int endIndex = closestIndex + area;

        for (int i = 0; i < points; i++)
        {
            if (i >= startIndex && i <= endIndex)
            {
                float distance = i - closestIndex;
                float acceleration = curve.Evaluate(1 - (distance / area));
                m_ripplePoints[i].height += acceleration * strength * Time.deltaTime;
            }
            m_ripplePoints[i].height = Mathf.MoveTowards(m_ripplePoints[i].height, 0, heightDamping * Time.deltaTime);
        }

        UpdateLine(ref m_ripplePoints);
    }

    void UpdateLine(ref RipplePoints[] ripplePoints)
    {
        for (int i = 0; i < points; i++)
        {
            m_positions[i].y = ripplePoints[i].height;
        }
        lineRenderer.SetPositions(m_positions);
    }


    [System.Serializable]
    public struct RipplePoints
    {
        public float height;
        // public float speed;
    }
}
