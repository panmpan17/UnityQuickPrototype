using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;
using MPack;

public class TopStrikePlayer : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private TransformPointer playerPointer;

    [Header("Move")]
    [SerializeField]
    private float moveSpeed;

    [SerializeField]
    private float dashSpeed;
    [SerializeField]
    private Timer dashTimer;

    [Header("Facing Direction")]
    [SerializeField]
    private SpriteRenderer facingDirection;
    [SerializeField]
    private Color nonActiveColor;
    private Color _activeColor;

    private Transform _facingDirectionTransform;
    private Vector2 faceDirectionOffset;
    [SerializeField]
    private float faceDirectionAngleOffset;

    [Header("Lock On")]
    [SerializeField]
    private LayerMask layerMask;
    [SerializeField]
    private float raycastDistance;

    [SerializeField]
    private TransformPointer lockOnTarget;
    private Collider2D _lockOnCollider;


    private Rigidbody2D _rigidbody2D;

    private InputScheme _inputScheme;
    // private Vector2 _mouseStartPosition;
    private Vector2 _mouseCurrentPosition;
    private Vector2 MouseCurrentWorldPosition => mainCamera.ScreenToWorldPoint(_mouseCurrentPosition);
    private Vector2 _moveInput;


    void Awake()
    {
        _activeColor = facingDirection.color;
        _facingDirectionTransform = facingDirection.transform;
        faceDirectionOffset = _facingDirectionTransform.localPosition;

        _inputScheme = new InputScheme();

        _inputScheme.Player.PointerInput.performed += OnPointerInputChanged;
        _inputScheme.Player.PointerInput.canceled += OnPointerInputChanged;

        _inputScheme.Player.Move.started += OnMoveChanged;
        _inputScheme.Player.Move.performed += OnMoveChanged;
        _inputScheme.Player.Move.canceled += OnMoveChanged;

        _inputScheme.Player.Attack.performed += OnAttack;

        _rigidbody2D = GetComponent<Rigidbody2D>();

        playerPointer.Target = transform;
    }

    void OnPointerInputChanged(CallbackContext callbackContext)
    {
        _mouseCurrentPosition = callbackContext.ReadValue<Vector2>();
    }

    void OnMoveChanged(CallbackContext callbackContext)
    {
        Vector2 move = callbackContext.ReadValue<Vector2>();
        _moveInput = move;
    }

    void OnAttack(CallbackContext callbackContext)
    {
        if (dashTimer.Running)
            return;
        if (_lockOnCollider == null)
            return;

        Vector2 delta = MouseCurrentWorldPosition - (Vector2)transform.position;
        _rigidbody2D.linearVelocity = delta.normalized * dashSpeed;
        dashTimer.Reset();
    }

    void OnEnable()
    {
        _inputScheme.Enable();
    }

    void OnDisable()
    {
        _inputScheme.Disable();
    }

    void Update()
    {
        Vector2 delta = MouseCurrentWorldPosition - (Vector2)transform.position;
        UpdatePointerPosition(delta);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, delta, raycastDistance, layerMask);
        if (hit.collider)
        {
            if (_lockOnCollider != hit.collider)
            {
                _lockOnCollider = hit.collider;
                lockOnTarget.Target = hit.collider.transform;
            }
            facingDirection.color = _activeColor;
        }
        else
        {
            _lockOnCollider = null;
            lockOnTarget.Target = null;
            facingDirection.color = nonActiveColor;
        }

        if (dashTimer.Running)
        {
            dashTimer.Update();
            if (dashTimer.UpdateEnd)
            {
                dashTimer.Running = false;
            }
        }
        else
        {
            _rigidbody2D.linearVelocity = _moveInput * moveSpeed;
        }
    }

    void UpdatePointerPosition(Vector2 delta)
    {
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

        Quaternion rotation = Quaternion.Euler(0, 0, angle + faceDirectionAngleOffset);
        _facingDirectionTransform.localPosition = rotation * faceDirectionOffset;
        _facingDirectionTransform.rotation = rotation;
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, raycastDistance);
    }
}
