using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BasicUnit : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Components")]
    [SerializeField]
    private new Rigidbody2D rigidbody2D;
    [SerializeField]
    private CircleCollider2D circleCollider2D;
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private bool spriteIsFacingRight = true;
    private Transform _spriteTransform;
    private bool _isCurrentlyFacingRight;

    [Header("Drag")]
    [SerializeField]
    private float maxDragDistance = 5f;
    [SerializeField]
    private float dragForce = 10f;
    [SerializeField]
    private LineRenderer lineRenderer;

    private Vector3 _startDragPosition;
    private Vector3 _lastDragPosition;

    void Awake()
    {
        lineRenderer.enabled = false;
        _spriteTransform = spriteRenderer.transform;
    }

    void LateUpdate()
    {
        bool newSpriteIsFacingRight = rigidbody2D.linearVelocity.x > 0;
        if (!spriteIsFacingRight)
            newSpriteIsFacingRight = !newSpriteIsFacingRight;

        if (newSpriteIsFacingRight != _isCurrentlyFacingRight)
        {
            _isCurrentlyFacingRight = newSpriteIsFacingRight;
            float x = Mathf.Abs(_spriteTransform.localScale.x);

            if (_isCurrentlyFacingRight)
            {
                if (!spriteIsFacingRight)
                    x = -x;
            }
            else
            {
                if (spriteIsFacingRight)
                    x = -x;
            }

            _spriteTransform.localScale = new Vector3(x, _spriteTransform.localScale.y, _spriteTransform.localScale.z);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        lineRenderer.enabled = true;

        _startDragPosition = transform.TransformPoint(circleCollider2D.offset);
        _lastDragPosition = _startDragPosition;
        lineRenderer.SetPositions(new Vector3[] { _startDragPosition, _startDragPosition });
        // Debug.Log("OnBeginDrag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 worldPosition = eventData.pointerCurrentRaycast.worldPosition;
        worldPosition.z = transform.position.z;

        Vector2 dragDistance = worldPosition - _startDragPosition;
        if (dragDistance.magnitude > maxDragDistance)
        {
            worldPosition = _startDragPosition + (Vector3)(dragDistance.normalized * maxDragDistance);
        }

        _lastDragPosition = worldPosition;

        lineRenderer.SetPosition(1, worldPosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        lineRenderer.enabled = false;

        rigidbody2D.linearVelocity = -(_lastDragPosition - _startDragPosition) * dragForce;
    }
}
