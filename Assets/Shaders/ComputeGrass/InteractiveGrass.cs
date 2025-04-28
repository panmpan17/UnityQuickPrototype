using System.Security.Principal;
using UnityEngine;

public class InteractiveGrass : MonoBehaviour
{
    [SerializeField]
    private Material material;
    [SerializeField]
    private int grassCount = 1000;
    [SerializeField]
    private Vector3 spawnAreaStart, spawnAreaEnd;

    ComputeBuffer m_grassBuffer;

    RenderParams m_renderParams;
    Camera m_mainCamera;

    int m_shader_UnityCameraUp;
    int m_shader_UnityCameraRight;
    int m_shader_UnityTime;

    void Awake()
    {
        m_mainCamera = Camera.main;
        m_renderParams = new RenderParams(material);
        m_renderParams.worldBounds = new Bounds(Vector3.zero, 1000 * Vector3.one);

        GrassPoint[] grassPoints = new GrassPoint[grassCount];

        for (int i = 0; i < grassCount; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(spawnAreaStart.x, spawnAreaEnd.x),
                Random.Range(spawnAreaStart.y, spawnAreaEnd.y),
                Random.Range(spawnAreaStart.z, spawnAreaEnd.z)
            );

            grassPoints[i] = new GrassPoint
            {
                position = randomPos,
                height = Random.Range(0.5f, 2.0f)
            };
        }

        m_grassBuffer = new ComputeBuffer(grassCount, sizeof(float) * 4);
        m_grassBuffer.SetData(grassPoints);

        material.SetBuffer("grassPointBuffer", m_grassBuffer);

        m_shader_UnityCameraUp = Shader.PropertyToID("unity_CameraUp");
        m_shader_UnityCameraRight = Shader.PropertyToID("unity_CameraRight");
        m_shader_UnityTime = Shader.PropertyToID("unity_Time");
    }

    void Update()
    {
        material.SetVector(m_shader_UnityCameraUp, m_mainCamera.transform.up);
        material.SetVector(m_shader_UnityCameraRight, m_mainCamera.transform.right);
        material.SetFloat(m_shader_UnityTime, Time.time);
        Graphics.RenderPrimitives(m_renderParams, MeshTopology.Triangles, 3, grassCount);
    }

    public struct GrassPoint
    {
        public Vector3 position;
        public float height;
    }
}
