using MPack;
using UnityEngine;

public class AudioReader : MonoBehaviour
{
    [SerializeField]
    AudioSource audioSource;
    AudioClip audioClip;
    [SerializeField]
    LineRenderer lineRenderer;
    [SerializeField]
    float segmentLength = 0.01f;
    [SerializeField]
    float maxAmplitude = 1.0f;
    // [SerializeField]
    int sampleSize = 1024;
    // [SerializeField]
    // float speedMultiplier = 1.0f;

    Timer m_timer;

    int m_offset = 0;

    float[] m_audioData;
    Vector3[] m_positions;

    void Start()
    {
        audioClip = audioSource.clip;
        sampleSize = Mathf.RoundToInt(audioClip.frequency * 0.02f);
        m_timer = new Timer(0.02f);

        m_audioData = new float[sampleSize];
        m_positions = new Vector3[sampleSize];
        lineRenderer.positionCount = sampleSize;

        Debug.Log("Frequency: " + audioClip.frequency);
        Debug.Log("Length: " + audioClip.length);
        Debug.Log("Samples: " + audioClip.samples);
        Debug.Log("Channels: " + audioClip.channels);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_timer.UpdateEnd)
        {
            m_timer.ResetContinuous();
            m_offset += sampleSize;
            GetData();
            UpdateLine();
        }
    }

    void GetData()
    {
        audioClip.GetData(m_audioData, m_offset);
    }

    void UpdateLine()
    {
        for (int i = 0; i < sampleSize; i++)
        {
            m_positions[i] = new Vector3(i * segmentLength, m_audioData[i] * maxAmplitude, 0);
        }

        lineRenderer.SetPositions(m_positions);
    }
}
