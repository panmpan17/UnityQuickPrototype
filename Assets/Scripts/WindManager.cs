using MPack;
using UnityEngine;

public class WindManager : MonoBehaviour
{
    public static WindManager Instance {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindFirstObjectByType<WindManager>();
            }
            return m_instance;
        }
    }
    private static WindManager m_instance;

    [SerializeField]
    private float maxWindSpeed = 10f;
    [SerializeField]
    private float maxWindChangeAmount = 3f;
    [SerializeField]
    private float minWindSpeed = 0.5f;
    [SerializeField]
    private float windChangeSpeed = 2f;

    [SerializeField]
    private RangeStruct windChangeInterval;
    private Timer m_windChangeTimer;

    [SerializeField]
    private AnimationCurve[] windChangeSpeedCurve;
    private int m_windChangeSpeedCurveIndex;
    private Stopwatch m_windChangeSpeedCurveTimer;

    private Vector2 m_currentWindSpeed;
    private Vector2 m_targetWindSpeed;
    private Vector2 m_currentWindStrength;

    public Vector2 CurrentWindSpeed => m_currentWindSpeed;
    public Vector2 CurrentWindStrength => m_currentWindStrength;


    void RandomWindowTarget()
    {
        m_targetWindSpeed = new Vector2(
            Random.Range(-maxWindSpeed, maxWindSpeed),
            Random.Range(-maxWindSpeed, maxWindSpeed)
        );

        Vector2 delta = m_targetWindSpeed - m_currentWindSpeed;
        if (delta.sqrMagnitude > maxWindChangeAmount * maxWindChangeAmount)
        {
            m_targetWindSpeed = m_currentWindSpeed + delta.normalized * maxWindChangeAmount;
        }

        if (m_targetWindSpeed.sqrMagnitude < minWindSpeed * minWindSpeed)
        {
            m_targetWindSpeed *= minWindSpeed / m_targetWindSpeed.magnitude;
        }

        m_windChangeSpeedCurveIndex = Random.Range(0, windChangeSpeedCurve.Length);
        m_windChangeSpeedCurveTimer.Update();
    }

    void Awake()
    {
        RandomWindowTarget();
        m_windChangeTimer = new Timer(windChangeInterval.PickRandomNumber());
    }

    void Update()
    {
        if (m_windChangeTimer.UpdateEnd)
        {
            RandomWindowTarget();
            m_windChangeTimer.Reset();
            m_windChangeTimer.TargetTime = windChangeInterval.PickRandomNumber();
        }

        m_currentWindSpeed = Vector2.MoveTowards(
            m_currentWindSpeed,
            m_targetWindSpeed,
            Time.deltaTime * (windChangeSpeedCurve[m_windChangeSpeedCurveIndex].Evaluate(m_windChangeSpeedCurveTimer.DeltaTime) * windChangeSpeed)
        );
    }
}
