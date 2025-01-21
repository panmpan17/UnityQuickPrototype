using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName="MPack/Camera Pointer")]
public class CameraPointer : ScriptableObject
{
#if UNITY_EDITOR
    [TextArea]
    public string Note;
#endif

    public Camera Target {
        get => _target;
        set {
            _target = value;
            OnChange?.Invoke(value);
        }
    }
    private Camera _target;
    public event System.Action<Camera> OnChange;

    public bool HasTarget => Target != null;

    public float FieldOfView => Target ? Target.fieldOfView : -1;

    public Vector3 WorldToScreenPoint(Vector3 position) => Target.WorldToScreenPoint(position);
    public Vector3 WorldToViewportPoint(Vector3 position) => Target.WorldToViewportPoint(position);
    public Vector3 ScreenToWorldPoint(Vector3 position) => Target.ScreenToWorldPoint(position);
    public Vector3 ViewportToWorldPoint(Vector3 position) => Target.ViewportToWorldPoint(position);
    public Ray ScreenPointToRay(Vector3 position) => Target.ScreenPointToRay(position);
    public Ray ViewportPointToRay(Vector3 position) => Target.ViewportPointToRay(position);

    public Ray ScreenCenterRay()
    {
        return new Ray(Target.transform.position, Target.transform.forward);
    }
}
