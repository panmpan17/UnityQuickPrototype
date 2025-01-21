using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MPack
{
    public class EventDispatcher : MonoBehaviour
    {
        [SerializeField]
        private AbstractEventRefernece eventReference;
        [HideInInspector, SerializeField]
        private ParameterMode parameterMode;

        public UnityEvent Event;
        public BoolUnityEvent BoolEvent;
        public IntUnityEvent IntEvent;
        public FloatUnityEvent FloatEvent;
        public StringUnityEvent StringEvent;
        public Vector2UnityEvent Vector2Event;
        public Vector3UnityEvent Vector3Event;
        public GameObjectUnityEvent GameObjectEvent;

        void OnEnable()
        {
            eventReference.RegisterEvent(this);
        }
        void OnDisable()
        {
            eventReference.UnregisterEvent(this);
        }

        public void DispatchEvent() => Event.Invoke();
        public void DispatchEventWithBool(bool parameter) => BoolEvent.Invoke(parameter);
        public void DispatchEventWithInt(int parameter) => IntEvent.Invoke(parameter);
        public void DispatchEventWithFloat(float parameter) => FloatEvent.Invoke(parameter);
        public void DispatchEventWithString(string parameter) => StringEvent.Invoke(parameter);
        public void DispatchEventWithVector2(Vector2 parameter) => Vector2Event.Invoke(parameter);
        public void DispatchEventWithVector3(Vector3 parameter) => Vector3Event.Invoke(parameter);
        public void DispatchEventWithGameObject(GameObject parameter) => GameObjectEvent.Invoke(parameter);

        public enum ParameterMode {
            NotAssigned,
            None,
            Bool,
            Int,
            Float,
            String,
            Vector2,
            Vector3,
            GameObject
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(EventDispatcher))]
        public class _Editor : Editor
        {
            SerializedProperty eventProperty;
            ParameterMode mode = ParameterMode.NotAssigned;
            EventDispatcher dispatcher;

            void OnEnable()
            {
                dispatcher = (EventDispatcher) target;
                eventProperty = serializedObject.FindProperty("eventReference");

                AbstractEventRefernece _event = (AbstractEventRefernece)eventProperty.objectReferenceValue;
                mode = ChangeEventDisplayMode(_event);
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(eventProperty);

                if (EditorGUI.EndChangeCheck())
                {
                    AbstractEventRefernece _event = (AbstractEventRefernece)eventProperty.objectReferenceValue;
                    mode = ChangeEventDisplayMode(_event);
                }

                switch (mode)
                {
                    case ParameterMode.None:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Event"));
                        break;
                    case ParameterMode.Bool:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("BoolEvent"));
                        break;
                    case ParameterMode.Int:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("IntEvent"));
                        break;
                    case ParameterMode.Float:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("FloatEvent"));
                        break;
                    case ParameterMode.String:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("StringEvent"));
                        break;
                    case ParameterMode.Vector2:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Vector2Event"));
                        break;
                    case ParameterMode.Vector3:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Vector3Event"));
                        break;
                    case ParameterMode.GameObject:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("GameObjectEvent"));
                        break;
                }

                serializedObject.ApplyModifiedProperties();
            }

            ParameterMode ChangeEventDisplayMode(AbstractEventRefernece abstractEventRefernece)
            {
                if (abstractEventRefernece == null)
                {
                    return ParameterMode.NotAssigned;
                }

                if (abstractEventRefernece is EventReference)
                    return ParameterMode.None;
                if (abstractEventRefernece is BoolEventReference)
                    return ParameterMode.Bool;
                if (abstractEventRefernece is IntEventReference)
                    return ParameterMode.Int;
                if (abstractEventRefernece is FloatEventReference)
                    return ParameterMode.Float;
                if (abstractEventRefernece is StringEventReference)
                    return ParameterMode.String;
                if (abstractEventRefernece is Vector2EventReference)
                    return ParameterMode.Vector2;
                if (abstractEventRefernece is Vector3EventReference)
                    return ParameterMode.Vector3;
                if (abstractEventRefernece is GameObjectEventReference)
                    return ParameterMode.GameObject;

                return ParameterMode.NotAssigned;
            }
        }
#endif
    }

    [System.Serializable]
    public class BoolUnityEvent : UnityEvent<bool> { }
    [System.Serializable]
    public class IntUnityEvent : UnityEvent<int> { }
    [System.Serializable]
    public class FloatUnityEvent : UnityEvent<float> { }
    [System.Serializable]
    public class StringUnityEvent : UnityEvent<string> { }
    [System.Serializable]
    public class Vector2UnityEvent : UnityEvent<Vector2> { }
    [System.Serializable]
    public class Vector3UnityEvent : UnityEvent<Vector3> { }
    [System.Serializable]
    public class GameObjectUnityEvent : UnityEvent<GameObject> { }
}