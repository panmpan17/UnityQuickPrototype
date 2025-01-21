using UnityEngine;


namespace MPack
{
    [CreateAssetMenu(menuName="MPack/Variable/Float", order=0)]
    public class FloatVariable : ScriptableObject
    {
        public float Value;

#if UNITY_EDITOR
        [TextArea]
        public string Note;

        public ValueWithEnable<float> EditorOverrideResetValue;
#endif

        public event System.Action<float> OnChanged;

        public void SetValue(float value)
        {
            Value = value;
            OnChanged?.Invoke(Value);
        }

        public void AddValue(float value)
        {
            Value += value;
            OnChanged?.Invoke(Value);
        }

        public void ResetValue(bool notify=true)
        {
            Value = 0;
#if UNITY_EDITOR
            if (EditorOverrideResetValue.Enable)
                Value = EditorOverrideResetValue.Value;
#endif
            if (notify)
                OnChanged?.Invoke(Value);
        }
    }
}