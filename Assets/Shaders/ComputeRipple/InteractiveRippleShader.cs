using UnityEngine;

public class InteractiveRippleShader : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    private ComputeShader computeShader;

    [SerializeField]
    private int rippleVerticleCount = 128;

    struct RipplePoint
    {
        public float height;
        public Vector3 velocity;
    }

    private int m_kernelID, m_groupSizeX;
    private ComputeBuffer m_rippleBuffer;
    private Material m_material;

    void Start()
    {
        m_material = meshRenderer.material;
        Initialization();
    }

    void Initialization()
    {
        RipplePoint[] ripplePointArray = new RipplePoint[rippleVerticleCount];

        for (int i = 0; i < rippleVerticleCount; i++)
        {
            ripplePointArray[i].height = 0;//(float)i / rippleVerticleCount;
            ripplePointArray[i].velocity = Vector3.zero;
        }

        ripplePointArray[64].height = 1.0f;
        ripplePointArray[64].velocity = new Vector3(1, 1, 1);

        m_rippleBuffer = new ComputeBuffer(rippleVerticleCount, 4 * sizeof(float));
        m_rippleBuffer.SetData(ripplePointArray);

        m_kernelID = computeShader.FindKernel("CSMain");
        uint threadsX, threadsY, threadsZ;
        computeShader.GetKernelThreadGroupSizes(m_kernelID, out threadsX, out threadsY, out threadsZ);
        // Debug.Log("threadsX: " + threadsX + " threadsY: " + threadsY + " threadsZ: " + threadsZ);
        m_groupSizeX = Mathf.CeilToInt((float)rippleVerticleCount / (float)threadsX);

        computeShader.SetBuffer(m_kernelID, "rippleBuffer", m_rippleBuffer);
        m_material.SetBuffer("rippleBuffer", m_rippleBuffer);
    }

    void Update()
    {
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.Dispatch(m_kernelID, m_groupSizeX, 1, 1);
    }
}
