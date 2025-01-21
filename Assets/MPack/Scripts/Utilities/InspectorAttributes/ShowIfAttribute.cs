using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MPack
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple=true)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public string FieldName;

        public bool UseEnum;
        public bool DesiredBoolStatus;
        public int DesiredEnumStatus;

        public ShowIfAttribute(string fieldName, bool conditionValue)
        {
            UseEnum = false;
            FieldName = fieldName;
            DesiredBoolStatus = conditionValue;
        }

        public ShowIfAttribute(string fieldName, int conditionValue)
        {
            UseEnum = true;
            FieldName = fieldName;
            DesiredEnumStatus = conditionValue;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ShowIfAttribute), true)]
    public class ShowIfPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!GetConditionIsMatched(property.serializedObject, (ShowIfAttribute)attribute))
                return 0f;

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (position.height <= 0)
                return;

            EditorGUI.PropertyField(position, property, label, true);
        }

        bool GetConditionIsMatched(SerializedObject obj, ShowIfAttribute attribute)
        {
            SerializedProperty property = obj.FindProperty(attribute.FieldName);
            if (property == null)
                return false;

            if (attribute.UseEnum)
                return property.enumValueIndex == attribute.DesiredEnumStatus;
            else
                return property.boolValue == attribute.DesiredBoolStatus;
        }
    }
#endif
}