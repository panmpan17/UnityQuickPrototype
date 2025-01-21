using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MPack
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple=true)]
    public class InspectorButtonAttribute : PropertyAttribute
    {
        public string Name;
        public string MethodName;

        public InspectorButtonAttribute(string name, string methodName)
        {
            Name = name;
            MethodName = methodName;
        }
    }



#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InspectorButtonAttribute))]
    public class InspectorButtonPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 20;
        }

        InspectorButtonAttribute _buttonAttribute
        {
            get
            {
                return (InspectorButtonAttribute)attribute;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (GUI.Button(position, _buttonAttribute.Name))
            {
                var obj = property.serializedObject.targetObject;
                var type = obj.GetType();
                var method = type.GetMethod(_buttonAttribute.MethodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                method.Invoke(obj, null);
            }
        }
    }
#endif
}