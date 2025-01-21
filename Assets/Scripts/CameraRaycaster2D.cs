using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CameraRaycaster2D : MonoBehaviour
{
    [SerializeField]
    private new Camera camera;
    [SerializeField]
    private LayerMask layerMask;
    [SerializeField]
    private float mouseStartDragThreshold = 0.1f;

    private GameObject _lastObject;
    private bool _isPressed;
    private bool _isDragged;

    private Vector2 _startClickMousePosition;
    private Vector2 _lastMousePosition;

    
    private bool IsPressed => Mouse.current.leftButton.isPressed;
    // private InputAction

    PointerEventData _pointerEventData;

    void Start()
    {
        _pointerEventData = new PointerEventData(EventSystem.current);
    }

    void Update()
    {
        // ray = new Ray(camera.transform.position, camera.transform.forward);
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Vector3 worldPosition = camera.ScreenToWorldPoint(mousePosition);
        
        if (_lastObject && _isPressed)
        {
            if (_isDragged)
            {
                UpdateDrag(mousePosition, worldPosition);
                return;
            }
            else
            {
                Vector2 distance = mousePosition - _startClickMousePosition;

                if (distance.magnitude > mouseStartDragThreshold)
                {
                    _isDragged = true;
                    _lastMousePosition = mousePosition;

                    _pointerEventData.dragging = true;
                    _pointerEventData.position = mousePosition;

                    var currentRaycastData = _pointerEventData.pointerCurrentRaycast;
                    currentRaycastData.worldPosition = worldPosition;
                    _pointerEventData.pointerCurrentRaycast = currentRaycastData;

                    if (_lastObject != null)
                        ExecuteEvents.Execute(_lastObject, _pointerEventData, ExecuteEvents.beginDragHandler);
                    return;
                }
            }
        }

        Collider2D collider = Physics2D.OverlapPoint(worldPosition, layerMask);
        if (collider != null)
        {
            var currentRaycastData = _pointerEventData.pointerCurrentRaycast;
            currentRaycastData.gameObject = collider.gameObject;
            currentRaycastData.worldPosition = worldPosition;
            currentRaycastData.worldNormal = Vector3.forward;

            _pointerEventData.pointerCurrentRaycast = currentRaycastData;


            if (_isPressed != IsPressed)
            {
                _isPressed = IsPressed;
                if (_isPressed)
                {
                    _startClickMousePosition = mousePosition;
                    _pointerEventData.pointerPressRaycast = currentRaycastData;
                    ExecuteEvents.Execute(_lastObject, _pointerEventData, ExecuteEvents.pointerDownHandler);
                }
                else
                {
                    if (_pointerEventData.pointerPressRaycast.gameObject != collider.gameObject)
                        HandlePointerReleaseButRaycastObjectIsNotTheSame();
                    else
                    {
                        ExecuteEvents.Execute(_lastObject, _pointerEventData, ExecuteEvents.pointerUpHandler);
                        ExecuteEvents.Execute(_lastObject, _pointerEventData, ExecuteEvents.pointerClickHandler);
                    }
                }
            }


            if (!_isPressed && collider.gameObject != _lastObject)
            {
                if (_lastObject != null)
                {
                    ExecuteEvents.Execute(_lastObject, _pointerEventData, ExecuteEvents.pointerExitHandler);
                }
                _lastObject = collider.gameObject;
                ExecuteEvents.Execute(_lastObject, _pointerEventData, ExecuteEvents.pointerEnterHandler);
            }
            // _isHoverd = true;
        }
        else
        {
            var currentRaycastData = _pointerEventData.pointerCurrentRaycast;
            currentRaycastData.gameObject = null;
            currentRaycastData.worldPosition = Vector3.zero;
            currentRaycastData.worldNormal = Vector3.zero;

            _pointerEventData.pointerCurrentRaycast = currentRaycastData;

            if (_isPressed)
            {
                if (!IsPressed)
                    HandlePointerReleaseButRaycastObjectIsNotTheSame();
                return;
            }

            if (_lastObject != null)
            {
                ExecuteEvents.Execute(_lastObject, _pointerEventData, ExecuteEvents.pointerExitHandler);
            }

            _lastObject = null;
        }
    }

    void UpdateDrag(Vector2 mousePosition, Vector3 worldPosition)
    {
        if (IsPressed)
        {
            Vector2 distance = mousePosition - _lastMousePosition;
            _lastMousePosition = mousePosition;

            _pointerEventData.position = mousePosition;

            var currentRaycastData = _pointerEventData.pointerCurrentRaycast;
            currentRaycastData.worldPosition = worldPosition;
            _pointerEventData.pointerCurrentRaycast = currentRaycastData;

            ExecuteEvents.Execute(_lastObject, _pointerEventData, ExecuteEvents.dragHandler);
        }
        else
        {
            _isPressed = false;
            _isDragged = false;
            _pointerEventData.dragging = false;
            _pointerEventData.pointerDrag = null;
            _pointerEventData.pointerPress = null;

            _pointerEventData.position = mousePosition;

            var currentRaycastData = _pointerEventData.pointerCurrentRaycast;
            currentRaycastData.worldPosition = worldPosition;
            _pointerEventData.pointerCurrentRaycast = currentRaycastData;

            ExecuteEvents.Execute(_lastObject, _pointerEventData, ExecuteEvents.endDragHandler);
        }
    }

    void HandlePointerReleaseButRaycastObjectIsNotTheSame()
    {
        _isPressed = false;
        ExecuteEvents.Execute(_lastObject, _pointerEventData, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(_lastObject, _pointerEventData, ExecuteEvents.pointerExitHandler);
        ExecuteEvents.Execute(_lastObject, _pointerEventData, ExecuteEvents.deselectHandler);
        _lastObject = null;
    }
}
