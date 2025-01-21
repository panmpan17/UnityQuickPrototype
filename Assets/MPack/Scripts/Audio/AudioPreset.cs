using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace MPack {
    [CreateAssetMenu(menuName="MPack/AudioPreset")]
    public class AudioPreset : ScriptableObject {
        public EnumToAudio[] Audios;

        [System.Serializable]
        public struct EnumToAudio {
            public AudioIDEnum Type;
            public AudioClip Clip;
            public float Volume;

            public EnumToAudio(AudioIDEnum type, AudioClip clip, float volume=1) {
                Type = type;
                Clip = clip;
                Volume = volume;
            }
        }

    #if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(EnumToAudio))]
        public class _PropertyDrawer : PropertyDrawer {
            public const float Height = 20;

            public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
            {
                rect.width /= 2;
                rect.height = Height - 2;
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("Type"), GUIContent.none);
                rect.x += rect.width;
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("Clip"), GUIContent.none);

                rect.y += Height;
                EditorGUI.Slider(rect, property.FindPropertyRelative("Volume"), 0f, 2f, GUIContent.none);
                // EditorGUI.PropertyField(rect, property.FindPropertyRelative("Volume"), GUIContent.none);
            }
        }

        [CustomEditor(typeof(AudioPreset))]
        public class _Editor : Editor {
            ReorderableList Audios;

            private void OnEnable() {
                Audios = new ReorderableList(serializedObject, serializedObject.FindProperty("Audios"));
                Audios.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Audios");
                Audios.elementHeightCallback = (index) => _PropertyDrawer.Height * 2;
                Audios.drawElementCallback = (rect, index, a, b) => {
                    EditorGUI.PropertyField(rect, Audios.serializedProperty.GetArrayElementAtIndex(index));
                };
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                Audios.DoLayoutList();

                serializedObject.ApplyModifiedProperties();
            }
        }
    #endif
    }
}