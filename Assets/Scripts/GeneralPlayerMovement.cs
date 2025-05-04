using UnityEngine;
using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;


public class GeneralPlayerMovement : MonoBehaviour
{
    [SerializeField]
    Animator animator;

    [SerializeField]
    float moveSpeed;
    [SerializeField]
    float rotationAngleOffset = -90f;
    [SerializeField]
    float rotateTargetSpeed = 5f;

    [SerializeField]
    AudioSource audioSource;
    [SerializeField]
    private AudioClipSet stepAudioClipSet;

    InputScheme m_inputScheme;
    Vector2 m_moveInputAxis;
    int m_walkngAimatorID;
    bool m_isWalking;
    

    void Awake()
    {
        m_inputScheme = new InputScheme();
        m_inputScheme.Player.Move.performed += OnMoveChanges;
        m_inputScheme.Player.Move.canceled += OnMoveChanges;

        m_walkngAimatorID = Animator.StringToHash("Walking");
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
        if (m_isWalking)
        {
            float angle = Mathf.Atan2(m_moveInputAxis.y, -m_moveInputAxis.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, angle + rotationAngleOffset, 0);
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, rotateTargetSpeed * Time.deltaTime);

            Vector3 position = transform.position;
            position += transform.forward * moveSpeed * Time.deltaTime;
            // position.z += transform.forward.y * moveSpeed * Time.deltaTime;
            transform.position = position;
        }
    }

    void OnMoveChanges(CallbackContext callbackContext)
    {
        m_moveInputAxis = callbackContext.ReadValue<Vector2>();
        m_isWalking = m_moveInputAxis.sqrMagnitude > 0.01f;

        if (animator)
        {
            animator.SetBool(m_walkngAimatorID, m_isWalking);
        }
    }

    public void OnAnimationTriggerd(int type)
    {
        if (type == 0)
        {
            audioSource.PlayOneShot(stepAudioClipSet);
        }
    }
}
