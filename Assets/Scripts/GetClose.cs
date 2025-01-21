using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetClose : MonoBehaviour
{
    [SerializeField]
    private TransformPointer playerPointer;
    [SerializeField]
    private float acceleration;
    [SerializeField]
    private float maxSpeed;

    private Rigidbody2D _rigidbody2D;
    private Vector2 _velocity;


    void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!playerPointer.Target) return;

        Vector2 playerPosition = playerPointer.Target.position;
        Vector2 currentPosition = transform.position;
        Vector2 direction = playerPosition - currentPosition;
        
        _velocity = _rigidbody2D.linearVelocity;
        _velocity += direction.normalized * acceleration * Time.deltaTime;

        if (_velocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            _velocity = _velocity.normalized * maxSpeed;
        }

        _rigidbody2D.linearVelocity = _velocity;
    }
}
