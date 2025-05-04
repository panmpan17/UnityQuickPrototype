using System.Security.Principal;
using Unity.VisualScripting;
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
    private WindManager windManager;
    [SerializeField]
    private Vector2 maxWindSpeed = new Vector2(2, 2);
    [SerializeField]
    private Transform playerTransform;
    [SerializeField]
    private Vector3 playerOffset;

    ComputeBuffer m_grassBuffer;
    int m_kernelID;
    int m_groupSizeX;

    RenderParams m_renderParams;
    Camera m_mainCamera;

    int m_shader_UnityCameraRotation;
    int m_shader_UnityTime;
    int m_shader_mouseInteractionIndex;
    int m_shader_deltaTime;
    int m_shader_mouseInteractionPosition;
    int m_shader_windSpeed;

    InputScheme m_inputScheme;
    int m_mouseIndex = -1;
    bool m_mouseDown = false;
    Vector4 m_mousePosition;

    Vector4 m_windFactor;

    void Awake()
    {
        m_inputScheme = new InputScheme();
        m_inputScheme.Player.Click.performed += OnClickPerformed;
        m_inputScheme.Player.Click.canceled += OnClickCanceled;

        m_mainCamera = Camera.main;

        m_shader_UnityCameraRotation = Shader.PropertyToID("unity_CameraRotation");
        m_shader_UnityTime = Shader.PropertyToID("unity_Time");
        m_shader_mouseInteractionIndex = Shader.PropertyToID("mouseInteractionIndex");
        m_shader_deltaTime = Shader.PropertyToID("deltaTime");
        m_shader_mouseInteractionPosition = Shader.PropertyToID("mouseInteractionPosition");
        m_shader_windSpeed = Shader.PropertyToID("_WindSpeed");

        InitBuffer();
        InitComputeShader();

        material = new Material(material);

        m_renderParams = new RenderParams(material);
        m_renderParams.worldBounds = new Bounds(Vector3.zero, 1000 * Vector3.one);
        material.SetBuffer("grassPointBuffer", m_grassBuffer);
        m_windFactor = material.GetVector(m_shader_windSpeed);
    }

    void Start()
    {
        if (windManager == null)
        {
            windManager = WindManager.Instance;
        }
    }

    void InitBuffer()
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
                height = Random.Range(0.5f, 2.0f),
                rotation = new Vector4(0, 0, 0, 1),
            };
        }

        m_grassBuffer = new ComputeBuffer(grassCount, sizeof(float) * 8);
        m_grassBuffer.SetData(grassPoints);
    }

    void InitComputeShader()
    {
        m_kernelID = computeShader.FindKernel("CSMain");
        computeShader.SetBuffer(m_kernelID, "grassPointBuffer", m_grassBuffer);
        computeShader.SetInt("bufferSize", grassCount);

        uint threadsX, threadsY, threadsZ;
        computeShader.GetKernelThreadGroupSizes(m_kernelID, out threadsX, out threadsY, out threadsZ);
        m_groupSizeX = Mathf.CeilToInt((float)grassCount / (float)threadsX);

        computeShader.SetFloat("mouseInteractionRadius", affectedRadius);
        computeShader.SetBool("mouseInteractionFalloff", affectedRadiusFallOff);
        // computeShader.SetTexture(m_kernelID, "samplerNoiseTexture", noiseTexture);
    }

    void OnEnable()
    {
        m_inputScheme.Enable();
    }

    void OnDisable()
    {
        m_inputScheme.Disable();
    }

    void Update()
    {
        if (playerTransform)
        {
            InteractWithPlayer();
        }
        else
        {
            InteractWithMouse();
        }

        computeShader.SetVector(m_shader_mouseInteractionPosition, m_mousePosition);
        computeShader.SetFloat(m_shader_deltaTime, Time.deltaTime);
        computeShader.SetInt(m_shader_mouseInteractionIndex, m_mouseIndex);
        computeShader.SetFloat(m_shader_UnityTime, Time.time);
        computeShader.Dispatch(m_kernelID, m_groupSizeX, 1, 1);

        Draw();
    }

    void InteractWithMouse()
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
    }

    void InteractWithPlayer()
    {
        m_mousePosition.x = playerTransform.position.x + playerOffset.x;
        m_mousePosition.y = playerTransform.position.y + playerOffset.y;
        m_mousePosition.z = playerTransform.position.z + playerOffset.z;
        m_mousePosition.w = 1;
    }

    void Draw()
    {
        // m_windFactor.x = windManager.CurrentWindSpeed.x;
        // m_windFactor.y = windManager.CurrentWindSpeed.y;
        m_windFactor.z = -Mathf.Clamp(windManager.CurrentWindSpeed.x, -maxWindSpeed.x, maxWindSpeed.x);
        m_windFactor.w = Mathf.Clamp(windManager.CurrentWindSpeed.y, -maxWindSpeed.y, maxWindSpeed.y);

        Quaternion rotation = m_mainCamera.transform.rotation;
        material.SetVector(m_shader_UnityCameraRotation, new Vector4(rotation.x, rotation.y, rotation.z, rotation.w));
        material.SetVector(m_shader_windSpeed, m_windFactor);
        material.SetFloat(m_shader_UnityTime, Time.time);
        Graphics.RenderPrimitives(m_renderParams, MeshTopology.Triangles, 3, grassCount);
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
        public Vector4 rotation;
    }
}
