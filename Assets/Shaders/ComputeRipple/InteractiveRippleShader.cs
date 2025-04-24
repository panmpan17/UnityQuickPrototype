using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;

public class InteractiveRippleShader : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    private ComputeShader computeShader;

    [SerializeField]
    private int rippleVerticleCount = 128;
    
    [SerializeField]
    private float forceTransferFactor = 0.5f;
    [SerializeField]
    private float speedDamping = 0.1f;
    [SerializeField]
    private float heightDamping = 0.1f;

    [SerializeField]
    private float affectedRadius = 5f;
    [SerializeField]
    private bool affectedRadiusFallOff = true;

    // [SerializeField]
    private InputScheme m_inputScheme;

    struct RipplePoint
    {
        public float height;
        public float velocity;
    }

    private int m_kernelID, m_groupSizeX;
    private ComputeBuffer m_rippleBuffer;
    private Material m_material;

    private int m_mouseIndex = -1;
    private bool m_mouseDown = false;

    private int m_ShaderID_mouseInteractionIndex;
    private int m_ShaderID_deltaTime;

    private Camera m_camera;

    void Awake()
    {
        m_inputScheme = new InputScheme();
        m_inputScheme.Player.Click.performed += OnClickPerformed;
        m_inputScheme.Player.Click.canceled += OnClickCanceled;
        m_camera = Camera.main;
    }

    void Start()
    {
        m_material = meshRenderer.material;
        Initialization();
    }

    void OnEnable()
    {
        m_inputScheme.Enable();
    }

    void OnDisable()
    {
        m_inputScheme.Disable();
    }

    void Initialization()
    {
        RipplePoint[] ripplePointArray = new RipplePoint[rippleVerticleCount * rippleVerticleCount];

        // for (int x = 0; x < rippleVerticleCount; x++)
        // {
        //     for (int y = 0; y < rippleVerticleCount; y++)
        //     {
        //         int index = x + rippleVerticleCount * y;
        //         ripplePointArray[index].height = Random.Range(0f, 0.4f);
        //     }
        // }

        m_rippleBuffer = new ComputeBuffer(rippleVerticleCount * rippleVerticleCount, 2 * sizeof(float));
        m_rippleBuffer.SetData(ripplePointArray);

        m_kernelID = computeShader.FindKernel("CSMain");
        uint threadsX, threadsY, threadsZ;
        computeShader.GetKernelThreadGroupSizes(m_kernelID, out threadsX, out threadsY, out threadsZ);
        m_groupSizeX = Mathf.CeilToInt((float)(rippleVerticleCount * rippleVerticleCount) / (float)threadsX);

        computeShader.SetInt("bufferSize", rippleVerticleCount);
        computeShader.SetBuffer(m_kernelID, "rippleBuffer", m_rippleBuffer);
        computeShader.SetFloat("forceTransferFactor", forceTransferFactor);
        computeShader.SetFloat("speedDamping", speedDamping);
        computeShader.SetFloat("heightDamping", heightDamping);
        computeShader.SetFloat("mouseInteractionRadius", affectedRadius);
        computeShader.SetBool("mouseInteractionFalloff", affectedRadiusFallOff);

        m_ShaderID_mouseInteractionIndex = Shader.PropertyToID("mouseInteractionIndex");
        m_ShaderID_deltaTime = Shader.PropertyToID("deltaTime");;

        m_material.SetBuffer("rippleBuffer", m_rippleBuffer);
        m_material.SetInt("_BufferSize", rippleVerticleCount);
    }

    void Update()
    {
        if (m_mouseDown)
        {
            Vector2 screenPosition = m_inputScheme.Player.PointerInput.ReadValue<Vector2>();
            Ray ray = m_camera.ScreenPointToRay(screenPosition);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 localPosition = meshRenderer.transform.InverseTransformPoint(hit.point);
                int x = Mathf.RoundToInt((localPosition.x + 0.5f) * rippleVerticleCount);
                int y = Mathf.RoundToInt((localPosition.y + 0.5f) * rippleVerticleCount);
                // float x = Mathf.Clamp(localPosition.x / meshRenderer.bounds.size.x * rippleVerticleCount, 0, rippleVerticleCount - 1);
                // float y = Mathf.Clamp(localPosition.y / meshRenderer.bounds.size.y * rippleVerticleCount, 0, rippleVerticleCount - 1);
                m_mouseIndex = x + rippleVerticleCount * y;
                // Debug.Log($"screenPosition: {screenPosition}, x: {x}, y: {y}, m_mouseIndex: {m_mouseIndex}");
            }
            else
            {
                m_mouseIndex = -1;
            }
        }
        else
        {
            m_mouseIndex = -1;
        }

        computeShader.SetInt(m_ShaderID_mouseInteractionIndex, m_mouseIndex);
        computeShader.SetFloat(m_ShaderID_deltaTime, Time.deltaTime);
        computeShader.Dispatch(m_kernelID, m_groupSizeX, 1, 1);
    }

    void OnClickPerformed(CallbackContext callbackContext)
    {
        m_mouseDown = true;
    }

    void OnClickCanceled(CallbackContext callbackContext)
    {
        m_mouseDown = false;
    }
}
