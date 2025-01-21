using UnityEngine;


namespace MPack
{
    [CreateAssetMenu(menuName="MPack/Variable/Color", order=0)]
    public class ColorVariable : ScriptableObject
    {
        public Color Value;

#if UNITY_EDITOR
        [TextArea]
        public string Note;

        public ValueWithEnable<Color> EditorOverrideResetValue;
#endif

        public event System.Action<Color> OnChanged;

        public void SetValue(Color value)
        {
            Value = value;
            OnChanged?.Invoke(Value);
        }

        public void ResetValue(bool notify=true)
        {
            Value = Color.clear;
#if UNITY_EDITOR
            if (EditorOverrideResetValue.Enable)
                Value = EditorOverrideResetValue.Value;
#endif
            if (notify)
                OnChanged?.Invoke(Value);
        }
    }
}