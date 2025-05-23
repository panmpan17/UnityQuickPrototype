using UnityEngine;
using UnityEngine.InputSystem;

public class GeometryPlayerControl : MonoBehaviour
{
    [SerializeField]
    private SnapController snapController;

    [SerializeField]
    private float rotateSpeed = 1.0f;
    private float m_currentRotateSpeed = 0.0f;
    [SerializeField]
    private float[] rotateSpeedDecrease;
    [SerializeField]
    private float moveSpeed = 1.0f;
    private float m_currentMoveSpeed = 0.0f;
    [SerializeField]
    private float[] moveSpeedDecrease;

    [SerializeField]
    private Transform shapeRenderer;
    [SerializeField]
    private RigibodyAttractParameter releaseParameter;
    private float m_shakeTimer;

    private InputScheme m_inputScheme;
    private bool m_isReleasing = false;

    void Awake()
    {
        SetUpInputs();

        if (snapController)
        {
            snapController.OnSnapPartAdded += OnSnapPartChanges;
            snapController.OnSnapPartRemoved += OnSnapPartChanges;
        }
    }

    void Start()
    {
        OnSnapPartChanges();
    }

    void Update()
    {
        UpdateMovement();
        UpdateRelease();
    }

    void UpdateMovement()
    {
        Vector2 move = m_inputScheme.Player.Move.ReadValue<Vector2>();

        transform.position += new Vector3(move.x, move.y, 0) * m_currentMoveSpeed * Time.deltaTime;

        if (move == Vector2.zero)
        {
            return;
        }
        float zRotation = Mathf.Atan2(move.y, move.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, zRotation);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, m_currentRotateSpeed * Time.deltaTime);
    }

    void UpdateRelease()
    {
        if (m_isReleasing)
        {
            if (releaseParameter.TimerProgress(m_shakeTimer) < 1)
            {
                m_shakeTimer += Time.deltaTime;

                if (m_shakeTimer > releaseParameter.ShakeTimer)
                {
                    snapController.ReleaseOtherParts();
                    shapeRenderer.localRotation = Quaternion.identity;
                    m_isReleasing = false;
                }
                else
                {
                    releaseParameter.GetShakeValues(releaseParameter.TimerProgress(m_shakeTimer), out float speed, out float amplitude);
                    shapeRenderer.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time % 1 * speed) * amplitude);
                }
            }
        }
        else
        {
            if (m_shakeTimer > 0)
            {
                m_shakeTimer -= Time.deltaTime;
                if (m_shakeTimer > 0)
                {
                    releaseParameter.GetShakeValues(releaseParameter.TimerProgress(m_shakeTimer), out float speed, out float amplitude);
                    shapeRenderer.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time % 1 * speed) * amplitude);
                }
            }
        }
    }


    void OnSnapPartChanges()
    {
        int count = snapController.SnapPartCount;
        m_currentMoveSpeed = moveSpeed - moveSpeedDecrease[Mathf.Clamp(count, 0, moveSpeedDecrease.Length - 1)];
        m_currentRotateSpeed = rotateSpeed - rotateSpeedDecrease[Mathf.Clamp(count, 0, rotateSpeedDecrease.Length - 1)];
    }


#region Input
    void SetUpInputs()
    {
        m_inputScheme = new InputScheme();
        m_inputScheme.Enable();

        m_inputScheme.Player.Attack.performed += OnAttackPerfermed;
        m_inputScheme.Player.Attack.canceled += OnAttackCanceled;

        m_inputScheme.Player.Shift.performed += OnShiftPerfermed;
        m_inputScheme.Player.Shift.canceled += OnShiftCanceled;
    }

    void OnAttackPerfermed(InputAction.CallbackContext context)
    {
        snapController.StartAttractingOtherParts();
    }

    void OnAttackCanceled(InputAction.CallbackContext context)
    {
        snapController.StopAttracting();
    }

    void OnShiftPerfermed(InputAction.CallbackContext context)
    {
        m_isReleasing = true;
    }

    void OnShiftCanceled(InputAction.CallbackContext context)
    {
        m_isReleasing = false;
    }
#endregion
}
