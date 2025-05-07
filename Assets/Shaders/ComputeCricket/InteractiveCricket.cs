using UnityEngine;

public class InteractiveCricket : MonoBehaviour
{
    [SerializeField]
    Mesh mesh;
    [SerializeField]
    Material material;
    [SerializeField]
    int cricketCount = 1000;
    [SerializeField]
    Vector3 spawnAreaStart, spawnAreaEnd;

    [SerializeField]
    ComputeShader computeShader;

    [SerializeField]
    Transform playerTransform;
    [SerializeField]
    Vector3 playerOffset;
    [SerializeField]
    float affectedRadius = 5f;

    [SerializeField]
    Vector2 jumpFocre = new Vector2(5, 5);
    [SerializeField]
    float gravity = -9.81f;

    ComputeBuffer m_cricketBuffer;
    int m_kernelID;
    int m_groupSizeX;

    RenderParams m_renderParams;
    GraphicsBuffer argsBuffer;

    int m_shader_deltaTime;
    int m_shader_interactionPosition;

    Vector3 m_interactionPosition;

    void Awake()
    {
        m_shader_deltaTime = Shader.PropertyToID("deltaTime");
        m_shader_interactionPosition = Shader.PropertyToID("interactionPosition");

        InitBuffer();
        InitComputeShader();

        // material = new Material(material);

        m_renderParams = new RenderParams(material);
        m_renderParams.worldBounds = new Bounds(Vector3.zero, 1000 * Vector3.one);
        material.SetBuffer("cricketBuffer", m_cricketBuffer);
    }

    void InitBuffer()
    {
        Cricket[] crickets = new Cricket[cricketCount];

        for (int i = 0; i < cricketCount; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(spawnAreaStart.x, spawnAreaEnd.x),
                Random.Range(spawnAreaStart.y, spawnAreaEnd.y),
                Random.Range(spawnAreaStart.z, spawnAreaEnd.z)
            );

            crickets[i] = new Cricket
            {
                position = randomPos,
                velocity = new Vector3(90, 0, 0),
            };
        }

        m_cricketBuffer = new ComputeBuffer(cricketCount, sizeof(float) * 6);
        m_cricketBuffer.SetData(crickets);

        argsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        GraphicsBuffer.IndirectDrawIndexedArgs[] data = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        data[0].indexCountPerInstance = mesh.GetIndexCount(0);
        data[0].instanceCount = (uint)cricketCount;
        argsBuffer.SetData(data);

        material.SetBuffer("cricketsBuffer", m_cricketBuffer);
    }

    void InitComputeShader()
    {
        m_kernelID = computeShader.FindKernel("InteractiveCricket");
        computeShader.SetBuffer(m_kernelID, "cricketBuffer", m_cricketBuffer);

        uint threadsX, threadsY, threadsZ;
        computeShader.GetKernelThreadGroupSizes(m_kernelID, out threadsX, out threadsY, out threadsZ);
        m_groupSizeX = Mathf.CeilToInt((float)cricketCount / (float)threadsX);

        computeShader.SetFloat("interactionRadius", affectedRadius);
        computeShader.SetVector("jumpParameters", new Vector3(jumpFocre.x, jumpFocre.y, gravity));
    }

    void Update()
    {
        InteractWithPlayer();

        computeShader.SetVector(m_shader_interactionPosition, m_interactionPosition);
        computeShader.SetFloat(m_shader_deltaTime, Time.deltaTime);
        computeShader.Dispatch(m_kernelID, m_groupSizeX, 1, 1);

        Draw();
    }

    void InteractWithPlayer()
    {
        m_interactionPosition.x = playerTransform.position.x + playerOffset.x;
        m_interactionPosition.y = playerTransform.position.y + playerOffset.y;
        m_interactionPosition.z = playerTransform.position.z + playerOffset.z;
    }

    void Draw()
    {
    //     Graphics.RenderPrimitives(m_renderParams, MeshTopology.Points, 1, cricketCount);
        Graphics.RenderMeshIndirect(m_renderParams, mesh, argsBuffer );
    }
    
    void OnDestroy()
    {
        if (m_cricketBuffer != null)
        {
            m_cricketBuffer.Release();
            m_cricketBuffer = null;
        }
    }

    public struct Cricket
    {
        public Vector3 position;
        public Vector3 velocity;
    }
}
