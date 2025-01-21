using UnityEngine;


namespace MPack
{
    [CreateAssetMenu(menuName="MPack/Variable/Int", order=0)]
    public class IntVariable : ScriptableObject
    {
        public int Value;

#if UNITY_EDITOR
        [TextArea]
        public string Note;

        public ValueWithEnable<int> EditorOverrideResetValue;
#endif

        public event System.Action<int> OnChanged;

        public void SetValue(int value)
        {
            Value = value;
            OnChanged?.Invoke(Value);
        }

        public void AddValue(int value)
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