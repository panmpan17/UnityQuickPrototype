using UnityEngine;


namespace MPack
{
    [CreateAssetMenu(menuName="MPack/Variable/Bool", order=0)]
    public class BoolVariable : ScriptableObject
    {
        // [System.NonSerialized]
        public bool Value;

#if UNITY_EDITOR
        [TextArea]
        public string Note;

        [SerializeField, InspectorButton("Set True", "SetToTrue")]
        private bool _setTrue;
        [SerializeField, InspectorButton("Set False", "SetToFalse")]
        private bool _setFalse;
#endif

        public event System.Action<bool> OnChanged;

        public void SetValue(bool value)
        {
            if (value == Value)
                return;

            Value = value;
            OnChanged?.Invoke(value);
        }

        public void Toggle()
        {
            Value = !Value;
            OnChanged?.Invoke(Value);
        }

        public void SetValueForceBoardcastChange(bool value)
        {
            if (value == Value)
            {
                OnChanged?.Invoke(value);
                return;
            }

            Value = value;
            OnChanged?.Invoke(value);
        }


#if UNITY_EDITOR
        void SetToTrue() => SetValue(true);
        void SetToFalse() => SetValue(false);

        public static BoolVariable AssetDatabaseGet(string fileName)
        {
            var guids = UnityEditor.AssetDatabase.FindAssets($"{fileName} t:BoolVariable");
            if (guids.Length == 0)
                return null;

            return UnityEditor.AssetDatabase.LoadAssetAtPath<BoolVariable>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
        }
#endif
    }
}