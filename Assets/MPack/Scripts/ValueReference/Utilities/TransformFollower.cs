using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPack
{
    public class TransformFollower : MonoBehaviour
    {
        [SerializeField]
            private Transform target;
            [SerializeField]
            private TransformPointer targetPointer;

            private Transform Target => targetPointer && targetPointer.Target ? targetPointer.Target : target;

            [SerializeField]
            private UpdateMode updateMode;

            [SerializeField]
            private bool updatePosition;
            [SerializeField, ShowIf("updatePosition", true)]
            private bool positionLockX;
            [SerializeField, ShowIf("updatePosition", true)]
            private bool positionLockY;
            [SerializeField, ShowIf("updatePosition", true)]
            private bool positionLockZ;
            [SerializeField, ShowIf("updatePosition", true)]
            private Vector3 positionOffset;

            [SerializeField]
            private bool updateRotation;
            [SerializeField, ShowIf("updateRotation", true)]
            private bool rotationLockX;
            [SerializeField, ShowIf("updateRotation", true)]
            private bool rotationLockY;
            [SerializeField, ShowIf("updateRotation", true)]
            private bool rotationLockZ;
            [SerializeField, ShowIf("updateRotation", true)]
            private Vector3 offset;
            [SerializeField, ShowIf("updateRotation", true)]
            private Vector3 rotationFixed;

            public enum UpdateMode { LateUpdate, FixedUpdate, Manual }

            void LateUpdate()
            {
                if (updateMode != UpdateMode.LateUpdate) return;
                Trigger();
            }

            void FixedUpdate()
            {
                if (updateMode != UpdateMode.FixedUpdate) return;
                Trigger();
            }

            public void Trigger()
            {
                Transform t = Target;
                if (!t) return;

                if (updatePosition)
                {

                    Vector3 position = transform.position;
                    if (!positionLockX) position.x = t.position.x;
                    if (!positionLockY) position.y = t.position.y;
                    if (!positionLockZ) position.z = t.position.z;
                    position += positionOffset;
                    transform.position = position;
                }

                if (updateRotation)
                {
                    Vector3 euler = rotationFixed;
                    if (!rotationLockX) euler.x = t.rotation.eulerAngles.x;
                    if (!rotationLockY) euler.y = t.rotation.eulerAngles.y;
                    if (!rotationLockZ) euler.z = t.rotation.eulerAngles.z;
                    euler += offset;
                    transform.rotation = Quaternion.Euler(euler);
                }
            }

            public void SetUpdateMode(UpdateMode mode)
            {
                updateMode = mode;
            }

            public void SetTarget(Transform target)
            {
                this.target = target;
            }
    }
}
