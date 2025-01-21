using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MPack;


public class ReferenceTarget : MonoBehaviour
{
    [SerializeField]
    private ReferenceType targetType = ReferenceType.Transform;
    public enum ReferenceType { Transform, Camera }

    [SerializeField, ShowIf("targetType", (int)ReferenceType.Transform)]
    private TransformPointer transformPointer;
    [SerializeField, ShowIf("targetType", (int)ReferenceType.Transform)]
    private Transform target;

    [SerializeField, ShowIf("targetType", (int)ReferenceType.Camera)]
    private CameraPointer cameraPointer;
    [SerializeField, ShowIf("targetType", (int)ReferenceType.Camera)]
    private Camera targetCamera;

    [SerializeField]
    private bool setOnEnable = true;

    void OnEnable()
    {
        if (setOnEnable)
            Set();
    }

    void OnDisable()
    {
        if (setOnEnable)
            Unset();
    }

    public void Set()
    {
        if (transformPointer) transformPointer.Target = target;
        if (cameraPointer) cameraPointer.Target = targetCamera;
    }
    public void Unset()
    {
        if (transformPointer) transformPointer.Target = null;
        if (cameraPointer) cameraPointer.Target = null;
    }
}
