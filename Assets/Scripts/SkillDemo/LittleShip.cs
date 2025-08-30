using UnityEngine;
using UnityEngine.InputSystem;
using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;

public class LittleShip : MonoBehaviour
{
    [SerializeField]
    Camera headCamera;
    [SerializeField]
    float rotationSpeed = 100f;
    [SerializeField]
    float speed = 5f;
    [SerializeField]
    float accelerationSpeed = 2f;
    [SerializeField]
    float clamedMagnitude = 10f;
    [SerializeField]
    float dragWhenNotMoving = 0.1f;
    [SerializeField]
    GameObject fireEffect;
    [SerializeField]
    ParticleSystem smokeEffect;
    [SerializeField]
    LineRenderer dragLine;
    [SerializeField]
    Transform dragPoint;

    InputAction m_pointerDown;
    InputAction m_pointerPosition;

    bool m_isPointerDown = false;

    Vector2 m_velocity = Vector2.zero;

    void OnEnable()
    {
        m_pointerDown = new InputAction("PointerDown", binding: "<Pointer>/press");
        m_pointerPosition = new InputAction("PointerPosition", binding: "<Pointer>/position");

        m_pointerDown.performed += onPointerDown;
        m_pointerDown.canceled += onPointerUp;

        m_pointerDown.Enable();
        m_pointerPosition.Enable();

        dragLine.enabled = false;
        dragPoint.position = transform.position;
    }

    void OnDisable()
    {
        m_pointerDown.Disable();
        m_pointerPosition.Disable();
    }


    void Update()
    {
        bool isMoving = false;

        if (m_isPointerDown)
        {
            isMoving = UpdateVelocity();
        }

        if (!isMoving)
        {
            m_velocity = Vector2.MoveTowards(m_velocity, Vector2.zero, dragWhenNotMoving * Time.deltaTime);
        }
        
        transform.position += (Vector3)m_velocity * Time.deltaTime;
    }

    bool UpdateVelocity()
    {
        Vector2 currentWorldPosition = headCamera.ScreenToWorldPoint(m_pointerPosition.ReadValue<Vector2>());
        Vector2 delta = currentWorldPosition - (Vector2)transform.position;

        Quaternion rotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90);

        transform.rotation = Quaternion.RotateTowards(rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (delta.sqrMagnitude > 0.01f)
        {
            Vector2 clamedDelta = Vector2.ClampMagnitude(delta, clamedMagnitude);

            Vector2 targetVelocity = clamedDelta * speed;
            m_velocity = Vector2.MoveTowards(m_velocity, targetVelocity, accelerationSpeed * Time.deltaTime);

            dragPoint.position = (Vector2)transform.position + clamedDelta;

            int segments = dragLine.positionCount - 1;
            Vector2 segementDelta = clamedDelta / segments;
            dragLine.SetPosition(0, transform.position);

            for (int i = 1; i <= segments; i++)
            {
                dragLine.SetPosition(i, (Vector2)transform.position + segementDelta * i);
            }

            return true;
        }

        return false;
    }

    void onPointerDown(CallbackContext context)
    {
        m_isPointerDown = true;
        dragLine.enabled = true;
        fireEffect.SetActive(true);
        smokeEffect.Play();
        UpdateVelocity();
    }

    void onPointerUp(CallbackContext context)
    {
        m_isPointerDown = false;
        dragLine.enabled = false;
        fireEffect.SetActive(false);
        smokeEffect.Stop();
        dragPoint.position = transform.position;
    }
}
