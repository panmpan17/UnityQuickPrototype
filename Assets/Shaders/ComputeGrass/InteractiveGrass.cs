using System.Security.Principal;
using UnityEngine;
using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;

public class InteractiveGrass : MonoBehaviour
{
    [SerializeField]
    private Material material;
    [SerializeField]
    private int grassCount = 1000;
    [SerializeField]
    private Vector3 spawnAreaStart, spawnAreaEnd;

    [SerializeField]
    private float affectedRadius = 5f;
    [SerializeField]
    private bool affectedRadiusFallOff = true;

    [SerializeField]
    private ComputeShader computeShader;

    [SerializeField]
    private MeshRenderer meshRenderer;

    ComputeBuffer m_grassBuffer;

    RenderParams m_renderParams;
    Camera m_mainCamera;

    int m_shader_UnityCameraRotation;
    int m_shader_UnityTime;
    int m_shader_mouseInteractionIndex;
    int m_shader_deltaTime;
    int m_shader_mouseInteractionPosition;

    int m_kernelID;
    int m_groupSizeX;

    InputScheme m_inputScheme;
    int m_mouseIndex = -1;
    bool m_mouseDown = false;

    Vector4 m_mousePosition;

    void Awake()
    {
        m_inputScheme = new InputScheme();
        m_inputScheme.Player.Click.performed += OnClickPerformed;
        m_inputScheme.Player.Click.canceled += OnClickCanceled;

        m_mainCamera = Camera.main;
        m_renderParams = new RenderParams(material);
        m_renderParams.worldBounds = new Bounds(Vector3.zero, 1000 * Vector3.one);

        InitBuffer();

        material.SetBuffer("grassPointBuffer", m_grassBuffer);

        m_kernelID = computeShader.FindKernel("CSMain");
        computeShader.SetBuffer(m_kernelID, "grassPointBuffer", m_grassBuffer);
        computeShader.SetInt("bufferSize", grassCount);

        uint threadsX, threadsY, threadsZ;
        computeShader.GetKernelThreadGroupSizes(m_kernelID, out threadsX, out threadsY, out threadsZ);
        m_groupSizeX = Mathf.CeilToInt((float)grassCount / (float)threadsX);

        computeShader.SetFloat("mouseInteractionRadius", affectedRadius);
        computeShader.SetBool("mouseInteractionFalloff", affectedRadiusFallOff);

        m_shader_UnityCameraRotation = Shader.PropertyToID("unity_CameraRotation");
        m_shader_UnityTime = Shader.PropertyToID("unity_Time");
        m_shader_mouseInteractionIndex = Shader.PropertyToID("mouseInteractionIndex");
        m_shader_deltaTime = Shader.PropertyToID("deltaTime");
        m_shader_mouseInteractionPosition = Shader.PropertyToID("mouseInteractionPosition");
    }

    void OnEnable()
    {
        m_inputScheme.Enable();
    }

    void OnDisable()
    {
        m_inputScheme.Disable();
    }

    private void InitBuffer()
    {
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
    }

    void Draw()
    {
        Quaternion rotation = m_mainCamera.transform.rotation;
        material.SetVector(m_shader_UnityCameraRotation, new Vector4(rotation.x, rotation.y, rotation.z, rotation.w));
        material.SetFloat(m_shader_UnityTime, Time.time);
        Graphics.RenderPrimitives(m_renderParams, MeshTopology.Triangles, 3, grassCount);
    }


    void Update()
    {
        if (m_mouseDown)
        {
            Vector2 screenPosition = m_inputScheme.Player.PointerInput.ReadValue<Vector2>();
            Ray ray = m_mainCamera.ScreenPointToRay(screenPosition);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                m_mousePosition.x = hit.point.x;
                m_mousePosition.y = hit.point.y;
                m_mousePosition.z = hit.point.z;
                m_mousePosition.w = 1;
            }
            else
            {
                m_mousePosition.w = 0;
            }
        }
        else
        {
            m_mousePosition.w = 0;
        }

        computeShader.SetVector(m_shader_mouseInteractionPosition, m_mousePosition);
        computeShader.SetFloat(m_shader_deltaTime, Time.deltaTime);
        computeShader.SetInt(m_shader_mouseInteractionIndex, m_mouseIndex);
        computeShader.Dispatch(m_kernelID, m_groupSizeX, 1, 1);

        Draw();
    }

    void OnClickPerformed(CallbackContext callbackContext)
    {
        m_mouseDown = true;
    }

    void OnClickCanceled(CallbackContext callbackContext)
    {
        m_mouseDown = false;
    }

    public struct GrassPoint
    {
        public Vector3 position;
        public float height;
    }
}
