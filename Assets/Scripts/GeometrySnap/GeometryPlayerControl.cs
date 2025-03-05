using UnityEngine;
using UnityEngine.InputSystem;

public class GeometryPlayerControl : MonoBehaviour
{
    [SerializeField]
    private SnapPart baseSnapPart;

    [SerializeField]
    private float rotateSpeed = 1.0f;
    [SerializeField]
    private float moveSpeed = 1.0f;

    private InputScheme m_inputScheme;

    private void Awake()
    {
        m_inputScheme = new InputScheme();
        m_inputScheme.Enable();

        m_inputScheme.Player.Attack.performed += OnAttackPerfermed;
        m_inputScheme.Player.Attack.canceled += OnAttackCanceled;
    }

    private void Update()
    {
        Vector2 move = m_inputScheme.Player.Move.ReadValue<Vector2>();

        transform.position += new Vector3(move.x, move.y, 0) * moveSpeed * Time.deltaTime;

        if (move == Vector2.zero)
        {
            return;
        }
        float zRotation = Mathf.Atan2(move.y, move.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, zRotation);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }

    void OnAttackPerfermed(InputAction.CallbackContext context)
    {
        baseSnapPart.AttractOtherParts();
    }

    void OnAttackCanceled(InputAction.CallbackContext context)
    {
        Debug.Log("OnAttackCanceled");
        baseSnapPart.StopAttracting();
    }
}
