using UnityEngine;


namespace MPack
{
    [CreateAssetMenu(menuName="MPack/Variable/Range", order=0)]
    public class RangeVariable : ScriptableObject
    {
        public float Min;
        public float Max;

#if UNITY_EDITOR
        [TextArea]
        public string Note;

        public ValueWithEnable<float> EditorOverrideResetMin;
        public ValueWithEnable<float> EditorOverrideResetMax;
#endif

        public event System.Action<float, float> OnChanged;

        public void SetValue(float min, float max)
        {
            Min = min;
            Max = max;
            OnChanged?.Invoke(Min, Max);
        }

        public void AddValue(float min, float max)
        {
            Min += min;
            Max += max;
            OnChanged?.Invoke(Min, Max);
        }

        public void ResetValue(bool notify=true)
        {
            Min = 0;
            Max = 0;
#if UNITY_EDITOR
            if (EditorOverrideResetMin.Enable)
                Min = EditorOverrideResetMin.Value;
            if (EditorOverrideResetMax.Enable)
                Max = EditorOverrideResetMax.Value;
#endif
            if (notify)
                OnChanged?.Invoke(Min, Max);
        }
    }
}