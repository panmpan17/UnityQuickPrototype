using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName="MPack/Transform Pointer")]
public class TransformPointer : ScriptableObject
{
#if UNITY_EDITOR
    [TextArea]
    public string Note;
#endif

    public Transform Target {
        get => _target;
        set {
            _target = value;
            OnChange?.Invoke(value);
        }
    }
    private Transform _target;
    public event System.Action<Transform> OnChange;

    public Transform[] Targets {
        get => _targets;
        set {
            _targets = value;
        }
    }
    private Transform[] _targets;

    public bool HasTarget => Target != null;

    public Vector3 Position => Target ? Target.position : Vector3.zero;
    public Quaternion Rotation => Target ? Target.rotation : Quaternion.identity;
    public Vector3 up => Target ? Target.up : Vector3.up;

    public float DistanceTo(Vector3 point) => Vector3.Distance(Position, point);

    public Vector3 TransformPoint(Vector3 point) => Target ? Target.TransformPoint(point) : point;
    public Vector3 InverseTransformPoint(Vector3 point) => Target.InverseTransformPoint(point);


    public void SyncPositionAndRotation(Transform target)
    {
        Transform t = Target;

        if (!t)
            return;

        target.SetPositionAndRotation(t.position, t.rotation);
    }

    public void SyncLocalPositionAndRotation(Transform target)
    {
        Transform t = Target;

        if (!t)
            return;

        target.SetLocalPositionAndRotation(t.localPosition, t.localRotation);
    }


#if UNITY_EDITOR
    public static TransformPointer AssetDatabaseGet(string fileName)
    {
        var guids = UnityEditor.AssetDatabase.FindAssets($"{fileName} t:TransformPointer");
        if (guids.Length == 0)
            return null;

        return UnityEditor.AssetDatabase.LoadAssetAtPath<TransformPointer>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
    }
#endif
}
